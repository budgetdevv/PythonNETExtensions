using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Python.Runtime;

namespace PythonNETExtensions
{
    public static class PythonCore
    {
        private const string CONFIG_PATH = "PythonExtsConfig.json",
                             CONFIG_BACKUP_PATH = "PythonExtsConfig_BAK.json";

        private static readonly JsonSerializerOptions J_OPTS = new JsonSerializerOptions()
        {
            IncludeFields = true,
            WriteIndented = true
        };
        
        private struct Config
        {
            public bool BundlePython = true;
            public string VirtualEnvironmentDirectory = "./venv";

            public Config() { }
        }
        
        public static async Task InitializeAsync()
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
            
            if (config.BundlePython)
            {
                var venvPath = config.VirtualEnvironmentDirectory;

                if (venvPath == null)
                {
                    configException = new NullReferenceException($"{nameof(config.VirtualEnvironmentDirectory)} may not be null!");
                    goto CorruptedConfig;
                }

                string dllName;

                if (OperatingSystem.IsWindows())
                {
                    dllName = "python311.dll";
                }
            
                else if (OperatingSystem.IsLinux())
                {
                    dllName = "libpython3.11.so";
                }

                else if (OperatingSystem.IsMacOS())
                {
                    dllName = "libpython3.11.dylib";
                }

                else
                {
                    throw new PlatformNotSupportedException();
                }
                
                var dllPath = Runtime.PythonDLL = $"{venvPath}/{dllName}";

                if (!Directory.Exists(venvPath))
                {
                    Directory.CreateDirectory(venvPath);
                    goto Download;
                }

                if (File.Exists(dllPath))
                {
                    goto DownloadComplete;
                }
                
                Download:
                // var packagesDirectory = $"{venvPath}/site-packages";
                
                var dllStream = File.Open(dllPath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                
                const string DOWNLOAD_PREFIX = "https://github.com/budgetdevv/PythonNETExtensions/raw/main/DLLs";

                var downloadURL = $"{DOWNLOAD_PREFIX}/{dllName}";
                
                var httpClient = new HttpClient();
                
                Console.WriteLine(downloadURL);
                Console.WriteLine(venvPath);
                
                await httpClient.DownloadFileAsync(downloadURL, dllStream);

                DownloadComplete:
                Console.WriteLine(dllPath);
                Console.WriteLine(venvPath);
                
                PythonEngine.PythonHome = venvPath;
            }
            
            PythonEngine.Initialize();
            PythonEngine.BeginAllowThreads();
            
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