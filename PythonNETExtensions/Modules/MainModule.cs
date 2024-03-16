namespace PythonNETExtensions.Modules
{
    public struct MainModule: IPythonModule<MainModule>, IPythonModuleBase
    {
        public static string DependentPackage => null;
        public static string ModuleName => "__main__";
    }
}