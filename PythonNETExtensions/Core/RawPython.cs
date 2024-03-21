using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Python.Runtime;
using PythonNETExtensions.Helpers;

namespace PythonNETExtensions.Core
{
    public static class RawPython
    {
        private struct StringBuilderThreadStaticDefinition: IThreadStaticDefinition<StringBuilder>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static StringBuilder CreateItem()
            {
                return new StringBuilder();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void OnGet(ref StringBuilder item)
            {
                item.Clear();
            }
        }
        
        private struct PyScopeThreadStaticDefinition: IThreadStaticDefinition<PyModule>
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static PyModule CreateItem()
            {
                return Py.CreateScope();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void OnGet(ref PyModule item) { }
        }

        [InterpolatedStringHandler]
        public struct CodeInterpolator
        {
            private readonly StringBuilder LocalStringBuilder;

            public readonly PyModule Scope;

            private int CurrentObjectIndex;

            public bool ShouldCompile;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public CodeInterpolator(int literalLength, int formattedCount)
            {
                // StringBuilderThreadStaticDefinition clears the StringBuilder for us
                var stringBuilder = LocalStringBuilder = ThreadStatic<StringBuilderThreadStaticDefinition, StringBuilder>.Item;
                stringBuilder.EnsureCapacity(literalLength);
                
                Scope = ThreadStatic<PyScopeThreadStaticDefinition, PyModule>.Item;
                ShouldCompile = true;
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AppendLiteral(string text)
            {
                LocalStringBuilder.Append(text);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AppendFormatted(string text)
            {
                // If any of the formatted strings are not interned, we shouldn't compile
                if (string.IsInterned(text) == null)
                {
                    ShouldCompile = false;
                }
                
                LocalStringBuilder.Append(text);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AppendFormatted<T>(T item)
            {
                LocalStringBuilder.Append(item);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AppendFormatted(dynamic item, string format)
            {
                AppendFormatted<dynamic>(item, format);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AppendFormatted<T>(T item, string format)
            {
                // JIT should be able to eliminate branching, since format is a constant string.
                if (format == "py")
                {
                    var name = $"py_local_{CurrentObjectIndex++}";
                    Scope.Set(name, item);
                    LocalStringBuilder.Append(name);
                }

                else
                {
                    throw new Exception("Invalid format");
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString()
            {
                return LocalStringBuilder.ToString();
            }
        }
        
        public enum CompilationOption
        {
            Auto,
            Compile,
            ExecuteOnly
        }

        private static readonly ConcurrentDictionary<string, PyObject> CODE_TO_COMPILATION_MAP = new(); 
        
        public static void Run(CodeInterpolator code, CompilationOption compilationOption = CompilationOption.Auto)
        {
            var codeText = code.ToString();
            RunInternal(codeText, code, compilationOption);
        }
        
        public static RetT Run<RetT>(CodeInterpolator code, CompilationOption compilationOption = CompilationOption.Auto)
        {
            var codeText = code.ToString();
            
            const string METHOD_WRAPPER_NAME = "py_wrapper_method", RET_VAR_NAME = "py_ret";
            
            // TODO: Somehow optimize performance of this
            codeText = 
            $"""
            def {METHOD_WRAPPER_NAME}():
            {codeText.IndentCode()}
                
            {RET_VAR_NAME} = {METHOD_WRAPPER_NAME}();
            """;
            
            RunInternal(codeText, code, compilationOption);
            
            return code.Scope.Get<RetT>(RET_VAR_NAME);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void RunInternal(string codeText, CodeInterpolator code, CompilationOption compilationOption)
        {
            var scope = code.Scope;
            
            bool shouldCompile;
            
            switch (compilationOption)
            {
                case CompilationOption.Auto:
                    shouldCompile = code.ShouldCompile;
                    break;
                case CompilationOption.Compile:
                    shouldCompile = true;
                    break;
                case CompilationOption.ExecuteOnly:
                    shouldCompile = false;
                    break;
                default:
                    throw new InvalidEnumArgumentException();
            }

            if (shouldCompile)
            {
                var compilations = CODE_TO_COMPILATION_MAP;

                if (!compilations.TryGetValue(codeText, out var compilation))
                {
                    compilations[codeText] = compilation = PythonEngine.Compile(codeText);
                }

                scope.Execute(compilation);
            }

            else
            {
                scope.Exec(codeText);
            }

            if (false)
            {
                Console.WriteLine(
                $"""
                Compiled: {shouldCompile}

                Codegen:
                {codeText}
                """);
            }
        }
    }
}