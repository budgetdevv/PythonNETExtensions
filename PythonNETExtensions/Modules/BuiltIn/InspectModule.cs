using System;

namespace PythonNETExtensions.Modules.BuiltIn
{
    public struct InspectModule: IPythonBuiltInModule<InspectModule>
    {
        public static string ModuleName => "inspect";
        public static event Action<InspectModule> OnModuleInitialized;
    }
}