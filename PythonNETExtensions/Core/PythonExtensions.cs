using System.Runtime.CompilerServices;
using PythonNETExtensions.Modules;

namespace PythonNETExtensions.Core
{
    public static class PythonExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static dynamic GetCachedPythonModule<PythonModuleT>() where PythonModuleT : struct, IPythonModule<PythonModuleT>
        {
            return PythonModuleT.ModuleCache;
        }
    }
}