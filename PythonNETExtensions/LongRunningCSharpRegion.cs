using System;
using Python.Runtime;

namespace PythonNETExtensions
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