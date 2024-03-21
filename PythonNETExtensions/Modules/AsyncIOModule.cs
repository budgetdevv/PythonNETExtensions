using System.Runtime.CompilerServices;
using PythonNETExtensions.AsyncIO;
using PythonNETExtensions.Core.Handles;

namespace PythonNETExtensions.Modules
{
    public struct AsyncIOModule: IPythonModule<AsyncIOModule>, IPythonConcreteModule<AsyncIOModule>
    {
        public static string DependentPackage => "asyncio";
        public static string ModuleName => DependentPackage;
        
        public static AsyncIOModule ConstructConcreteModule(dynamic moduleCache)
        {
            return new AsyncIOModule(moduleCache);
        }
        
        public dynamic Module { get; init; }

        public AsyncIOModule(dynamic module)
        {
            Module = module;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public dynamic CreateEventLoop() => Module.new_event_loop();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public dynamic GetEventLoop() => Module.get_event_loop();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public dynamic SetEventLoop(dynamic loop) => Module.set_event_loop(loop);

        public dynamic RunCoroutineThreadSafe(dynamic coroutine, dynamic loop) => Module.run_coroutine_threadsafe(coroutine, loop);

        public AsyncIOCoroutineAwaiter RunCoroutine(dynamic coroutine, AsyncPythonHandle handle)
        {
            return new AsyncIOCoroutineAwaiter(AsyncIOCore.CoroutineToTask(coroutine), handle);
        }
        
        public AsyncIOCoroutineAwaiter<RetT> RunCoroutine<RetT>(dynamic coroutine, AsyncPythonHandle handle)
        {
            return new AsyncIOCoroutineAwaiter<RetT>(AsyncIOCore.CoroutineToTask(coroutine), handle);
        }

        public dynamic Sleep(int durationInSeconds) => Module.sleep(durationInSeconds);
    }
}