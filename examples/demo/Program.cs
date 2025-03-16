using TreeSitter;

{
    using var language = new Language("JavaScript");
    using var parser = new Parser(language);
    using var tree = parser.Parse("console.log('Hello World');")!;
    Console.WriteLine($"Root node: {tree.RootNode}");
}

{
    using var language = new Language("JavaScript");
    using var parser = new Parser(language);
    using var tree = parser.Parse("function one() { function two() {} }")!;
    using var query = new Query(language, "(function_declaration name: (identifier) @fn)");
    foreach (var capture in query.Execute(tree.RootNode).Captures)
    {
        Console.WriteLine($"Found function: {capture.Node.Text}");
    }
}
