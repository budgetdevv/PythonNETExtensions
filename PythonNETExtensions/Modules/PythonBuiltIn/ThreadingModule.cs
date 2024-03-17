namespace PythonNETExtensions.Modules.PythonBuiltIn
{
    public struct ThreadingModule: IPythonBuiltInModule<ThreadingModule>, IPythonConcreteModule<ThreadingModule>
    {
        public static string ModuleName => "sys";
        
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