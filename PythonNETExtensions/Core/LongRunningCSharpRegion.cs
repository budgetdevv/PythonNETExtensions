using System;
using Python.Runtime;

namespace PythonNETExtensions.Core
{
    public readonly struct LongRunningCSharpRegion: IDisposable
    {
        private readonly nint Handle;
        
        public LongRunningCSharpRegion()
        {
            Handle = PythonEngine.BeginAllowThreads();
        }
        
        public void Dispose()
        {
            PythonEngine.EndAllowThreads(Handle);
        }
    }
}