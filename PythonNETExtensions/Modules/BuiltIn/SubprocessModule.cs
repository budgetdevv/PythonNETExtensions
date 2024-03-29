using System;

namespace PythonNETExtensions.Modules.BuiltIn
{
    public struct SubprocessModule: IPythonBuiltInModule<SubprocessModule>
    {
        public static string ModuleName => "subprocess";
        public static event Action OnModuleInitialized;
    }
}