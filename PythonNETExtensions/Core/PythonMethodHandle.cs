using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Python.Runtime;
using PythonNETExtensions.Modules;

namespace PythonNETExtensions.Core
{
    public readonly struct PythonMethodHandle: IDisposable
    {
        private static readonly PyObject MAIN_MODULE = PythonExtensions.GetCachedPythonModule<MainModule>();

        private static int MethodCount = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetMethodNumber()
        {
            return Interlocked.Increment(ref MethodCount);
        }
        
        public readonly dynamic Method; // Underlying type is actually PyObject
        
        public PythonMethodHandle(string[] parameters, string methodBody, string methodName = null)
        {
            methodName ??= $"PythonMethod_{GetMethodNumber()}";

            var code =
            $"""
            def {methodName}({string.Join(", ", parameters)}):
                {methodBody}
            """;
            PythonEngine.RunSimpleString(code);
            
            Method = MAIN_MODULE.GetAttr("__dict__")[methodName];
        }

        public void Dispose()
        {
            // TODO
        }
    }
}