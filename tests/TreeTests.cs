//
// Tests for .NET bindings for tree-sitter
// Adapted from official tree-sitter web bindings tests 'tree.test.ts'
// Copyright (c) 2025 Marius Greuel
// Copyright (c) 2018-2024 Max Brunsfeld
// SPDX-License-Identifier: MIT
//

namespace TreeSitter.Tests;

[TestClass]
public class TreeTests
{
    [TestMethod]
    public void UpdatesThePositionsOfNodes()
    {
        var input = "abc + cde";

        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse(input)!;
        Assert.AreEqual("(program (expression_statement (binary_expression left: (identifier) right: (identifier))))", tree.RootNode.Expression);

        var sumNode = tree.RootNode.FirstChild!.FirstChild!;
        var left = sumNode["left"];
        var right = sumNode["right"];
        Assert.AreEqual(0, left.StartIndex);
        Assert.AreEqual(3, left.EndIndex);
        Assert.AreEqual(6, right.StartIndex);
        Assert.AreEqual(9, right.EndIndex);

        (input, var edit) = SpliceInput(input, input.IndexOf("bc"), 0, " * ");
        Assert.AreEqual("a * bc + cde", input);
        tree.Edit(edit);

        sumNode = tree.RootNode.FirstChild!.FirstChild!;
        left = sumNode["left"];
        right = sumNode["right"];
        Assert.AreEqual(0, left.StartIndex);
        Assert.AreEqual(6, left.EndIndex);
        Assert.AreEqual(9, right.StartIndex);
        Assert.AreEqual(12, right.EndIndex);

        using var treeEdit = parser.Parse(input, tree)!;
        Assert.AreEqual("(program (expression_statement (binary_expression left: (binary_expression left: (identifier) right: (identifier)) right: (identifier))))", treeEdit.RootNode.Expression);
    }

    [TestMethod]
    public void HandlesNonAsciiCharacters()
    {
        var input = "αβδ + cde";

        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse(input)!;
        Assert.AreEqual("(program (expression_statement (binary_expression left: (identifier) right: (identifier))))", tree.RootNode.Expression);

        var variableNode = tree.RootNode.FirstChild!.FirstChild!.LastChild!;
        Assert.AreEqual(6, variableNode.StartIndex);
        Assert.AreEqual(9, variableNode.EndIndex);
        Assert.AreEqual("cde", variableNode.Text);

        (input, var edit) = SpliceInput(input, input.IndexOf('δ'), 0, "👍 * ");
        Assert.AreEqual("αβ👍 * δ + cde", input);
        tree.Edit(edit);

        variableNode = tree.RootNode.FirstChild!.FirstChild!.LastChild!;
        Assert.AreEqual(input.IndexOf("cde"), variableNode.StartIndex);

        using var treeEdit = parser.Parse(input, tree)!;
        Assert.AreEqual("(program (expression_statement (binary_expression left: (binary_expression left: (identifier) right: (identifier)) right: (identifier))))", treeEdit.RootNode.Expression);
        Assert.AreEqual("cde", treeEdit.RootNode.FirstChild!.FirstChild!.LastChild!.Text);
    }

    [TestMethod]
    public void ReportsTheRangesOfTextWhoseSyntacticMeaningHasChanged()
    {
        var input = "abcdefg + hij";

        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse(input)!;
        Assert.AreEqual("(program (expression_statement (binary_expression left: (identifier) right: (identifier))))", tree.RootNode.Expression);

        input = "abc + defg + hij";
        tree.Edit(new()
        {
            StartIndex = 2,
            OldEndIndex = 2,
            NewEndIndex = 5,
            StartPosition = new() { Row = 0, Column = 2 },
            OldEndPosition = new() { Row = 0, Column = 2 },
            NewEndPosition = new() { Row = 0, Column = 5 },
        });

        using var tree2 = parser.Parse(input, tree)!;
        Assert.AreEqual("(program (expression_statement (binary_expression left: (binary_expression left: (identifier) right: (identifier)) right: (identifier))))", tree2.RootNode.Expression);

        var ranges = tree.GetChangedRanges(tree2);
        Assert.AreEqual(1, ranges.Count);
        Assert.AreEqual(0, ranges[0].StartIndex);
        Assert.AreEqual("abc + defg".Length, ranges[0].EndIndex);
        Assert.AreEqual(new(0, 0), ranges[0].StartPosition);
        Assert.AreEqual(new(0, "abc + defg".Length), ranges[0].EndPosition);
    }

