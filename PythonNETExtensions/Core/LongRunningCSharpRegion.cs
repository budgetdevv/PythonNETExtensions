using System.Runtime.CompilerServices;
using PythonNETExtensions.Core.Handles;

namespace PythonNETExtensions.Core
{
    public readonly ref struct LongRunningCSharpRegion
    {
        private readonly ref PythonHandle Handle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal LongRunningCSharpRegion(ref PythonHandle handle)
        {
            Handle = ref handle;
            handle.Dispose();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            Handle = new PythonHandle();
        }
    }
}