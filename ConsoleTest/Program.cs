using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PythonNETExtensions;

namespace ConsoleTest
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            await PythonCore.InitializeAsync();

            // Console.WriteLine(typeof(PythonCore));
            //
            // while (!Volatile.Read(ref PythonCore.Ready))
            // {
            //     
            // }
        }
    }
}