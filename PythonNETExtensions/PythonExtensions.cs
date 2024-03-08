using Python.Runtime;

namespace PythonNETExtensions
{
    internal static class ModuleCache<ModuleT> where ModuleT: struct, IPythonModule
    {
        public static readonly PyObject MODULE = Py.Import(ModuleT.ModuleName);
    }
    
    public class PythonExtensions
    {
        public static PyObject GetModule<ModuleT>() where ModuleT: struct, IPythonModule
        {
            return ModuleCache<ModuleT>.MODULE;
        }
    }
}