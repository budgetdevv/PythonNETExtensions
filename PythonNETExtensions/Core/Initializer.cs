using System.Runtime.CompilerServices;
using System.Threading;

namespace PythonNETExtensions.Core
{
    public struct Initializer
    {
        public const int INITIALIZED = -1, UNINITIALIZED = 0;

        private int State;

        public Initializer(int initialState = INITIALIZED)
        {
            State = initialState;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryInitialize()
        {
            var success = Interlocked.CompareExchange(ref State, INITIALIZED, UNINITIALIZED);

            return success == UNINITIALIZED;
        }

        public bool IsInitialized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => State == INITIALIZED;
        }
    }
}