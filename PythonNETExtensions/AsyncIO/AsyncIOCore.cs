using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Python.Runtime;
using PythonNETExtensions.Core;
using PythonNETExtensions.Core.Handles;
using PythonNETExtensions.Modules;
using PythonNETExtensions.Modules.ThirdParty;

namespace PythonNETExtensions.AsyncIO
{
    internal static class AsyncIOCore
    {
        private static readonly AsyncIOModule ASYNC_IO;
        
        private static readonly dynamic EVENT_LOOP, EVENT_LOOP_THREAD;

        static AsyncIOCore()
        {
            // Assume that GIL is taken. It is an internal class - only we can invoke it anyway

            var asyncIO = ASYNC_IO = PythonModule.GetConcrete<AsyncIOModule>();
             
            EVENT_LOOP = asyncIO.CreateEventLoop();
                
            var eventLoopThread = EVENT_LOOP_THREAD = RawPython.Run<dynamic>(
            $"""
            import threading;
            return threading.Thread(target={(object) SetupAndRunLoop:py});
            """);

            eventLoopThread.start();
            
            return;

            void SetupAndRunLoop()
            {
                var eventLoop = EVENT_LOOP;
                asyncIO.SetEventLoop(eventLoop);
                eventLoop.run_forever();
            }
        }

        private static void HandleAsyncExceptions(TaskCompletionSource tcs, PyObject exception)
        {
            try
            {
                using (new PythonHandle())
                {
                    RawPython.Run($"throw {exception:py};");
                }
            }

            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            
            // ThrowLastAsClrException(null);
            //
            // [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = nameof(ThrowLastAsClrException))]
            // static extern Exception ThrowLastAsClrException(PythonException @class);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static dynamic GenerateAwaiter(dynamic taskCompletionSource, dynamic coroutine, bool hasReturn)
        { 
            const string METHOD_NAME = "generated_awaiter", 
                         RESULT_VAR_NAME = "result",
                         TASK_COMPLETION_SOURCE_VAR_NAME = "tcs",
                         EXCEPTION_VAR_NAME = "exception";
            
            var awaiter = RawPython.Run<dynamic>(
            $"""
            async def {METHOD_NAME}():
                try:
                    {TASK_COMPLETION_SOURCE_VAR_NAME} = {taskCompletionSource:py};
                    {RESULT_VAR_NAME} = await {coroutine:py};
                    {TASK_COMPLETION_SOURCE_VAR_NAME}.{nameof(TaskCompletionSource.SetResult)}({(hasReturn ? RESULT_VAR_NAME : string.Empty)});
                except Exception as {EXCEPTION_VAR_NAME}:
                    {(object) HandleAsyncExceptions:py}({TASK_COMPLETION_SOURCE_VAR_NAME}, {EXCEPTION_VAR_NAME});
                                
            return {METHOD_NAME}();
            """);

            return awaiter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task CoroutineToTask(dynamic coroutine)
        {
            var taskCompletionSource = new TaskCompletionSource();
            
            ASYNC_IO.RunCoroutineThreadSafe(GenerateAwaiter(taskCompletionSource, coroutine, hasReturn: false), EVENT_LOOP);

            return taskCompletionSource.Task;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<RetT> CoroutineToTask<RetT>(dynamic coroutine)
        {
            var taskCompletionSource = new TaskCompletionSource<RetT>();
            
            ASYNC_IO.RunCoroutineThreadSafe(GenerateAwaiter(taskCompletionSource, coroutine, hasReturn: true), EVENT_LOOP);

            return taskCompletionSource.Task;
        }
    }
}