    [TestMethod]
    public void ReturnsACursorThatCanBeUsedToWalkTheTree()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse("a * b + c / d")!;
        using var cursor = tree.Walk();

        Assert.AreEqual("a * b + c / d", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "program", true, new Point(0, 0), new Point(0, 13), 0, 13);

        Assert.IsTrue(cursor.GotoFirstChild());
        Assert.AreEqual("a * b + c / d", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "expression_statement", true, new Point(0, 0), new Point(0, 13), 0, 13);

        Assert.IsTrue(cursor.GotoFirstChild());
        Assert.AreEqual("a * b + c / d", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "binary_expression", true, new Point(0, 0), new Point(0, 13), 0, 13);

        Assert.IsTrue(cursor.GotoFirstChild());
        Assert.AreEqual("a * b", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "binary_expression", true, new Point(0, 0), new Point(0, 5), 0, 5);

        Assert.IsTrue(cursor.GotoFirstChild());
        Assert.AreEqual("a", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "identifier", true, new Point(0, 0), new Point(0, 1), 0, 1);

        Assert.IsFalse(cursor.GotoFirstChild());

        Assert.IsTrue(cursor.GotoNextSibling());
        Assert.AreEqual("*", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "*", false, new Point(0, 2), new Point(0, 3), 2, 3);

        Assert.IsTrue(cursor.GotoNextSibling());
        Assert.AreEqual("b", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "identifier", true, new Point(0, 4), new Point(0, 5), 4, 5);

        Assert.IsFalse(cursor.GotoNextSibling());

        Assert.IsTrue(cursor.GotoParent());
        Assert.AreEqual("a * b", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "binary_expression", true, new Point(0, 0), new Point(0, 5), 0, 5);

        Assert.IsTrue(cursor.GotoNextSibling());
        Assert.AreEqual("+", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "+", false, new Point(0, 6), new Point(0, 7), 6, 7);

        Assert.IsTrue(cursor.GotoNextSibling());
        Assert.AreEqual("c / d", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "binary_expression", true, new Point(0, 8), new Point(0, 13), 8, 13);

        using var copy = tree.Walk();
        copy.ResetTo(cursor);
        Assert.AreEqual("c / d", copy.CurrentNode.Text);
        AssertCursorState(copy, "binary_expression", true, new Point(0, 8), new Point(0, 13), 8, 13);

        Assert.IsTrue(copy.GotoPreviousSibling());
        Assert.AreEqual("+", copy.CurrentNode.Text);
        AssertCursorState(copy, "+", false, new Point(0, 6), new Point(0, 7), 6, 7);

        Assert.IsTrue(copy.GotoPreviousSibling());
        Assert.AreEqual("a * b", copy.CurrentNode.Text);
        AssertCursorState(copy, "binary_expression", true, new Point(0, 0), new Point(0, 5), 0, 5);

        Assert.IsTrue(copy.GotoLastChild());
        Assert.AreEqual("b", copy.CurrentNode.Text);
        AssertCursorState(copy, "identifier", true, new Point(0, 4), new Point(0, 5), 4, 5);

        Assert.IsTrue(copy.GotoParent());
        Assert.AreEqual("a * b", copy.CurrentNode.Text);
        AssertCursorState(copy, "binary_expression", true, new Point(0, 0), new Point(0, 5), 0, 5);

        Assert.IsTrue(copy.GotoParent());
        Assert.AreEqual("a * b + c / d", copy.CurrentNode.Text);
        AssertCursorState(copy, "binary_expression", true, new Point(0, 0), new Point(0, 13), 0, 13);

        Assert.IsTrue(copy.GotoParent());
        Assert.AreEqual("a * b + c / d", copy.CurrentNode.Text);
        AssertCursorState(copy, "expression_statement", true, new Point(0, 0), new Point(0, 13), 0, 13);

        Assert.IsTrue(copy.GotoParent());
        Assert.AreEqual("a * b + c / d", copy.CurrentNode.Text);
        AssertCursorState(copy, "program", true, new Point(0, 0), new Point(0, 13), 0, 13);

        Assert.IsFalse(copy.GotoParent());

        Assert.IsTrue(cursor.GotoParent());
        Assert.AreEqual("a * b + c / d", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "binary_expression", true, new Point(0, 0), new Point(0, 13), 0, 13);

        Assert.IsTrue(cursor.GotoParent());
        Assert.AreEqual("a * b + c / d", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "expression_statement", true, new Point(0, 0), new Point(0, 13), 0, 13);

