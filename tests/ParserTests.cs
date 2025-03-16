//
// Tests for .NET bindings for tree-sitter
// Adapted from official tree-sitter web bindings tests 'parser.test.ts'
// Copyright (c) 2025 Marius Greuel
// Copyright (c) 2018-2024 Max Brunsfeld
// SPDX-License-Identifier: MIT
//

namespace TreeSitter.Tests;

[TestClass]
public class ParserTests
{
    [TestMethod]
    public void AllowsSettingTheLanguageToNull()
    {
        using var parser = new Parser();
        Assert.IsNull(parser.Language);

        parser.Language = _javaScript;
        Assert.AreEqual(_javaScript, parser.Language);

        parser.Language = null;
        Assert.IsNull(parser.Language);
    }

    [TestMethod]
    public void ParsesTheTextWithinARange()
    {
        var sourceCode = "<span>hi</span><script>console.log('sup');</script>";
        using var parser = new Parser(_html);
        using var tree = parser.Parse(sourceCode)!;
        var scriptContentNode = tree.RootNode.Children[1].Children[1];
        Assert.AreEqual("raw_text", scriptContentNode.Type);

        parser.Language = _javaScript;
        Assert.AreEqual(1, parser.IncludedRanges.Count);
        Assert.AreEqual(new Point(0, 0), parser.IncludedRanges[0].StartPosition);
        Assert.AreEqual(new Point(int.MaxValue, int.MaxValue), parser.IncludedRanges[0].EndPosition);
        Assert.AreEqual(0, parser.IncludedRanges[0].StartIndex);
        Assert.AreEqual(int.MaxValue, parser.IncludedRanges[0].EndIndex);

        List<TreeSitter.Range> ranges = [scriptContentNode.Range];
        parser.IncludedRanges = ranges;
        using var jsTree = parser.Parse(sourceCode)!;
        CollectionAssert.AreEqual(ranges, parser.IncludedRanges.ToList());

        Assert.AreEqual(
            "(program (expression_statement (call_expression " +
            "function: (member_expression object: (identifier) property: (property_identifier)) " +
            "arguments: (arguments (string (string_fragment))))))",
            jsTree.RootNode.Expression);
    }

    [TestMethod]
    public void ParsesTheTextWithinMultipleRanges()
    {
        var sourceCode = "html `<div>Hello, ${name.toUpperCase()}, it's <b>${now()}</b>.</div>`";
        using var parser = new Parser(_javaScript);
        using var jsTree = parser.Parse(sourceCode)!;
        var templateStringNode = jsTree.RootNode.GetDescendantForIndex(
            sourceCode.IndexOf("`<"),
            sourceCode.IndexOf(">`")
        )!;
        Assert.AreEqual("template_string", templateStringNode.Type);

        var openQuoteNode = templateStringNode.Children[0];
        var interpolationNode1 = templateStringNode.Children[2];
        var interpolationNode2 = templateStringNode.Children[4];
        var closeQuoteNode = templateStringNode.Children[6];

        parser.Language = _html;
        List<TreeSitter.Range> htmlRanges =
        [
            new()
            {
                StartIndex = openQuoteNode.EndIndex,
                StartPosition = openQuoteNode.EndPosition,
                EndIndex = interpolationNode1.StartIndex,
                EndPosition = interpolationNode1.StartPosition,
            },
            new()
            {
                StartIndex = interpolationNode1.EndIndex,
                StartPosition = interpolationNode1.EndPosition,
                EndIndex = interpolationNode2.StartIndex,
                EndPosition = interpolationNode2.StartPosition,
            },
            new()
            {
                StartIndex = interpolationNode2.EndIndex,
                StartPosition = interpolationNode2.EndPosition,
                EndIndex = closeQuoteNode.StartIndex,
                EndPosition =closeQuoteNode.StartPosition,
            },
        ];

        parser.IncludedRanges = htmlRanges;
        using var htmlTree = parser.Parse(sourceCode)!;
        CollectionAssert.AreEqual(htmlRanges, parser.IncludedRanges.ToList());

        Assert.AreEqual(
            "(document (element" +
            " (start_tag (tag_name))" +
            " (text)" +
            " (element (start_tag (tag_name)) (end_tag (tag_name)))" +
            " (text)" +
            " (end_tag (tag_name))))",
            htmlTree.RootNode.Expression);


        var divElementNode = htmlTree.RootNode.Children[0];
        var helloTextNode = divElementNode.Children[1];
        var bElementNode = divElementNode.Children[2];
        var bStartTagNode = bElementNode.Children[0];
        var bEndTagNode = bElementNode.Children[1];

        Assert.AreEqual("text", helloTextNode.Type);
        Assert.AreEqual(sourceCode.IndexOf("Hello"), helloTextNode.StartIndex);
        Assert.AreEqual(sourceCode.IndexOf(" <b>"), helloTextNode.EndIndex);

        Assert.AreEqual("start_tag", bStartTagNode.Type);
        Assert.AreEqual(sourceCode.IndexOf("<b>"), bStartTagNode.StartIndex);
        Assert.AreEqual(sourceCode.IndexOf("${now()}"), bStartTagNode.EndIndex);

        Assert.AreEqual("end_tag", bEndTagNode.Type);
        Assert.AreEqual(sourceCode.IndexOf("</b>"), bEndTagNode.StartIndex);
        Assert.AreEqual(sourceCode.IndexOf(".</div>"), bEndTagNode.EndIndex);
    }

