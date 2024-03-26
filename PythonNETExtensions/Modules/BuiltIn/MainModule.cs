namespace PythonNETExtensions.Modules.BuiltIn
{
    public struct MainModule: IPythonBuiltInModule<MainModule>
    {
        public static string ModuleName => "__main__";
    }
}