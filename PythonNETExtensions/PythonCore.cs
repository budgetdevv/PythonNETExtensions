using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Python.Runtime;
using PythonNETExtensions.Helpers;
using PythonNETExtensions.PythonVersions;

namespace PythonNETExtensions
{
    public static class PythonCore
    {
        private const string CONFIG_PATH = "PythonExtsConfig.json",
                             CONFIG_BACKUP_PATH = "PythonExtsConfig_BAK.json",
                             PYTHON_BUNDLE_NAME = "PythonBundle";

        private static readonly JsonSerializerOptions J_OPTS = new JsonSerializerOptions()
        {
            IncludeFields = true,
            WriteIndented = true
        };
        
        private struct Config
        {
            public string PythonBundleContainingDirectory = "./";

            public Config() { }
        }
        
        public static readonly HttpClient HTTP_CLIENT = new HttpClient();

        private static string PackagesPath;
        
        public static async Task InitializeAsync<PyVersionT>() where PyVersionT: struct, IPythonVersion
        {
            Config config;

            Exception configException;
            
            try
            {
                config = JsonSerializer.Deserialize<Config>(await File.ReadAllTextAsync(CONFIG_PATH), J_OPTS);
            }

            catch (Exception exception)
            {
                configException = exception;
                goto CorruptedConfig;
            }

            var platformEmbeddable = IPythonVersion.GetPlatformEmbeddable<PyVersionT>();

            var pythonBundleDirectory = config.PythonBundleContainingDirectory;

            if (pythonBundleDirectory == null)
            {
                configException = new NullReferenceException(nameof(pythonBundleDirectory));
                goto CorruptedConfig;
            }
            
            pythonBundleDirectory = $"{pythonBundleDirectory}{PYTHON_BUNDLE_NAME}";

            if (Directory.Exists(pythonBundleDirectory))
            {
                Console.WriteLine("Existing python bundle found!");
                goto SkipDownload;
            }
            var pythonBundleZipPath = $"{pythonBundleDirectory}.zip";
            
            var pythonBundleZipStream = File.OpenWrite(pythonBundleZipPath);

            if (true)
            {
                Console.WriteLine("Downloading python bundle...");
            
                await HTTP_CLIENT.DownloadFileAsync(platformEmbeddable.GetDownloadURLForCurrentArch(), pythonBundleZipStream, progress: new Progress<float>(p => Console.WriteLine($"Download progress: {p * 100}%")));
                
                Console.WriteLine("Download complete!");
            }

            await pythonBundleZipStream.DisposeAsync();
            
            Directory.CreateDirectory(pythonBundleDirectory);
            
            pythonBundleZipStream = File.OpenRead(pythonBundleZipPath);

            Console.WriteLine("Unzipping python bundle...");

            await pythonBundleZipStream.UnzipAsync(pythonBundleDirectory);
            
            Console.WriteLine("Unzip complete!");
            
            Console.WriteLine("Deleting temp files...");
            
            File.Delete(pythonBundleZipPath);
            
            Console.WriteLine("Delete complete!");
            
            SkipDownload:
            Runtime.PythonDLL = IPythonVersion.GetDLLPath<PyVer3_11>(pythonBundleDirectory);
            // This must NOT be reordered before Runtime.PythonDLL, as there's a dependency on it.
            PythonEngine.PythonHome = pythonBundleDirectory;
            
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();

            PackagesPath = IPythonVersion.GetPackagesPath<PyVer3_11>(pythonBundleDirectory);
            
            return;
            
            CorruptedConfig:
            Console.WriteLine(
                $"""
                 Config corrupted! Exception: {configException}
                 Generating a new one!
                 """);

            if (File.Exists(CONFIG_PATH))
            {
                File.Move(CONFIG_PATH, CONFIG_BACKUP_PATH);
            }
            
            await File.WriteAllTextAsync(CONFIG_PATH, JsonSerializer.Serialize<Config>(new Config(), J_OPTS));
            Environment.Exit(0);
        }
    }
}