    [TestMethod]
    public void AnIncludedRangeContainingMismatchedPositions()
    {
        var sourceCode = "<div>test</div>{_ignore_this_part_}";
        using var parser = new Parser(_html);

        var endIndex = sourceCode.IndexOf("{_ignore_this_part_");

        TreeSitter.Range rangeToParse = new()
        {
            StartIndex = 0,
            StartPosition = new() { Row = 10, Column = 12 },
            EndIndex = endIndex,
            EndPosition = new() { Row = 10, Column = 12 + endIndex },
        };

        parser.IncludedRanges = [rangeToParse];
        var htmlTree = parser.Parse(sourceCode)!;

        Assert.AreEqual(rangeToParse, htmlTree.IncludedRanges[0]);
        Assert.AreEqual("(document (element (start_tag (tag_name)) (text) (end_tag (tag_name))))", htmlTree.RootNode.Expression);
    }

    [TestMethod]
    public void ReadsFromTheGivenInput()
    {
        var sourceCode = "first_second_third";
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse(sourceCode)!;
        Assert.AreEqual("(program (expression_statement (identifier)))", tree.RootNode.Expression);
    }

    [TestMethod]
    public void CanUseTheBashParser()
    {
        using var parser = new Parser(new("bash"));
        using var tree = parser.Parse("FOO=bar echo <<EOF 2> err.txt > hello.txt \nhello${FOO}\nEOF")!;
        Assert.AreEqual(
            "(program " +
            "(redirected_statement " +
            "body: (command " +
            "(variable_assignment name: (variable_name) value: (word)) " +
            "name: (command_name (word))) " +
            "redirect: (heredoc_redirect (heredoc_start) " +
            "redirect: (file_redirect descriptor: (file_descriptor) destination: (word)) " +
            "redirect: (file_redirect destination: (word)) " +
            "(heredoc_body " +
            "(expansion (variable_name)) (heredoc_content)) (heredoc_end))))",
            tree.RootNode.Expression);
    }

    [TestMethod]
    public void CanUseTheCppParser()
    {
        using var parser = new Parser(new("cpp"));
        using var tree = parser.Parse("const char *s = R\"EOF(HELLO WORLD)EOF\";")!;
        Assert.AreEqual(
            "(translation_unit (declaration " +
            "(type_qualifier) " +
            "type: (primitive_type) " +
            "declarator: (init_declarator " +
            "declarator: (pointer_declarator declarator: (identifier)) " +
            "value: (raw_string_literal delimiter: (raw_string_delimiter) (raw_string_content) (raw_string_delimiter)))))",
            tree.RootNode.Expression);
    }

    [TestMethod]
    public void CanUseTheHtmlParser()
    {
        using var parser = new Parser(new("HTML"));
        using var tree = parser.Parse("<div><span><custom></custom></span></div>")!;
        Assert.AreEqual(
            "(document (element (start_tag (tag_name)) (element (start_tag (tag_name)) " +
            "(element (start_tag (tag_name)) (end_tag (tag_name))) (end_tag (tag_name))) (end_tag (tag_name))))",
            tree.RootNode.Expression);
    }

    [TestMethod]
    public void CanUseThePythonParser()
    {
        using var parser = new Parser(new("Python"));
        using var tree = parser.Parse("class A:\n  def b():\n    c()")!;
        Assert.AreEqual(
            "(module (class_definition " +
            "name: (identifier) " +
            "body: (block " +
            "(function_definition " +
            "name: (identifier) " +
            "parameters: (parameters) " +
            "body: (block (expression_statement (call " +
            "function: (identifier) " +
            "arguments: (argument_list))))))))",
            tree.RootNode.Expression);
    }

    [TestMethod]
    public void CanUseTheRustParser()
    {
        using var parser = new Parser(new("rust"));
        using var tree = parser.Parse("const x: &'static str = r###\"hello\"###;")!;
        Assert.AreEqual(
            "(source_file (const_item " +
            "name: (identifier) " +
            "type: (reference_type (lifetime (identifier)) type: (primitive_type)) " +
            "value: (raw_string_literal (string_content))))",
            tree.RootNode.Expression);
    }

    [TestMethod]
    public void CanUseTheTypeScriptParser()
    {
        using var parser = new Parser(new("TypeScript"));
        using var tree = parser.Parse("a()\nb()\n[c]")!;
        Assert.AreEqual(
            "(program " +
            "(expression_statement (call_expression function: (identifier) arguments: (arguments))) " +
            "(expression_statement (subscript_expression " +
            "object: (call_expression " +
            "function: (identifier) " +
            "arguments: (arguments)) " +
            "index: (identifier))))",
            tree.RootNode.Expression);
    }

    [TestMethod]
    public void ParsesOnlyTheTextWithinTheIncludedRangesIfTheyAreSpecified()
    {
        var sourceCode = "<% foo() %> <% bar %>";

        var start1 = sourceCode.IndexOf("foo");
        var end1 = start1 + 5;
        var start2 = sourceCode.IndexOf("bar");
        var end2 = start2 + 3;

        using var parser = new Parser(new("JavaScript"));
        parser.IncludedRanges =
        [
            new()
            {
                StartIndex = start1,
                EndIndex = end1,
                StartPosition = new() { Row = 0, Column = start1 },
                EndPosition = new() { Row = 0, Column = end1 },
            },
            new()
            {
                StartIndex = start2,
                EndIndex = end2,
                StartPosition = new() { Row = 0, Column = start2 },
                EndPosition = new() { Row = 0, Column = end2 },
            },
        ];
        using var tree = parser.Parse(sourceCode)!;
        Assert.AreEqual(
            "(program " +
            "(expression_statement (call_expression function: (identifier) arguments: (arguments))) " +
            "(expression_statement (identifier)))",
            tree.RootNode.Expression);
    }

    readonly Language _html = new("HTML");
    readonly Language _javaScript = new("JavaScript");
}
