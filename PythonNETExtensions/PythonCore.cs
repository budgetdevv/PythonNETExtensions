using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Python.Runtime;

namespace PythonNETExtensions
{
    public static class PythonCore
    {
        private const string CONFIG_PATH = "/PythonExtsConfig.json",
                             CONFIG_BACKUP_PATH = "/PythonExtsConfig_BAK.json";

        private static readonly JsonSerializerOptions J_OPTS = new JsonSerializerOptions()
        {
            IncludeFields = true,
            WriteIndented = true
        };
        
        private struct Config
        {
            public bool BundlePython = true;
            public string VirtualEnvironmentDirectory = "/venv";

            public Config() { }
        }
        
        [ModuleInitializer]
        public static void Initialize()
        {
            Config config;

            Exception configException;
            
            try
            {
                config = JsonSerializer.Deserialize<Config>(File.ReadAllText(CONFIG_PATH));
            }

            catch (Exception exception)
            {
                configException = exception;
                goto CorruptedConfig;
            }

            if (config.BundlePython)
            {
                var venvPath = PythonEngine.PythonHome = config.VirtualEnvironmentDirectory;

                if (venvPath == null)
                {
                    configException = new NullReferenceException($"{nameof(config.VirtualEnvironmentDirectory)} may not be null!");
                    goto CorruptedConfig;
                }

                string dllName;

                if (OperatingSystem.IsWindows())
                {
                    dllName = "python38.dll";
                }
            
                else if (OperatingSystem.IsLinux())
                {
                    dllName = "libpython3.8.so";
                }

                else if (OperatingSystem.IsMacOS())
                {
                    dllName = "libpython3.8.dylib";
                }

                else
                {
                    throw new PlatformNotSupportedException();
                }

                if (!Directory.Exists(venvPath))
                {
                    Directory.CreateDirectory(venvPath);
                    goto Download;
                }

                var dllPath = Runtime.PythonDLL = $"{venvPath}/{dllName}";

                if (File.Exists(dllPath))
                {
                    goto DownloadComplete;
                }
                
                Download:
                var packagesDirectory = $"{venvPath}/site-packages";

                var packagesDirectoryStream = File.Open(packagesDirectory, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                
                const string DOWNLOAD_PREFIX = "https://github.com/budgetdevv/PythonNETExtensions/raw/main/DLLs";

                var downloadURL = $"{DOWNLOAD_PREFIX}/{dllName}";
                
                var httpClient = new HttpClient();

                httpClient.DownloadWithProgressAsync(downloadURL, packagesDirectoryStream).Wait();
            }
            
            DownloadComplete:
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
            
            File.WriteAllText(CONFIG_PATH, JsonSerializer.Serialize<Config>(new Config()));
            Environment.Exit(0);
        }
    }
}