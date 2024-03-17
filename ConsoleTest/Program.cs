using System;
using System.Threading.Tasks;
using Python.Runtime;
using PythonNETExtensions.Awaiters;
using PythonNETExtensions.Core;
using PythonNETExtensions.Modules;
using PythonNETExtensions.Modules.PythonBuiltIn;
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
            var pythonCore = PythonCore<PyVer3_11<DefaultPythonConfig>, DefaultPythonConfig>.INSTANCE;
            await pythonCore.InitializeAsync();
            await pythonCore.InitializeDependentPackages();
            
            using (new PythonHandle())
            {
                var helloWorldText = "Hello World!";
                
                new PythonMethodHandle
                (
                    parameters: [ nameof(helloWorldText) ],
                    methodBody: 
                    $"""
                    print({nameof(helloWorldText)});
                    """
                ).Method(helloWorldText);
                
                // Numpy module
                dynamic np = PythonExtensions.GetPythonModule<Numpy>();
                Console.WriteLine(np.array((int[]) [1, 2, 3, 4, 5]));
            }
        }
    }
}