using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Python.Runtime;
using PythonNETExtensions.AsyncIO;
using PythonNETExtensions.Config;
using PythonNETExtensions.Core.Handles;
using PythonNETExtensions.Helpers;
using PythonNETExtensions.Modules;
using PythonNETExtensions.Modules.BuiltIn;
using PythonNETExtensions.Versions;

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
        where PyVersionT: struct, IPythonVersion<PyVersionT, PyConfigT>
        where PyConfigT: struct, IPythonConfig<PyConfigT>
    {
        public static readonly PythonCore<PyVersionT, PyConfigT> INSTANCE = new PythonCore<PyVersionT, PyConfigT>();

        static PythonCore()
        {
            PythonInstance.OnCreate();
        }
        
        private PythonCore() { }
        
        private static readonly HttpClient HTTP_CLIENT = new HttpClient();

        private static Initializer CoreInitializer, AsyncIOIntiializer;

        private static dynamic MainPythonThread;

        public Task InitializeAsync()
        {
            // It seems like async await code causes stack overflow exception when importing Py modules.
            // I suspect PythonEngine.Initialize() must be ran in the initial thread,
            // and crossing await boundaries does mess with that.
            
            // TODO: Find out why this happens.
            InitializeAsyncInternal().Wait();
            return Task.CompletedTask;
            
            // return InitializeAsyncInternal();
        }
        
        // AggressiveInlining - Inline into instance method
        // AggressiveOptimization - Only ran once during startup, will never have the chance to tier up
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static async Task InitializeAsyncInternal()
        {
            if (!CoreInitializer.TryInitialize())
            {
                return;
            }
            
            var pythonBundleDirectory = PyConfigT.PythonHomePath;

            if (Directory.Exists(pythonBundleDirectory))
            {
                Console.WriteLine("Existing python bundle found!");
                goto SkipDownload;
            }

            var pythonBundleZipPath = Path.Combine(PyConfigT.PythonBundleContainingDirectory, "PythonBundle.zip");
            
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
            Runtime.PythonDLL = PyVersionT.GetDLLPath();
            // This must NOT be reordered before Runtime.PythonDLL, as there's a dependency on it.
            PythonEngine.PythonHome = pythonBundleDirectory;
            
            PythonEngine.Initialize();

            MainPythonThread = PythonModule.GetConcrete<ThreadingModule>().GetCurrentThread();
            
            PythonEngine.BeginAllowThreads();
        }

        public static string[] PythonPackages { get; private set; }
        
        public Task InitializeDependentPackages(bool force = false) => InitializeDependentPackagesInternal(force);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureInitialized()
        {
            if (!CoreInitializer.IsInitialized)
            {
                throw new Exception($"Please run {nameof(InitializeAsync)}() first!");
            }
        }
        
        // AggressiveInlining - Inline into instance method
        // AggressiveOptimization - Only ran once during startup, will never have the chance to tier up
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static async Task InitializeDependentPackagesInternal(bool force)
        {
            EnsureInitialized();
            
            var pythonModuleBaseType = typeof(IPythonModuleBase);

            Assembly[] assemblies = [ Assembly.GetEntryAssembly(), typeof(PythonCore<PyVersionT, PyConfigT>).Assembly ];

            var modules = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => x.IsValueType && x.GetInterfaces().Contains(pythonModuleBaseType))
                .ToArray();

            var bindingFlags = unchecked((BindingFlags) (-1)) & ~BindingFlags.Instance;

            var moduleCount = modules.Length;
            
            var packageNames = new HashSet<string>(moduleCount);
            
            var pipInstallNames = new List<string>(moduleCount);
            
            var moduleTypeHandles = new HashSet<RuntimeTypeHandle>(moduleCount);
            
            // Initializing the module cache uses Py code, which means we need to take the GIL
            using (new PythonHandle())
            {
                foreach (var module in modules)
                {
                    // Has nothing to do with MainModule - it is just a generic argument required to satisfy the compiler
                    // module.GetProperty() may return null, since Python built-in modules do not have explicit DependentPackage declaration
                    var packageName = Unsafe.As<string>(module
                        .GetProperty(nameof(IPythonModule<MainModule>.DependentPackage), bindingFlags)?
                        .GetValue(null)
                    );

                    // TODO: Are pip packages case insensitive?
                    if (!string.IsNullOrWhiteSpace(packageName))
                    {
                        packageNames.Add(packageName);
                        
                        var packageVersion = Unsafe.As<string?>(module
                            .GetProperties(bindingFlags)?
                            .FirstOrDefault(x => x.Name.Split('.').LastOrDefault() == nameof(IPythonModule<MainModule>.DependentPackageVersion))?
                            .GetValue(null)
                        );
                        
                        var pipInstallName = packageName;
                        
                        if (!string.IsNullOrWhiteSpace(packageVersion))
                        {
                            pipInstallName += $"=={packageVersion}";
                        }
                        
                        pipInstallNames.Add(pipInstallName);
                    }

                    moduleTypeHandles.Add(typeof(ModuleCache<>).MakeGenericType(module).TypeHandle);
                }

                PythonPackages = packageNames.ToArray();
                
                InstallPackages(pipInstallNames, force);

                foreach (var typeHandle in moduleTypeHandles)
                {
                    // Initialize module cache
                    RuntimeHelpers.RunClassConstructor(typeHandle);
                }

                IPythonModuleBase.OnModulesLoaded?.Invoke();
            }
        }
        
        // AggressiveInlining - Inline into InitializeDependentPackagesInternal()
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InstallPackages(List<string> packageNames, bool force)
        {
            Console.WriteLine($"Installing packages! [{packageNames.Count}]...");

            var forceReinstallText = force ? "--force-reinstall " : string.Empty;

            var command = $"-m pip install {forceReinstallText}{string.Join(' ', packageNames)}";

            Console.WriteLine(command);
            
            PyVersionT.RunWithPythonExecutable(command);
        }
        
        public void SetupAsyncIO() => SetupAsyncIOInternal();

        // AggressiveInlining - Inline into SetupAsyncIO()
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetupAsyncIOInternal()
        {
            EnsureInitialized();

            using (new PythonHandle())
            {
                RuntimeHelpers.RunClassConstructor(typeof(AsyncIOCore).TypeHandle);
            }
        }
    }
}