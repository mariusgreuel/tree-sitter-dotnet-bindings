# .NET bindings for tree-sitter parsing library

Provides .NET bindings for the [tree-sitter](https://github.com/tree-sitter/tree-sitter) parsing library.
Also includes the native tree-sitter parsing library and a complete set of native language parsing libraries.

## Key Features

* .NET bindings for the tree-sitter parsing library.
* Includes native libraries for the tree-sitter parsing library and language grammars.
* Includes 28+ language grammars.
* Work with all .NET languages such as C#, F#, and VB.NET.
* Work with Windows (x86, x64, arm64), Linux (x86, x64, arm, arm64), and macOS (x64, arm64).
* Support for [predicates queries](https://github.com/tree-sitter/tree-sitter/issues/4075).
* Passes the [WebAssembly bindings](https://github.com/tree-sitter/tree-sitter/tree/master/lib/binding_web) test suite.

## How to Use

Parsing source code:
```csharp
using TreeSitter;

using var language = new Language("JavaScript");
using var parser = new Parser(language);
using var tree = parser.Parse("console.log('Hello World');")!;
Console.WriteLine($"Root node: {tree.RootNode}");
```

Expected output:
```text
Root node: (program (expression_statement (call_expression function: (member_expression object: (identifier) property: (property_identifier)) arguments: (arguments (string (string_fragment))))))
```

Running queries:
```csharp
using TreeSitter;

using var language = new Language("JavaScript");
using var parser = new Parser(language);
using var tree = parser.Parse("function one() { function two() {} }")!;
using var query = new Query(language, "(function_declaration name: (identifier) @fn)");
foreach (var capture in query.Execute(tree.RootNode).Captures)
{
    Console.WriteLine($"Found function: {capture.Node.Text}");
}
```

Expected output:
```text
Found function: one
Found function: two
```

## Installation

To install the .NET bindings for tree-sitter, add the NuGet package [TreeSitter.DotNet](https://www.nuget.org/packages/TreeSitter.DotNet) to your .NET project.

The NuGet package **TreeSitter.DotNet** consists of three components:

1. The tree-sitter .NET bindings (TreeSitter.dll)
1. The native tree-sitter parsing library (tree-sitter.[dll/so])
1. A number of native language parser DLLs (e.g. tree-sitter-c.[dll/so])

The NuGet package includes all DLLs and shared objects for the supported platforms and languages.

## Included runtime libraries

The NuGet package included pre-built native libraries for the following runtime identifier (RIDs):

- win-x64
- win-x86
- win-arm64
- linux-x64
- linux-x86
- linux-arm
- linux-arm64
- osx-x64
- osx-arm64

and the following projects:

- [Tree-sitter parsing library](https://github.com/tree-sitter/tree-sitter)
- [Agda grammar](https://github.com/tree-sitter/tree-sitter-agda)
- [Bash grammar](https://github.com/tree-sitter/tree-sitter-bash)
- [C grammar](https://github.com/tree-sitter/tree-sitter-c)
- [C++ grammar](https://github.com/tree-sitter/tree-sitter-cpp)
- [C# grammar](https://github.com/tree-sitter/tree-sitter-c-sharp)
- [CSS grammar](https://github.com/tree-sitter/tree-sitter-css)
- [Embedded template languages like ERB, EJS grammar](https://github.com/tree-sitter/tree-sitter-embedded-template)
- [Go grammar](https://github.com/tree-sitter/tree-sitter-go)
- [Haskell grammar](https://github.com/tree-sitter/tree-sitter-haskell)
- [HTML grammar](https://github.com/tree-sitter/tree-sitter-html)
- [Java grammar](https://github.com/tree-sitter/tree-sitter-java)
- [JavaScript grammar](https://github.com/tree-sitter/tree-sitter-javascript)
- [JSDoc grammar](https://github.com/tree-sitter/tree-sitter-jsdoc)
- [JSON grammar](https://github.com/tree-sitter/tree-sitter-json)
- [Julia grammar](https://github.com/tree-sitter/tree-sitter-julia)
- [OCaml grammar](https://github.com/tree-sitter/tree-sitter-ocaml)
- [PHP grammar](https://github.com/tree-sitter/tree-sitter-php)
- [Python grammar](https://github.com/tree-sitter/tree-sitter-python)
- [CodeQL grammar](https://github.com/tree-sitter/tree-sitter-ql)
- [Rust grammar](https://github.com/tree-sitter/tree-sitter-rust)
- [Razor grammar](https://github.com/tree-sitter/tree-sitter-razor)
- [Ruby grammar](https://github.com/tree-sitter/tree-sitter-ruby)
- [Rust grammar](https://github.com/tree-sitter/tree-sitter-rust)
- [Scala grammar](https://github.com/tree-sitter/tree-sitter-scala)
- [Swift grammar](https://github.com/tree-sitter/tree-sitter-swift)
- [TOML grammar](https://github.com/tree-sitter/tree-sitter-toml)
- [Tree-sitter query grammar](https://github.com/tree-sitter/tree-sitter-tsq)
- [TypeScript grammar](https://github.com/tree-sitter/tree-sitter-typescript)
- [SystemVerilog grammar](https://github.com/tree-sitter/tree-sitter-verilog)

## Development

The tree-sitter .NET bindings consists of two projects:

- The tree-sitter .NET bindings
- The native tree-sitter libraries

If you want to build the tree-sitter .NET bindings,
you need to build the native tree-sitter libraries first.

Building the native tree-sitter libraries is currently supported only for Windows x64 and Linux x64.

### Building .NET bindings for Windows

To build the tree-sitter libraries for **Windows**
open a **Developer Command Prompt for VS 2022** and run the following commands:

```cmd
cd tree-sitter-native
msbuild tree-sitter-native.sln /p:Configuration=Release /p:Platform=x64
cd ..\src
dotnet build --runtime win-x64 --configuration Release
```

The native tree-sitter libraries will be placed in the folder `/build/runtimes/win-x64/native`
and automatically copied to your .NET `bin` folder when you build `TreeSitter.csproj`.

### Building .NET bindings for Linux

To build the tree-sitter libraries for **Linux**
open a shell and run the following commands:

```bash
cd tree-sitter-native
make
cd ../src
dotnet build --runtime linux-x64 --configuration Release
```

The native tree-sitter libraries will be built in the folder `/build/runtimes/linux-x64/native`
and automatically copied to your .NET `bin` folder when you build `TreeSitter.csproj`.

## Additional Documentation

* [GitHub project README](https://github.com/mariusgreuel/tree-sitter-dotnet-bindings)
* [Tree-sitter documentation](https://tree-sitter.github.io/tree-sitter/)

## Feedback & Contributing

.NET bindings for tree-sitter is released as open source under the [MIT license](https://licenses.nuget.org/MIT).
Bug reports and contributions are welcome at [the GitHub repository](https://github.com/mariusgreuel/tree-sitter-dotnet-bindings).
