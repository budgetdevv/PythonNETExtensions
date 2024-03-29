using System;

namespace PythonNETExtensions.Modules.BuiltIn
{
    public struct MainModule: IPythonBuiltInModule<MainModule>
    {
        public static string ModuleName => "__main__";
        public static event Action OnModuleInitialized;
    }
}