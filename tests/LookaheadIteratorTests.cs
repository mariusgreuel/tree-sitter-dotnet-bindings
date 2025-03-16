//
// Tests for .NET bindings for tree-sitter
// Adapted from official tree-sitter web bindings tests 'language.test.ts'
// Copyright (c) 2025 Marius Greuel
// Copyright (c) 2018-2024 Max Brunsfeld
// SPDX-License-Identifier: MIT
//

namespace TreeSitter.Tests;

[TestClass]
public class LookaheadIteratorTests
{
    readonly Language _javaScript = new("JavaScript");
    readonly List<string> _expected = ["(", "identifier", "*", "formal_parameters", "html_comment", "comment"];

    ushort _state;
    LookaheadIterator _lookahead = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse("function fn() {}")!;
        using var cursor = tree.Walk();
        Assert.IsTrue(cursor.GotoFirstChild());
        Assert.IsTrue(cursor.GotoFirstChild());
        _state = cursor.CurrentNode.NextParseState;
        _lookahead = _javaScript.CreateLookaheadIterator(_state)!;
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _lookahead.Dispose();
    }

    [TestMethod]
    public void IteratesOverValidSymbolsInState()
    {
        CollectionAssert.AreEqual(_expected, _lookahead.Symbols.Select(s => s.Name).ToList());
    }

    [TestMethod]
    public void ResetsToInitialState()
    {
        CollectionAssert.AreEqual(_expected, _lookahead.Symbols.Select(s => s.Name).ToList());
        Assert.IsTrue(_lookahead.ResetState(_state));
        CollectionAssert.AreEqual(_expected, _lookahead.Symbols.Select(s => s.Name).ToList());
    }

    [TestMethod]
    public void Resets()
    {
        Assert.IsTrue(_lookahead.Reset(_javaScript, _state));
        CollectionAssert.AreEqual(_expected, _lookahead.Symbols.Select(s => s.Name).ToList());
    }
}
