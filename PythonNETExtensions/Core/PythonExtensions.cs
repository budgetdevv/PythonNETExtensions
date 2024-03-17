using System.Runtime.CompilerServices;
using PythonNETExtensions.Modules;

namespace PythonNETExtensions.Core
{
    public static class PythonExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dynamic GetPythonModule<PythonModuleT>() where PythonModuleT: struct, IPythonModule<PythonModuleT>
        {
            return PythonModuleT.ModuleCache;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PythonModuleT GetConcretePythonModule<PythonModuleT>() 
            where PythonModuleT: struct, IPythonConcreteModule<PythonModuleT>, IPythonModule<PythonModuleT>
        {
            return PythonModuleT.ConstructConcreteModule(PythonModuleT.ModuleCache);
        }
    }
}