namespace PythonNETExtensions
{
    public interface IPythonModule
    {
        public static abstract string[] DependentPackages { get; }
        
        public static abstract string ModuleName { get; }
    }
}