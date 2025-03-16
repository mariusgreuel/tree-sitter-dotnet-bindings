//
// Tests for .NET bindings for tree-sitter
// Adapted from official tree-sitter web bindings tests 'query.test.ts'
// Copyright (c) 2025 Marius Greuel
// Copyright (c) 2018-2024 Max Brunsfeld
// SPDX-License-Identifier: MIT
//

namespace TreeSitter.Tests;

static class Helpers
{
    public static string TrimAndRemoveCR(this string text)
    {
        return text.Replace("\r", "").Trim();
    }
}

[TestClass]
public class QueryTests
{
    static string FormatCapture(QueryCapture capture)
    {
        return $"{capture.PatternIndex}: {capture.Name} => '{capture.Node.Text}'";
    }

    static string FormatProperties(IReadOnlyDictionary<string, string?>? properties)
    {
        if (properties == null)
            return "";

        string result = "";

        foreach (var property in properties)
        {
            result += result.Length == 0 ? "" : ", ";
            result += $"{property.Key}: {(property.Value != null ? ("'" + property.Value + "'") : "null")}";
        }

        return result;
    }

    [TestMethod]
    public void ReturnsAllOfTheMatchesForTheGivenQuery()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse("function one() { two(); function three() {} }")!;
        var query = new Query(_javaScript, @"
                (function_declaration name: (identifier) @fn-def)
                (call_expression function: (identifier) @fn-ref)
            ");
        var matches = query.Execute(tree.RootNode).Matches.ToList();
        Assert.AreEqual(3, matches.Count);
        Assert.AreEqual("0: fn-def => 'one'", FormatCapture(matches[0].Captures[0]));
        Assert.AreEqual("1: fn-ref => 'two'", FormatCapture(matches[1].Captures[0]));
        Assert.AreEqual("0: fn-def => 'three'", FormatCapture(matches[2].Captures[0]));
    }

