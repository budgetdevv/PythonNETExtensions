using System;
using Python.Runtime;

namespace PythonNETExtensions
{
    public readonly struct PythonHandle: IDisposable
    {
        private readonly Py.GILState GILState;
        
        public PythonHandle()
        {
            GILState = Py.GIL();
        }
        
        public void Dispose()
        {
            GILState.Dispose();
        }
    }
}