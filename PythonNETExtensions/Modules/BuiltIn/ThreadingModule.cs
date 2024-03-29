using System;

namespace PythonNETExtensions.Modules.BuiltIn
{
    public struct ThreadingModule: IPythonBuiltInModule<ThreadingModule>, IPythonConcreteModule<ThreadingModule>
    {
        public static string ModuleName => "threading";
        public static event Action<ThreadingModule> OnModuleInitialized;

        public static ThreadingModule ConstructConcreteModule(dynamic moduleCache)
        {
            return new ThreadingModule(moduleCache);
        }

        public dynamic Module { get; init; }

        public ThreadingModule(dynamic module)
        {
            Module = module;
        }
        
        public dynamic GetCurrentThread() => Module.current_thread();
    }
}