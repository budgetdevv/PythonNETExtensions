namespace PythonNETExtensions.Modules
{
    public interface IPythonModule
    {
        public static abstract string[] DependentPackage { get; }
        
        public static abstract string ModuleName { get; }
    }
}