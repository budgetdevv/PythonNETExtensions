namespace PythonNETExtensions.Modules.BuiltIns
{
    public struct MainModule: IPythonBuiltInModule<MainModule>
    {
        public static string ModuleName => "__main__";
    }
}