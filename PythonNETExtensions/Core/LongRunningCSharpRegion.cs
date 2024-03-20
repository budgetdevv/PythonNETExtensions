using System.Runtime.CompilerServices;

namespace PythonNETExtensions.Core
{
    public readonly ref struct LongRunningCSharpRegion
    {
        private readonly ref PythonHandle Handle;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LongRunningCSharpRegion()
        {
            Handle.Dispose();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Handle = new PythonHandle();
        }
    }
}