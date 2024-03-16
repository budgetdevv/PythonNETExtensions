using System;
using System.Threading.Tasks;
using PythonNETExtensions.Core;
using PythonNETExtensions.Modules;
using PythonNETExtensions.PythonConfig;
using PythonNETExtensions.PythonVersions;

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
            var pythonCore = PythonCore<PyVer3_11, DefaultPythonConfig>.INSTANCE;
            await pythonCore.InitializeAsync();
            await pythonCore.InitializeDependentPackages();

            using (new PythonHandle())
            {
                // Compile method
                const string TEXT_PARAM_NAME = "text";
                var method = new PythonMethodHandle([ TEXT_PARAM_NAME ], $"print({TEXT_PARAM_NAME})");
                method.Method("Hello world!");

                // Numpy module
                var np = PythonExtensions.GetCachedPythonModule<Numpy>();
                Console.WriteLine(np.array((int[]) [1, 2, 3, 4, 5]));
                
                var sys = PythonExtensions.GetCachedPythonModule<SysModule>();
                Console.WriteLine(sys.path);
            }
        }
    }
}