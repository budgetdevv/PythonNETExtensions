using System;

namespace PythonNETExtensions.Modules
{
    public interface IPythonModuleBase
    {
        // Tag interface for reflection 
    }
    
    public interface IPythonModule<PyModuleT>: IPythonModuleBase where PyModuleT: struct, IPythonModule<PyModuleT>
    {
        public static abstract string DependentPackage { get; }
        
        public static abstract string ModuleName { get; }

        public static abstract event Action OnModuleInitialized;

        public static virtual dynamic ModuleCache => ModuleCache<PyModuleT>.CACHED_MODULE;
    }
    
    public interface IPythonBuiltInModule<PyModuleT>: IPythonModule<PyModuleT> where PyModuleT: struct, IPythonModule<PyModuleT>
    {
        static string IPythonModule<PyModuleT>.DependentPackage => null;
    }

    public interface IPythonConcreteModule<PyModuleT> where PyModuleT: struct, IPythonConcreteModule<PyModuleT>, IPythonModule<PyModuleT>
    {
        public static abstract PyModuleT ConstructConcreteModule(dynamic moduleCache);
        
        public dynamic Module { get; init; }
    }
}