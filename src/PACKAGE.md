## About

Provides .NET bindings for the [tree-sitter](https://github.com/tree-sitter/tree-sitter) parsing library.
Also includes the native tree-sitter parsing library and a complete set of native language parsing libraries.

## Key Features

* .NET bindings for the tree-sitter parsing library.
* Includes native libraries for the tree-sitter parsing library and language grammars.
* Supports 28+ language grammars.
* Supports both Windows and Linux.
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

## Main Types

The main types provided by this library are:

* `TreeSitter.Language`
* `TreeSitter.Parser`
* `TreeSitter.Tree`
* `TreeSitter.Node`
* `TreeSitter.TreeCursor`
* `TreeSitter.Query`

## Additional Documentation

* [GitHub project README](https://github.com/mariusgreuel/tree-sitter-dotnet-bindings)
* [Tree-sitter documentation](https://tree-sitter.github.io/tree-sitter/)

## Feedback & Contributing

.NET bindings for tree-sitter is released as open source under the [MIT license](https://licenses.nuget.org/MIT).
Bug reports and contributions are welcome at [the GitHub repository](https://github.com/mariusgreuel/tree-sitter-dotnet-bindings).
