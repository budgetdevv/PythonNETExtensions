using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Python.Runtime;

namespace PythonNETExtensions.Core
{
    public unsafe struct PythonHandle: IDisposable
    {
        private PyGILState GILState;

        private enum PyGILState
        {
            PyGILState_LOCKED,
            PyGILState_UNLOCKED 
        }
        
        // TODO: Wait for .NET 9 support

        // [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = nameof(AcquireLock))]
        // static extern PyGILState AcquireLock(PythonEngine @class);
        //
        // [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = nameof(ReleaseLock))]
        // private static extern void ReleaseLock(PythonEngine @class, PyGILState state);

        private static readonly delegate* unmanaged[Cdecl]<PyGILState> PyGILState_Ensure;

        private static readonly delegate* unmanaged[Cdecl]<PyGILState, void> PyGILState_Release;
        
        static PythonHandle()
        {
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;

            var delegates = typeof(Runtime).GetNestedType("Delegates", bindingFlags)!;
            
            PyGILState_Ensure = (delegate* unmanaged[Cdecl]<PyGILState>) (nint) delegates
                .GetProperty(nameof(PyGILState_Ensure), bindingFlags)!
                .GetValue(null)!;
            
            PyGILState_Release = (delegate* unmanaged[Cdecl]<PyGILState, void>) (nint) delegates
                .GetProperty(nameof(PyGILState_Release), bindingFlags)!
                .GetValue(null)!;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PythonHandle()
        {
            GILState = PyGILState_Ensure();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            PyGILState_Release(GILState);
        }
    }
}