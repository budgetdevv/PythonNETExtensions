using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Python.Runtime;
using PythonNETExtensions.Modules;
using PythonNETExtensions.Modules.PythonBuiltIn;

namespace PythonNETExtensions.Core
{
    public readonly struct PythonMethodHandle: IDisposable
    {
        private static readonly PyObject MAIN_MODULE = PythonExtensions.GetPythonModule<MainModule>();

        private static int MethodCount = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetMethodNumber()
        {
            return Interlocked.Increment(ref MethodCount);
        }
        
        public readonly dynamic Method; // Underlying type is actually PyObject
        
        public PythonMethodHandle(string[] parameters, string methodBody, string methodName = null, bool async = false)
        {
            methodName ??= $"PythonMethod_{GetMethodNumber()}";

            var indentedMethodBody = string.Empty;
            
            foreach (var line in methodBody.Split('\n'))
            {
                indentedMethodBody += $"   {line}\n";
            }

            var code =
            $"""
            {(async ? "async " : string.Empty)}def {methodName}({string.Join(", ", parameters)}):
            {indentedMethodBody}
            """;
            Console.WriteLine(code);
            
            PythonEngine.RunSimpleString(code);
            
            Method = MAIN_MODULE.GetAttr("__dict__")[methodName];
        }

        public void Dispose()
        {
            // TODO
        }
    }
}