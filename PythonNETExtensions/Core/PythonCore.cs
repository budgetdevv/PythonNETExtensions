using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Python.Runtime;
using PythonNETExtensions.Helpers;
using PythonNETExtensions.Modules;
using PythonNETExtensions.PythonConfig;
using PythonNETExtensions.PythonVersions;

namespace PythonNETExtensions.Core
{
    internal static class PythonInstance
    {
        private static int InstanceCount = 0;

        public static void OnCreate()
        {
            if (Interlocked.Increment(ref InstanceCount) == 1)
            {
                return;
            }

            throw new Exception("There may only be 1 PythonCore instance!");
        }
    }
    
    public class PythonCore<PyVersionT, PyConfigT>
        where PyVersionT: struct, IPythonVersion<PyVersionT>
        where PyConfigT: struct, IPythonConfig<PyConfigT>
    {
        public static readonly PythonCore<PyVersionT, PyConfigT> INSTANCE = new PythonCore<PyVersionT, PyConfigT>();

        static PythonCore()
        {
            PythonInstance.OnCreate();
        }
        
        private PythonCore() { }
        
        private static readonly HttpClient HTTP_CLIENT = new HttpClient();
        
        private enum InitializationState: int
        {
            Uninitialized = 0,
            Initialized = -1
        }

        private static int IsInitialized = (int) InitializationState.Uninitialized;
        
        private static bool TryInitialize()
        {
            var oldVal = (InitializationState) Interlocked.CompareExchange(ref IsInitialized, (int) InitializationState.Initialized, (int) InitializationState.Uninitialized);

            return oldVal == InitializationState.Uninitialized;
        }

        public Task InitializeAsync()
        {
            return InitializeAsyncInternal();
        }
        
        // AggressiveInlining - Inline into instance method
        // AggressiveOptimization - Only ran once during startup, will never have the chance to tier up
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static async Task InitializeAsyncInternal()
        {
            TryInitialize();
            
            var pythonBundleDirectory = PyConfigT.PythonHomePath;

            if (Directory.Exists(pythonBundleDirectory))
            {
                Console.WriteLine("Existing python bundle found!");
                goto SkipDownload;
            }
            var pythonBundleZipPath = $"{PyConfigT.PythonBundleContainingDirectory}PythonBundle.zip";
            
            var pythonBundleZipStream = File.OpenWrite(pythonBundleZipPath);

            if (true)
            {
                Console.WriteLine("Downloading python bundle...");
            
                await HTTP_CLIENT.DownloadFileAsync(PyVersionT.GetPlatformEmbeddable().GetDownloadURLForCurrentArch(), pythonBundleZipStream, progress: new Progress<float>(p => Console.WriteLine($"Download progress: {p * 100}%")));
                
                Console.WriteLine("Download complete!");
            }

            await pythonBundleZipStream.DisposeAsync();
            
            Directory.CreateDirectory(pythonBundleDirectory);
            
            pythonBundleZipStream = File.OpenRead(pythonBundleZipPath);

            Console.WriteLine("Unzipping python bundle...");

            await pythonBundleZipStream.UnzipAsync(pythonBundleDirectory);

            await pythonBundleZipStream.DisposeAsync();
            
            Console.WriteLine("Unzip complete!");
            
            Console.WriteLine("Deleting temp files...");
            
            File.Delete(pythonBundleZipPath);
            
            Console.WriteLine("Delete complete!");
            
            SkipDownload:
            Runtime.PythonDLL = PyVersionT.GetDLLPath(pythonBundleDirectory);
            // This must NOT be reordered before Runtime.PythonDLL, as there's a dependency on it.
            PythonEngine.PythonHome = pythonBundleDirectory;
            
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
        }

        public static string[] PythonPackages { get; private set; }
        
        public Task InitializeDependentPackages()
        {
            return InitializeDependentPackagesInternal();
        }
        
        // AggressiveInlining - Inline into instance method
        // AggressiveOptimization - Only ran once during startup, will never have the chance to tier up
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static async Task InitializeDependentPackagesInternal()
        {
            if ((InitializationState) IsInitialized == InitializationState.Uninitialized)
            {
                throw new Exception($"Please run {nameof(InitializeAsync)}() first!");
            }
            
            // Apparently this causes stackoverflow when PythonExtensions.GetCachedPythonModule<>() is invoked...
            // // TODO: Consider asynchronous awaiting of pip process
            // await Task.Yield();
            
            var pythonModuleBaseType = typeof(IPythonModuleBase);

            Assembly[] assemblies = [ Assembly.GetEntryAssembly(), typeof(PythonCore<PyVersionT, PyConfigT>).Assembly ];

            var modules = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsValueType && x.GetInterfaces().Contains(pythonModuleBaseType))
                .ToArray();

            var bindingFlags = BindingFlags.Static | BindingFlags.Public;

            var packages = new HashSet<string>(modules.Length);

            var homePath = PyConfigT.PythonHomePath;

            var pipExecutablePath = PyVersionT.GetPipExecutablePath(homePath);
            var pipPackagesPath = PyVersionT.GetPackagesPath(homePath);

            // Initializing the module cache uses Py code, which means we need to take the GIL
            using (new PythonHandle())
            {
                foreach (var module in modules)
                {
                    // Has nothing to do with MainModule - it is just a generic argument required to satisfy the compiler
                    var packageName = Unsafe.As<string>(module.GetProperty(nameof(IPythonModule<MainModule>.DependentPackage), bindingFlags)!.GetValue(null));
                
                    // TODO: Are pip packages case insensitive?
                    if (!string.IsNullOrWhiteSpace(packageName))
                    {
                        InstallPackage(packageName, pipExecutablePath, pipPackagesPath);
                    }
                    
                    // Initialize module cache
                    RuntimeHelpers.RunClassConstructor(typeof(IPythonModule<>).MakeGenericType(module).TypeHandle);
                }
            }

            PythonPackages = packages.ToArray();
        }
        
        // AggressiveInlining - Inline into InitializeDependentPackagesInternal()
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstallPackage(string packageName, string pipExecutablePath, string pipPackagesPath)
        {
            Console.WriteLine($"Installing {packageName}...");

            var span = pipExecutablePath.AsSpan();
            var index =span.LastIndexOf('/');
            
            // Define the process start information
            var startInfo = new ProcessStartInfo
            {
                FileName = span.Slice(index + 1).ToString(),
                Arguments = $"install {packageName} --target {pipPackagesPath}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = span.Slice(0, index).ToString()
            };

            // Create and start the process
            using (var process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    Console.WriteLine("Process could not be started. Ensure Python is installed and accessible.");
                    return;
                }
            
                // Read the output
                string output = process.StandardOutput.ReadToEnd();
                string err = process.StandardError.ReadToEnd();

                process.WaitForExit(); // Wait for the process to exit

                // Optionally: Display the output
                Console.WriteLine(output);
                if (!string.IsNullOrEmpty(err))
                {
                    Console.WriteLine("Errors during installation:");
                    Console.WriteLine(err);
                }
                else
                {
                    Console.WriteLine($"{packageName} installation was successful.");
                }
            }
        }
    }
}