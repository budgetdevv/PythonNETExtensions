using System;
using System.Threading.Tasks;
using Python.Runtime;
using PythonNETExtensions.Awaiters;
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
            var pythonCore = PythonCore<PyVer3_11<DefaultPythonConfig>, DefaultPythonConfig>.INSTANCE;
            await pythonCore.InitializeAsync();
            await pythonCore.InitializeDependentPackages();
            
            using (new PythonHandle())
            {
                var helloWorldText = "Hello World!";

                var sys = PythonExtensions.GetPythonModule<SysModule>();

                var result = RawPython.Run<string>(
                $"""
                print({new RawPython.PythonObject(helloWorldText)});
                return {new RawPython.PythonObject(sys)}.executable;
                """);
                
                Console.WriteLine(result);
                
                // Numpy module
                dynamic np = PythonExtensions.GetPythonModule<Numpy>();
                Console.WriteLine(np.array((int[]) [1, 2, 3, 4, 5]));
            }
        }
    }
}