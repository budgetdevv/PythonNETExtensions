using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PythonNETExtensions.Config;
using PythonNETExtensions.Core;
using PythonNETExtensions.Core.Handles;
using PythonNETExtensions.Modules;
using PythonNETExtensions.Modules.PythonBuiltIn;
using PythonNETExtensions.Versions;

namespace SampleCode
{
    internal static class Program
    {
        private struct Numpy: IPythonModule<Numpy>
        {
            public static string DependentPackage => "numpy";
            public static string ModuleName => DependentPackage;
        }

        private static async Task Main(string[] args)
        {
            var task = Sample();
            
            // Simulate CPU-bound work
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.WriteLine("AsyncIO.Sleep() is non-blocking!");
            
            await task;
        }
        
        private static async Task Sample()
        {
            var pythonCore = new PythonCoreBuilder()
                .WithConfig<DefaultPythonConfig>()
                .WithVersion<PyVer3_11<DefaultPythonConfig>>()
                .Build();
            
            await pythonCore.InitializeAsync();
            await pythonCore.InitializeDependentPackages();
            
            using (new PythonHandle())
            {
                const string HELLO_WORLD_TEXT = "Hello World!";

                var sys = PythonModule.Get<SysModule>();

                var result = RawPython.Run<string>(
                $"""
                print({HELLO_WORLD_TEXT:py});
                return {sys:py}.executable;
                """);
                
                Console.WriteLine(result);
                
                // Numpy module
                var np = PythonModule.Get<Numpy>();
                Console.WriteLine(np.array((int[]) [ 1, 2, 3, 4, 5 ]));
            }

            using (var handle = AsyncPythonHandle.Create())
            {
                pythonCore.SetupAsyncIO();
            
                var asyncIO = PythonModule.GetConcrete<AsyncIOModule>();

                const int DELAY_SECONDS = 2;
                
                Debug.Assert(DELAY_SECONDS >= 2);
                
                var awaiter = RawPython.RunAsync
                (
                $"""
                print("{nameof(asyncIO)} is running!");
                await {asyncIO.Sleep(DELAY_SECONDS):py};
                """, handle);

                await awaiter;
            
                Console.WriteLine($"Hello after {DELAY_SECONDS} seconds");

                var threadedTask = Task.Run(() =>
                {
                    using (new PythonHandle())
                    {
                        Console.WriteLine("Threaded task running when long-running C# code is");
                    }
                });
                
                LongRunningCSharpCode();

                return;
                
                void LongRunningCSharpCode()
                {
                    using (handle.GetLongRunningCSharpRegion())
                    {
                        Console.WriteLine("Start of long-running C# code");

                        Thread.Sleep(1000);
                        
                        Console.WriteLine("End of long-running C# code");
                    }
                }
            }
        }
    }
}