using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Python.Runtime;
using PythonNETExtensions.Helpers;

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
            public string PythonBundleDirectory = "./";
            public string VirtualEnvironmentDirectory = "./";

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

            var httpClient = new HttpClient();

            var pythonBundleDirectory = config.PythonBundleDirectory;

            if (pythonBundleDirectory == null)
            {
                configException = new NullReferenceException(nameof(pythonBundleDirectory));
                goto CorruptedConfig;
            }
            
            pythonBundleDirectory = $"{pythonBundleDirectory}/{PYTHON_BUNDLE_NAME}";

            if (Directory.Exists(pythonBundleDirectory))
            {
                Console.WriteLine("Existing python bundle found!");
                goto SkipDownload;
            }
            var pythonBundleZipPath = $"{pythonBundleDirectory}.zip";
            
            var pythonBundleZipStream = File.OpenWrite(pythonBundleZipPath);

            string pythonBundleDownloadURL;

            if (true)
            {
                // All bundles are 3.11, for now.
                if (OperatingSystem.IsMacOS())
                {
                    pythonBundleDownloadURL = "https://drive.usercontent.google.com/download?id=1v-ddEOnkNszZGhXmh1UQjCvYfRAfS4Kc&export=download&authuser=1&confirm=t&uuid=d9817ef1-7a5f-41ce-8b9d-1cdc88f07641&at=APZUnTUxuSG3nFARelCdTsSvPfoi%3A1710318036536";
                }
            
                else if (OperatingSystem.IsWindows())
                {
                    pythonBundleDownloadURL = "";
                }

                else if (OperatingSystem.IsLinux())
                {
                    pythonBundleDownloadURL = "";
                }

                else
                {
                    throw new PlatformNotSupportedException();
                }
                
                Console.WriteLine("Downloading python bundle...");
            
                await httpClient.DownloadFileAsync(pythonBundleDownloadURL, pythonBundleZipStream, progress: new Progress<float>(p => Console.WriteLine($"Download progress: {p * 100}%")));
                
                Console.WriteLine("Download complete!");
            }

            await pythonBundleZipStream.DisposeAsync();
            
            Directory.CreateDirectory(pythonBundleDirectory);
            
            pythonBundleZipStream = File.OpenRead(pythonBundleZipPath);

            Console.WriteLine("Unzipping python bundle...");

            await pythonBundleZipStream.UnzipAsync(pythonBundleDirectory);
            
            Console.WriteLine("Unzip complete!");
            
            SkipDownload:
            Runtime.PythonDLL = $"{pythonBundleDirectory}/Python";
            
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