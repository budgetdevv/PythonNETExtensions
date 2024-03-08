using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Python.Runtime;
using PythonNETExtensions.Modules;

namespace PythonNETExtensions
{
    public readonly struct PythonMethodHandle: IDisposable
    {
        private static readonly string GUID = Guid.NewGuid().ToString();

        private static readonly PyObject MAIN = PythonExtensions.GetModule<MainModule>();

        private static int MethodCount = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetMethodNumber()
        {
            return Interlocked.Increment(ref MethodCount);
        }
        
        public readonly PyObject Method;
        
        public PythonMethodHandle(string[] parameters, string methodBody, string methodName = null)
        {
            methodName ??= $"{GUID}{GetMethodNumber()}";
            
            PythonEngine.RunSimpleString(@$"
def {methodName}({string.Join(", ", parameters)}):
    {methodBody}");
            
            Method = MAIN.GetAttr("__dict__")[methodName];
        }

        public void Dispose()
        {
            // TODO
        }
    }
}