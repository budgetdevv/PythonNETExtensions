using System.Runtime.CompilerServices;

namespace PythonNETExtensions.Modules
{
    public static class PythonModule
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dynamic Get<PythonModuleT>() where PythonModuleT: struct, IPythonModule<PythonModuleT>
        {
            return PythonModuleT.ModuleCache;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PythonModuleT GetConcrete<PythonModuleT>() 
            where PythonModuleT: struct, IPythonConcreteModule<PythonModuleT>, IPythonModule<PythonModuleT>
        {
            return PythonModuleT.ConstructConcreteModule(PythonModuleT.ModuleCache);
        }
    }
}