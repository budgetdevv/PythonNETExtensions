using System;

namespace PythonNETExtensions.Modules.BuiltIn
{
    public struct SysModule: IPythonBuiltInModule<SysModule>
    {
        public static string ModuleName => "sys";
        public static event Action OnModuleInitialized;
    }
}