        Assert.IsTrue(cursor.GotoParent());
        Assert.AreEqual("a * b + c / d", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "program", true, new Point(0, 0), new Point(0, 13), 0, 13);
    }

    [TestMethod]
    public void KeepsTrackOfTheFieldNameAssociatedWithEachNode()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse("a.b();")!;
        using var cursor = tree.Walk();

        Assert.IsTrue(cursor.GotoFirstChild());

        Assert.IsTrue(cursor.GotoFirstChild());
        Assert.AreEqual("a.b()", cursor.CurrentNode.Text);
        Assert.AreEqual("call_expression", cursor.CurrentNode.Type);
        Assert.IsNull(cursor.CurrentFieldName);

        Assert.IsTrue(cursor.GotoFirstChild());
        Assert.AreEqual("a.b", cursor.CurrentNode.Text);
        Assert.AreEqual("member_expression", cursor.CurrentNode.Type);
        Assert.AreEqual("function", cursor.CurrentFieldName);

        Assert.IsTrue(cursor.GotoFirstChild());
        Assert.AreEqual("a", cursor.CurrentNode.Text);
        Assert.AreEqual("identifier", cursor.CurrentNode.Type);
        Assert.AreEqual("object", cursor.CurrentFieldName);

        Assert.IsTrue(cursor.GotoNextSibling());
        Assert.AreEqual(".", cursor.CurrentNode.Text);
        Assert.AreEqual(".", cursor.CurrentNode.Type);
        Assert.IsNull(cursor.CurrentFieldName);

        Assert.IsTrue(cursor.GotoNextSibling());
        Assert.AreEqual("b", cursor.CurrentNode.Text);
        Assert.AreEqual("property_identifier", cursor.CurrentNode.Type);
        Assert.AreEqual("property", cursor.CurrentFieldName);

        Assert.IsTrue(cursor.GotoParent());
        Assert.IsTrue(cursor.GotoNextSibling());
        Assert.AreEqual("()", cursor.CurrentNode.Text);
        Assert.AreEqual("arguments", cursor.CurrentNode.Type);
        Assert.AreEqual("arguments", cursor.CurrentFieldName);
    }

    [TestMethod]
    public void ReturnsACursorThatCanBeResetAnywhereInTheTree()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse("a * b + c / d")!;
        using var cursor = tree.Walk();

        cursor.Reset(tree.RootNode.FirstChild!.FirstChild!.FirstChild!);
        Assert.AreEqual("a * b", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "binary_expression", true, new Point(0, 0), new Point(0, 5), 0, 5);

        Assert.IsTrue(cursor.GotoFirstChild());
        Assert.AreEqual("a", cursor.CurrentNode.Text);
        AssertCursorState(cursor, "identifier", true, new Point(0, 0), new Point(0, 1), 0, 1);

        Assert.IsTrue(cursor.GotoParent());
        Assert.IsFalse(cursor.GotoParent());
    }

    static void AssertCursorState(TreeCursor cursor, string type, bool isNamed, Point startPosition, Point endPosition, int startIndex, int endIndex)
    {
        Assert.AreEqual(type, cursor.CurrentNode.Type);
        Assert.AreEqual(isNamed, cursor.CurrentNode.IsNamed);
        Assert.AreEqual(startPosition, cursor.CurrentNode.StartPosition);
        Assert.AreEqual(endPosition, cursor.CurrentNode.EndPosition);
        Assert.AreEqual(startIndex, cursor.CurrentNode.StartIndex);
        Assert.AreEqual(endIndex, cursor.CurrentNode.EndIndex);
    }

    static (string, Edit) SpliceInput(string input, int startIndex, int lengthRemoved, string newText)
    {
        var oldEndIndex = startIndex + lengthRemoved;
        var newEndIndex = startIndex + newText.Length;
        var startPosition = GetExtent(input[..startIndex]);
        var oldEndPosition = GetExtent(input[..oldEndIndex]);
        input = input[..startIndex] + newText + input[oldEndIndex..];
        var newEndPosition = GetExtent(input[..newEndIndex]);

        return (input, new Edit()
        {
            StartIndex = startIndex,
            OldEndIndex = oldEndIndex,
            NewEndIndex = newEndIndex,
            StartPosition = startPosition,
            OldEndPosition = oldEndPosition,
            NewEndPosition = newEndPosition,
        });
    }

    static Point GetExtent(string text)
    {
        var lines = text.Split('\n');
        return new Point(lines.Length - 1, lines[^1].Length);
    }

    readonly Language _javaScript = new("JavaScript");
}
