using Python.Runtime;

namespace PythonNETExtensions.Modules
{
    public static class ModuleCache<PyModuleT> where PyModuleT: struct, IPythonModule<PyModuleT>
    {
        public static readonly dynamic CACHED_MODULE = Py.Import(PyModuleT.ModuleName);
    }
}