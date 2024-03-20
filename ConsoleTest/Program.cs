using System;
using System.Threading;
using System.Threading.Tasks;
using PythonNETExtensions.Config;
using PythonNETExtensions.Core;
using PythonNETExtensions.Modules;
using PythonNETExtensions.Modules.PythonBuiltIn;
using PythonNETExtensions.Versions;

namespace ConsoleTest
{
    internal static class Program
    {
        public struct Numpy: IPythonModule<Numpy>
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
            var pythonCore = PythonCore<PyVer3_11<DefaultPythonConfig>, DefaultPythonConfig>.INSTANCE;
            await pythonCore.InitializeAsync();
            await pythonCore.InitializeDependentPackages();
            
            using (new PythonHandle())
            {
                const string HELLO_WORLD_TEXT = "Hello World!";

                var sys = PythonExtensions.GetPythonModule<SysModule>();

                var result = RawPython.Run<string>(
                $"""
                print({HELLO_WORLD_TEXT:py});
                return {(object) sys:py}.executable;
                """);
                
                Console.WriteLine(result);
                
                // Numpy module
                var np = PythonExtensions.GetPythonModule<Numpy>();
                Console.WriteLine(np.array((int[]) [ 1, 2, 3, 4, 5 ]));
            }

            using (var handle = AsyncPythonHandle.Create())
            {
                pythonCore.SetupAsyncIO();
            
                var asyncIO = PythonExtensions.GetConcretePythonModule<AsyncIOModule>();

                const int DELAY_SECONDS = 3;
                
                 var asyncCoroutine = RawPython.Run<dynamic>(
                 $"""
                    async def async_coroutine():
                        print("{nameof(asyncIO)} is running!");
                        await {(object) asyncIO.Sleep(DELAY_SECONDS):py};
                    return async_coroutine();
                 """);
                 
                var task = asyncIO.RunCoroutine(asyncCoroutine, handle);
                
                await task;
            
                Console.WriteLine($"Hello after {DELAY_SECONDS} seconds");
            }
        }
    }
}