using System.Runtime.CompilerServices;

namespace PythonNETExtensions.Helpers
{
    public static class TextHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string IndentCode(this string code, string indentation = "    ")
        {
            return $"{indentation}{code.Replace("\n", $"\n{indentation}")}";
        }
    }
}