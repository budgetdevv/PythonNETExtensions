using Python.Runtime;

namespace PythonNETExtensions.Modules
{
    public interface IPythonModuleBase
    {
        // Tag interface for reflection 
    }
    
    public interface IPythonModule<PythonModuleT>: IPythonModuleBase where PythonModuleT: struct, IPythonModule<PythonModuleT>
    {
        public static abstract string DependentPackage { get; }
        
        public static abstract string ModuleName { get; }
        
        private static readonly PyObject MODULE_CACHE = Py.Import(PythonModuleT.ModuleName);

        public static virtual dynamic ModuleCache => MODULE_CACHE;
    }
}