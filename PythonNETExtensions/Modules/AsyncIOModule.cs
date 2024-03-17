using System.Runtime.CompilerServices;

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
        public dynamic SetEventLoop() => Module.sget_event_loop();
    }
}