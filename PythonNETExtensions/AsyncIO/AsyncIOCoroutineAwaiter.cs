using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PythonNETExtensions.Core;

namespace PythonNETExtensions.AsyncIO
{
    public struct AsyncIOCoroutineAwaiter: ICriticalNotifyCompletion
    {
        private readonly Task CoroutineTask;

        private readonly AsyncPythonHandle Handle;

        internal AsyncIOCoroutineAwaiter(Task coroutineTask, AsyncPythonHandle handle)
        {
            CoroutineTask = coroutineTask;
            Handle = handle;
        }

        public bool IsCompleted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CoroutineTask.IsCompleted;
        }
        
        public void OnCompleted(Action continuation)
        {
            throw new NotImplementedException();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            var handle = Handle;
            handle.Handle.Dispose();
            
            CoroutineTask.ContinueWith(_ =>
            {
                handle.Handle = new PythonHandle();
                continuation();
            });
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AsyncIOCoroutineAwaiter GetAwaiter()
        {
            return this;
        }
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetResult() { }
    }
}