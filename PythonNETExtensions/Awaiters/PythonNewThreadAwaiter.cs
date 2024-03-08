using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PythonNETExtensions.Awaiters
{
    public struct PythonNewThreadAwaiter: ICriticalNotifyCompletion
    {
        public PythonNewThreadAwaiter()
        {
            
        }

        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => false;
        }
        
        public void OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsafeOnCompleted(Action continuation)
        {
            Task.Run(continuation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PythonNewThreadAwaiter GetAwaiter()
        {
            return this;
        }
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult() { }
    }
}