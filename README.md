# PythonNETExtensions
Extension library built on top of [PythonNET](https://github.com/pythonnet/pythonnet)

# Features
- Embedded Python installation: Bundle Python installation in application!
  
```cs
var pythonCore = new PythonCoreBuilder()
    .WithConfig<DefaultPythonConfig>() // Use default config
    .WithVersion<PyVer3_11<DefaultPythonConfig>>() // Select Python 3.11
    .Build();

// Download and bundle Python3.11 into the application itself, initializing PythonEngine with it
await pythonCore.InitializeAsync();
```

- Embedded pip packages: Define and install pip-packages for bundled Python installation on the fly!
  
```cs
private struct Numpy: IPythonModule<Numpy>
{
    public static string DependentPackage => "numpy"; // Package name
    public static string ModuleName => DependentPackage; // Name of module
}

...

// This will download the pip packages and initialize them.
await pythonCore.InitializeDependentPackages();

// Python code should always be wrapped in using (PythonHandle.Create()) / using (AsyncPythonHandle.Create())
// Prefer using() over using var to relinquish GIL as soon as possible
using (PythonHandle.Create())
{
  var numpy = PythonModule.Get<Numpy>();
  Console.WriteLine(numpy.array((int[]) [ 1, 2, 3, 4, 5 ]));
}
```

- AsyncIO support: Leverage async-await C# side for asyncIO coroutines.

```cs
// Run this once during startup
pythonCore.SetupAsyncIO();

using (var handle = AsyncPythonHandle.Create())
{
  const int SLEEP_SECONDS = 3;

  var asyncIO = PythonModule.GetConcrete<AsyncIOModule>();
  await asyncIO.RunCoroutine(asyncIO.Sleep(SLEEP_SECONDS), handle);

  Console.WriteLine($"Hello after {SLEEP_SECONDS} seconds!");
}
```

- Run python code ( Text-based ), with support for async await. Directly embed C# objects into python code via string interpolation `$"{someCSharpObject:py}"`
```cs
using (PythonHandle.Create())
{
    const string HELLO_WORLD_TEXT = "Hello World!";

    // Prints "Hello World!", returns "Goodbye World" as a string
    var result = RawPython.Run<string>(
    $"""
    print({HELLO_WORLD_TEXT:py});
    return 'Goodbye World';
    """);
    
    // Prints "Goodbye World"
    Console.WriteLine(result);
}

...

// Run this once during startup
pythonCore.SetupAsyncIO();

using (var handle = AsyncPythonHandle.Create())
{
    var awaiter = RawPython.RunAsync<int>(
    $"""
    import asyncio;
    await asyncio.sleep(3);
    return 12;
    """, handle);
    
    // The result variable will be assigned 12 after 3 seconds
    var result = await awaiter;
    
    // Should print 12        
    Console.WriteLine(result);
}
```

# Supported Platforms

- MacOS: AMD64 / ARM64
- Windows: AMD64 ( ARM64 support coming soon )
- Linux: ARM64 ( AMD64 support coming soon )

32-bit platforms are NOT supported!
