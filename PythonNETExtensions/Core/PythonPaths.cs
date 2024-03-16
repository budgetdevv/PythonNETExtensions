namespace PythonNETExtensions.Core
{
    public readonly struct PythonPaths
    {
        public readonly string Home, PipExecutable, Packages;

        public PythonPaths(string home, string pipExecutable, string packages)
        {
            Home = home;
            PipExecutable = pipExecutable;
            Packages = packages;
        }
    }
}