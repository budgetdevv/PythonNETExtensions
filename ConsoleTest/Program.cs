using System.Threading.Tasks;
using PythonNETExtensions;

namespace ConsoleTest
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            await PythonCore.InitializeAsync();
        }
    }
}