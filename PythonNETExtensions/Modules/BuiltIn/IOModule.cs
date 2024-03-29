using System;

namespace PythonNETExtensions.Modules.BuiltIn
{
    public struct IOModule: IPythonBuiltInModule<IOModule>
    {
        public static string ModuleName => "io";
        public static event Action OnModuleInitialized;
    }
}