    [TestMethod]
    public void CanSearchInSpecifiedIndexRanges()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse("[a, b,\nc, d,\ne, f,\ng, h]")!;
        var query = new Query(_javaScript, "(identifier) @element");
        var matches = query.Execute(tree.RootNode, new() { StartIndex = 8, EndIndex = 20 }).Matches.ToList();
        Assert.AreEqual(4, matches.Count);
        Assert.AreEqual("0: element => 'd'", FormatCapture(matches[0].Captures[0]));
        Assert.AreEqual("0: element => 'e'", FormatCapture(matches[1].Captures[0]));
        Assert.AreEqual("0: element => 'f'", FormatCapture(matches[2].Captures[0]));
        Assert.AreEqual("0: element => 'g'", FormatCapture(matches[3].Captures[0]));
    }

    [TestMethod]
    public void CanSearchInSpecifiedPositionRanges()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse("[a, b,\nc, d,\ne, f,\ng, h]")!;
        var query = new Query(_javaScript, "(identifier) @element");
        var matches = query.Execute(tree.RootNode, new() { StartPoint = new Point(1, 1), EndPoint = new Point(3, 1) }).Matches.ToList();
        Assert.AreEqual(4, matches.Count);
        Assert.AreEqual("0: element => 'd'", FormatCapture(matches[0].Captures[0]));
        Assert.AreEqual("0: element => 'e'", FormatCapture(matches[1].Captures[0]));
        Assert.AreEqual("0: element => 'f'", FormatCapture(matches[2].Captures[0]));
        Assert.AreEqual("0: element => 'g'", FormatCapture(matches[3].Captures[0]));
    }

    [TestMethod]
    public void HandlesPredicatesThatCompareTheTextOfCaptureToLiteralStrings()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse(@"
                giraffe(1, 2, []);
                helment([false]);
                goat(false);
                gross(3, []);
                hiccup([]);
                gaff(5);
            ")!;

        // Find all calls to functions beginning with 'g', where one argument is an array literal.
        var query = new Query(_javaScript, @"
                (call_expression
                  function: (identifier) @name
                  arguments: (arguments (array))
                  (#match? @name ""^g""))
            ");
        var matches = query.Execute(tree.RootNode).Matches.ToList();
        Assert.AreEqual(2, matches.Count);
        Assert.AreEqual("0: name => 'giraffe'", FormatCapture(matches[0].Captures[0]));
        Assert.AreEqual("0: name => 'gross'", FormatCapture(matches[1].Captures[0]));
    }

    [TestMethod]
    public void HandlesMultipleMatchesWhereTheFirstOneIsFiltered()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse(@"
                const a = window.b;
            ")!;

        // Find all calls to functions beginning with 'g', where one argument is an array literal.
        var query = new Query(_javaScript, @"
                ((identifier) @variable.builtin
                  (#match? @variable.builtin ""^(arguments|module|console|window|document)$"")
                  (#is-not? local))
            ");
        var matches = query.Execute(tree.RootNode).Matches.ToList();
        Assert.AreEqual(1, matches.Count);
        Assert.AreEqual("0: variable.builtin => 'window'", FormatCapture(matches[0].Captures[0]));
    }

    [TestMethod]
    public void ReturnsAllOfTheCapturesForTheGivenQueryInOrder()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse(@"
                a({
                  bc: function de() {
                    const fg = function hi() {}
                  },
                  jk: function lm() {
                    const no = function pq() {}
                  },
                });
            ")!;
        var query = new Query(_javaScript, @"
                (pair
                  key: _ @method.def
                  (function_expression
                    name: (identifier) @method.alias))

                (variable_declarator
                  name: _ @function.def
                  value: (function_expression
                    name: (identifier) @function.alias))

                "":"" @delimiter
                ""="" @operator
            ");
        var captures = query.Execute(tree.RootNode).Captures.ToList();
        Assert.AreEqual(12, captures.Count);
        Assert.AreEqual("0: method.def => 'bc'", FormatCapture(captures[0]));
        Assert.AreEqual("2: delimiter => ':'", FormatCapture(captures[1]));
        Assert.AreEqual("0: method.alias => 'de'", FormatCapture(captures[2]));
        Assert.AreEqual("1: function.def => 'fg'", FormatCapture(captures[3]));
        Assert.AreEqual("3: operator => '='", FormatCapture(captures[4]));
        Assert.AreEqual("1: function.alias => 'hi'", FormatCapture(captures[5]));
        Assert.AreEqual("0: method.def => 'jk'", FormatCapture(captures[6]));
        Assert.AreEqual("2: delimiter => ':'", FormatCapture(captures[7]));
        Assert.AreEqual("0: method.alias => 'lm'", FormatCapture(captures[8]));
        Assert.AreEqual("1: function.def => 'no'", FormatCapture(captures[9]));
        Assert.AreEqual("3: operator => '='", FormatCapture(captures[10]));
        Assert.AreEqual("1: function.alias => 'pq'", FormatCapture(captures[11]));
    }

    [TestMethod]
    public void HandlesConditionsThatCompareTheTextOfCaptureToLiteralStrings()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse(@"
                lambda
                panda
                load
                toad
                const ab = require('./ab');
                new Cd(EF);
            ")!;
        var query = new Query(_javaScript, @"
                ((identifier) @variable
                 (#not-match? @variable ""^(lambda|load)$""))

                ((identifier) @function.builtin
                 (#eq? @function.builtin ""require""))

                ((identifier) @constructor
                 (#match? @constructor ""^[A-Z]""))

                ((identifier) @constant
                 (#match? @constant ""^[A-Z]{2,}$""))
            ");
        var captures = query.Execute(tree.RootNode).Captures.ToList();
        Assert.AreEqual(10, captures.Count);
        Assert.AreEqual("0: variable => 'panda'", FormatCapture(captures[0]));
        Assert.AreEqual("0: variable => 'toad'", FormatCapture(captures[1]));
        Assert.AreEqual("0: variable => 'ab'", FormatCapture(captures[2]));
        Assert.AreEqual("0: variable => 'require'", FormatCapture(captures[3]));
        Assert.AreEqual("1: function.builtin => 'require'", FormatCapture(captures[4]));
        Assert.AreEqual("0: variable => 'Cd'", FormatCapture(captures[5]));
        Assert.AreEqual("2: constructor => 'Cd'", FormatCapture(captures[6]));
        Assert.AreEqual("0: variable => 'EF'", FormatCapture(captures[7]));
        Assert.AreEqual("2: constructor => 'EF'", FormatCapture(captures[8]));
        Assert.AreEqual("3: constant => 'EF'", FormatCapture(captures[9]));
    }

    [TestMethod]
    public void HandlesConditionsThatCompareTheTextOfCaptureToEachOther()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse(@"
                ab = abc + 1;
                def = de + 1;
                ghi = ghi + 1;
            ")!;
        var query = new Query(_javaScript, @"
                (
                  (assignment_expression
                    left: (identifier) @id1
                    right: (binary_expression
                      left: (identifier) @id2))
                  (#eq? @id1 @id2)
                )
            ");
        var captures = query.Execute(tree.RootNode).Captures.ToList();
        Assert.AreEqual(2, captures.Count);
        Assert.AreEqual("0: id1 => 'ghi'", FormatCapture(captures[0]));
        Assert.AreEqual("0: id2 => 'ghi'", FormatCapture(captures[1]));
    }

    [TestMethod]
    public void HandlesPatternsWithProperties()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse("a(b.c);")!;
        var query = new Query(_javaScript, @"
                ((call_expression (identifier) @func)
                 (#set! foo)
                 (#set! bar baz))

                ((property_identifier) @prop
                 (#is? foo)
                 (#is-not? bar baz))
            ");
        var queryCursor = query.Execute(tree.RootNode);
        var captures = queryCursor.Captures.ToList();
        Assert.IsFalse(queryCursor.IsMatchLimitExceeded);
        Assert.AreEqual(2, captures.Count);
        Assert.AreEqual("0: func => 'a'", FormatCapture(captures[0]));
        Assert.AreEqual("foo: null, bar: 'baz'", FormatProperties(captures[0].SetProperties));
        Assert.AreEqual("1: prop => 'c'", FormatCapture(captures[1]));
        Assert.AreEqual("foo: null", FormatProperties(captures[1].AssertedProperties));
        Assert.AreEqual("bar: 'baz'", FormatProperties(captures[1].RefutedProperties));
    }

    [TestMethod]
    public void DetectsQueriesWithTooManyPermutationsToTrack()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse(@"
                [
                  hello, hello, hello, hello, hello, hello, hello, hello, hello, hello,
                  hello, hello, hello, hello, hello, hello, hello, hello, hello, hello,
                  hello, hello, hello, hello, hello, hello, hello, hello, hello, hello,
                  hello, hello, hello, hello, hello, hello, hello, hello, hello, hello,
                  hello, hello, hello, hello, hello, hello, hello, hello, hello, hello,
                ];
            ")!;
        var query = _javaScript.CreateQuery("(array (identifier) @pre (identifier) @post)");
        var queryCursor = query.Execute(tree.RootNode, new() { MatchLimit = 32 });
        Assert.IsFalse(queryCursor.IsMatchLimitExceeded);
        var first = queryCursor.Captures.First();
        Assert.IsTrue(queryCursor.IsMatchLimitExceeded);
    }

    [TestMethod]
    public void HandlesQuantifiedCapturesProperly()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse(@"
                /// foo
                /// bar
                /// baz
            ")!;

        int GetCaptureCount(string query)
        {
            return _javaScript.CreateQuery(query).Execute(tree.RootNode).Captures.ToList().Count;
        }

        Assert.AreEqual(3, GetCaptureCount("((comment)+ @foo (#any-eq? @foo \"/// foo\"))"));
        Assert.AreEqual(0, GetCaptureCount("((comment)+ @foo (#eq? @foo \"/// foo\"))"));
        Assert.AreEqual(3, GetCaptureCount("((comment)+ @foo (#any-not-eq? @foo \"/// foo\"))"));
        Assert.AreEqual(0, GetCaptureCount("((comment)+ @foo (#not-eq? @foo \"/// foo\"))"));
        Assert.AreEqual(0, GetCaptureCount("((comment)+ @foo (#match? @foo \"^/// foo\"))"));
        Assert.AreEqual(3, GetCaptureCount("((comment)+ @foo (#any-match? @foo \"^/// foo\"))"));
        Assert.AreEqual(0, GetCaptureCount("((comment)+ @foo (#not-match? @foo \"^/// foo\"))"));
        Assert.AreEqual(3, GetCaptureCount("((comment)+ @foo (#not-match? @foo \"fsdfsdafdfs\"))"));
        Assert.AreEqual(0, GetCaptureCount("((comment)+ @foo (#any-not-match? @foo \"^///\"))"));
        Assert.AreEqual(3, GetCaptureCount("((comment)+ @foo (#any-not-match? @foo \"^/// foo\"))"));
    }

    [TestMethod]
    public void ReturnsAllOfThePredicatesAsObjects()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse(@"
                (
                  (binary_expression
                    left: (identifier) @a
                    right: (identifier) @b)
                  (#something? @a @b)
                  (#match? @a ""c"")
                  (#something-else? @a ""A"" @b ""B"")
                )

                ((identifier) @c
                 (#hello! @c))

                ""if"" @d
            ")!;
        var query = _javaScript.CreateQuery("(array (identifier) @pre (identifier) @post)");
    }

    [TestMethod]
    public void DisableCapture()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse("function foo() { return 1; }")!;
        var query = _javaScript.CreateQuery(@"
                (function_declaration
                  (identifier) @name1 @name2 @name3
                  (statement_block) @body1 @body2)
            ");
        var matches = query.Execute(tree.RootNode).Matches.ToList();
        Assert.AreEqual(1, matches.Count);
        Assert.AreEqual(5, matches[0].Captures.Count);
        Assert.AreEqual("0: name1 => 'foo'", FormatCapture(matches[0].Captures[0]));
        Assert.AreEqual("0: name2 => 'foo'", FormatCapture(matches[0].Captures[1]));
        Assert.AreEqual("0: name3 => 'foo'", FormatCapture(matches[0].Captures[2]));
        Assert.AreEqual("0: body1 => '{ return 1; }'", FormatCapture(matches[0].Captures[3]));
        Assert.AreEqual("0: body2 => '{ return 1; }'", FormatCapture(matches[0].Captures[4]));

        query.DisableCapture("name2");
        matches = query.Execute(tree.RootNode).Matches.ToList();
        Assert.AreEqual(1, matches.Count);
        Assert.AreEqual(4, matches[0].Captures.Count);
        Assert.AreEqual("0: name1 => 'foo'", FormatCapture(matches[0].Captures[0]));
        Assert.AreEqual("0: name3 => 'foo'", FormatCapture(matches[0].Captures[1]));
        Assert.AreEqual("0: body1 => '{ return 1; }'", FormatCapture(matches[0].Captures[2]));
        Assert.AreEqual("0: body2 => '{ return 1; }'", FormatCapture(matches[0].Captures[3]));
    }

    [TestMethod]
    public void DisablePattern()
    {
        using var parser = new Parser(_javaScript);
        using var tree = parser.Parse("class A { constructor() {} } function b() { return 1; }")!;
        var query = _javaScript.CreateQuery(@"
                (function_declaration name: (identifier) @name)
                (function_declaration body: (statement_block) @body)
                (class_declaration name: (identifier) @name)
                (class_declaration body: (class_body) @body)
            ");
        query.Patterns[0].Disable();
        query.Patterns[2].Disable();
        var matches = query.Execute(tree.RootNode).Matches.ToList();
        Assert.AreEqual(2, matches.Count);
        Assert.AreEqual(1, matches[0].Captures.Count);
        Assert.AreEqual(1, matches[1].Captures.Count);
        Assert.AreEqual("3: body => '{ constructor() {} }'", FormatCapture(matches[0].Captures[0]));
        Assert.AreEqual("1: body => '{ return 1; }'", FormatCapture(matches[1].Captures[0]));
    }

    [TestMethod]
    public void ReturnsTheStartAndEndIndicesForAPattern()
    {
        string patterns1 = @"
""+"" @operator
""-"" @operator
""*"" @operator
""="" @operator
""=>"" @operator
            ".TrimAndRemoveCR();
        string patterns2 = @"
(identifier) @a
(string) @b
            ".TrimAndRemoveCR();
        string patterns3 = @"
((identifier) @b (#match? @b i))
(function_declaration name: (identifier) @c)
(method_definition name: (property_identifier) @d)
            ".TrimAndRemoveCR();
        var source = patterns1 + patterns2 + patterns3;
        var query = _javaScript.CreateQuery(source);

        Assert.AreEqual(0, query.Patterns[0].StartIndex);
        Assert.AreEqual("\"+\" @operator\n".Length, query.Patterns[0].EndIndex);

        Assert.AreEqual(patterns1.Length, query.Patterns[5].StartIndex);
        Assert.AreEqual(patterns1.Length + "(identifier) @a\n".Length, query.Patterns[5].EndIndex);

        Assert.AreEqual(patterns1.Length + patterns2.Length, query.Patterns[7].StartIndex);
        Assert.AreEqual(patterns1.Length + patterns2.Length + "((identifier) @b (#match? @b i))\n".Length, query.Patterns[7].EndIndex);
    }

    readonly Language _javaScript = new("JavaScript");
}
