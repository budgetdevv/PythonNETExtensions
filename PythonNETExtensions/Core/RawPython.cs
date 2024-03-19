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
        public readonly struct Object
        {
            public readonly dynamic Item;

            public readonly string Name;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Object(dynamic item, string name = null)
            {
                Item = item;
                Name = name;
            }
        }
        
        [InterpolatedStringHandler]
        public ref struct InterpolationHandler
        {
            [ThreadStatic]
            private static StringBuilder _StringBuilder;

            private static StringBuilder STRING_BUILDER
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    return _StringBuilder ?? CreateStringBuilder();

                    [MethodImpl(MethodImplOptions.NoInlining)]
                    StringBuilder CreateStringBuilder()
                    {
                        return new StringBuilder();
                    }
                }
            }

            private readonly StringBuilder LocalStringBuilder;

            public readonly PyModule Scope;

            private int CurrentObjectIndex;

            public bool ShouldCompile;
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public InterpolationHandler(int literalLength, int formattedCount)
            {
                var stringBuilder = LocalStringBuilder = STRING_BUILDER;
                stringBuilder.Clear();
                stringBuilder.EnsureCapacity(literalLength);

                Scope = Py.CreateScope();
                
                ShouldCompile = true;

                // var scope = Py.CreateScope();
                // scope.Set()
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AppendLiteral(string text)
            {
                LocalStringBuilder.Append(text);
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void AppendFormatted(Object pyObject)
            {
                var name = pyObject.Name;

                if (string.IsNullOrWhiteSpace(name))
                {
                    name = $"py_local_{CurrentObjectIndex++}";
                }

                else
                {
                    // If custom name is specified ( Which means we won't be using generated object name ),
                    // then we can't guarantee similar text, which means compiling is bad idea
                    ShouldCompile = false;
                }

                Scope.Set(name, pyObject.Item);
                
                LocalStringBuilder.Append(name);
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
            public void AppendFormatted<T>(T item, string format)
            {
                if (format == "py")
                {
                    AppendFormatted(new Object(item));
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

        private static readonly ConcurrentDictionary<string, PyObject> CODE_TO_COMPILATION_MAP = new ConcurrentDictionary<string, PyObject>(); 
        
        public static void Run(InterpolationHandler code, CompilationOption compilationOption = CompilationOption.Auto)
        {
            var codeText = code.ToString();
            using var scope = code.Scope;
            RunInternal(codeText, code, compilationOption);
        }
        
        public static RetT Run<RetT>(InterpolationHandler code, CompilationOption compilationOption = CompilationOption.Auto)
        {
            var codeText = code.ToString();
            
            const string METHOD_WRAPPER_NAME = "py_wrapper_method", RET_VAR_NAME = "py_ret";
            
            codeText = 
            $"""
            def {METHOD_WRAPPER_NAME}():
            {codeText.IndentCode()}
                
            {RET_VAR_NAME} = {METHOD_WRAPPER_NAME}();
            """;
            
            RunInternal(codeText, code, compilationOption);
            
            using var scope = code.Scope;
            return scope.Get<RetT>(RET_VAR_NAME);
        }

        private static void RunInternal(string codeText, InterpolationHandler code, CompilationOption compilationOption)
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