using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PythonNETExtensions.Core;
using PythonNETExtensions.Modules;
using PythonNETExtensions.Modules.PythonBuiltIn;

namespace PythonNETExtensions.AsyncIO
{
    internal static class AsyncIOCore
    {
        private static readonly AsyncIOModule ASYNC_IO;
        
        private static readonly dynamic EVENT_LOOP, EVENT_LOOP_THREAD;

        static AsyncIOCore()
        {
            // Assume that GIL is taken. It is an internal class so only we can invoke it anyway

            var asyncIO = ASYNC_IO = PythonExtensions.GetConcretePythonModule<AsyncIOModule>();
             
            if (false)
            {
                 var result = RawPython.Run<dynamic>(
                 $"""
                 import asyncio;
                 import threading;

                 loop = asyncio.new_event_loop();

                 def set_and_run(loop):
                 asyncio.set_event_loop(loop);
                 loop.run_forever();
                               
                 loop_thread = threading.Thread(target=set_and_run, args=(loop,));
                 loop_thread.start();
                 return (loop, loop_thread);
                 """);
             
                EVENT_LOOP = result[0];
                EVENT_LOOP_THREAD = result[1];
            }
             
            else
            {
                EVENT_LOOP = asyncIO.CreateEventLoop();
                
                var eventLoopThread = EVENT_LOOP_THREAD = RawPython.Run<dynamic>(
                    $"""
                     import threading;
                     return threading.Thread(target={new RawPython.PythonObject((Action) SetupAndRunLoop)});
                     """);

                eventLoopThread.start();
            
                void SetupAndRunLoop()
                {
                    var eventLoop = EVENT_LOOP;
                    asyncIO.SetEventLoop(eventLoop);
                    eventLoop.run_forever();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static dynamic GenerateAwaiter(dynamic taskCompletionSource, dynamic coroutine, bool hasReturn)
        { 
            const string METHOD_NAME = "generated_awaiter", RESULT_VAR_NAME = "result";
            
            var awaiter = RawPython.Run<dynamic>(
            $"""
            async def {METHOD_NAME}():
                {RESULT_VAR_NAME} = await {new RawPython.PythonObject(coroutine)};
                {new RawPython.PythonObject(taskCompletionSource)}.{nameof(taskCompletionSource.SetResult)}({(hasReturn ?  RESULT_VAR_NAME : string.Empty)});
                      
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