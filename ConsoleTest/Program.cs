using System.Threading.Tasks;
using PythonNETExtensions;
using PythonNETExtensions.PythonVersions;

namespace ConsoleTest
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            await PythonCore.InitializeAsync<PyVer3_11>();
        }
    }
}