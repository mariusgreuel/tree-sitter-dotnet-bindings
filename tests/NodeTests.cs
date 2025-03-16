//
// Tests for .NET bindings for tree-sitter
// Adapted from official tree-sitter web bindings tests 'node.test.ts'
// Copyright (c) 2025 Marius Greuel
// Copyright (c) 2018-2024 Max Brunsfeld
// SPDX-License-Identifier: MIT
//

namespace TreeSitter.Tests;

[TestClass]
public class NodeTests
{
    readonly Language _c = new("C");
    readonly Language _javaScript = new("JavaScript");
    readonly Language _json = new("JSON");
    //readonly Language _embeddedTemplate = new("EmbeddedTemplate");
    readonly Language _python = new("Python");
    Parser _parser = null!;
    Tree? _tree;
    private const string JSON_EXAMPLE = @"
[
  123,
  false,
  {
    ""x"": null
  }
]
";

    [TestInitialize]
    public void TestInitialize()
    {
        _parser = new Parser(_javaScript);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _tree?.Dispose();
        _parser.Dispose();
    }

    [TestMethod]
    public void Children_ReturnsArrayOfChildNodes()
    {
        _tree = _parser.Parse("x10 + 1000")!;
        Assert.AreEqual(1, _tree.RootNode.Children.Count);
        var sumNode = _tree.RootNode.FirstChild!.FirstChild!;
        CollectionAssert.AreEqual(new[] { "identifier", "+", "number" }, sumNode.Children.Select(child => child.Type).ToList());
    }

    [TestMethod]
    public void NamedChildren_ReturnsArrayOfNamedChildNodes()
    {
        _tree = _parser.Parse("x10 + 1000")!;
        var sumNode = _tree.RootNode.FirstChild!.FirstChild!;
        Assert.AreEqual(1, _tree.RootNode.NamedChildren.Count);
        CollectionAssert.AreEqual(new[] { "identifier", "number" }, sumNode.NamedChildren.Select(child => child.Type).ToList());
    }

    [TestMethod]
    public void ChildrenForFieldName_ReturnsArrayOfChildNodesForGivenFieldName()
    {
        _parser.Language = _python;
        string source = @"
                if one:
                    a()
                elif two:
                    b()
                elif three:
                    c()
                elif four:
                    d()";

        _tree = _parser.Parse(source)!;
        var node = _tree.RootNode.FirstChild!;
        Assert.AreEqual("if_statement", node.Type);

        var alternatives = node.GetChildrenForField("alternative");
        var alternativeTexts = alternatives.Select(node =>
        {
            var condition = node["condition"];
            return source[condition.StartIndex..condition.EndIndex];
        });
        CollectionAssert.AreEqual(new[] { "two", "three", "four" }, alternativeTexts.ToList());
    }

    [TestMethod]
    public void StartIndexAndEndIndex_ReturnsCharacterIndexWhereNodeStartsAndEnds()
    {
        _tree = _parser.Parse("a👍👎1 / b👎c👎")!;
        var quotientNode = _tree.RootNode.FirstChild!.FirstChild!;

        Assert.AreEqual(0, quotientNode.StartIndex);
        Assert.AreEqual(15, quotientNode.EndIndex);
        CollectionAssert.AreEqual(new[] { 0, 7, 9 }, quotientNode.Children.Select(child => child.StartIndex).ToList());
        CollectionAssert.AreEqual(new[] { 6, 8, 15 }, quotientNode.Children.Select(child => child.EndIndex).ToList());
    }

    [TestMethod]
    public void StartPositionAndEndPosition_ReturnsRowAndColumnWhereNodeStartsAndEnds()
    {
        _tree = _parser.Parse("x10 + 1000")!;
        var sumNode = _tree.RootNode.FirstChild!.FirstChild!;
        Assert.AreEqual("binary_expression", sumNode.Type);

        Assert.AreEqual(new Point(0, 0), sumNode.StartPosition);
        Assert.AreEqual(new Point(0, 10), sumNode.EndPosition);
        CollectionAssert.AreEqual(new[]
        {
            new Point(0, 0),
            new Point(0, 4),
            new Point(0, 6)
        }, sumNode.Children.Select(child => child.StartPosition).ToList());
        CollectionAssert.AreEqual(new[]
        {
            new Point(0, 3),
            new Point(0, 5),
            new Point(0, 10)
        }, sumNode.Children.Select(child => child.EndPosition).ToList());
    }

    [TestMethod]
    public void StartIndexAndEndIndex_HandlesCharactersThatOccupyTwoUTF16CodeUnits()
    {
        _tree = _parser.Parse("a👍👎1 /\n b👎c👎")!;
        var quotientNode = _tree.RootNode.FirstChild!.FirstChild!;

        CollectionAssert.AreEqual(new[]
        {
            new Point(0, 0),
            new Point(0, 7),
            new Point(1, 1),
        }, quotientNode.Children.Select(child => child.StartPosition).ToList());
        CollectionAssert.AreEqual(new[]
        {
            new Point(0, 6),
            new Point(0, 8),
            new Point(1, 7),
        }, quotientNode.Children.Select(child => child.EndPosition).ToList());
    }

    [TestMethod]
    public void Parent_ReturnsNodesParent()
    {
        _tree = _parser.Parse("x10 + 1000")!;
        var sumNode = _tree.RootNode.FirstChild!;
        var variableNode = sumNode.FirstChild!;
        Assert.AreNotEqual(sumNode.Id, variableNode.Id);
        Assert.AreEqual(sumNode.Id, variableNode.Parent!.Id);
        Assert.AreEqual(_tree.RootNode.Id, sumNode.Parent!.Id);
    }

    [TestMethod]
    public void ChildFirstChildLastChild_ReturnsNullWhenNodeHasNoChildren()
    {
        _tree = _parser.Parse("x10 + 1000")!;
        var sumNode = _tree.RootNode.FirstChild!.FirstChild!;
        var variableNode = sumNode.FirstChild!;
        Assert.IsNull(variableNode.FirstChild);
        Assert.IsNull(variableNode.LastChild);
        Assert.IsNull(variableNode.FirstNamedChild);
        Assert.IsNull(variableNode.LastNamedChild);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => variableNode.Children[1]);
    }

    [TestMethod]
    public void ChildForFieldName_ReturnsNodeForGivenFieldName()
    {
        _tree = _parser.Parse("class A { b() {} }")!;

        var classNode = _tree.RootNode.FirstChild!;
        Assert.AreEqual("class_declaration", classNode.Type);

        var classNameNode = classNode.GetChildForField("name")!;
        Assert.AreEqual("identifier", classNameNode.Type);
        Assert.AreEqual("A", classNameNode.Text);

        var bodyNode = classNode.GetChildForField("body")!;
        Assert.AreEqual("class_body", bodyNode.Type);
        Assert.AreEqual("{ b() {} }", bodyNode.Text);

        var methodNode = bodyNode.FirstNamedChild!;
        Assert.AreEqual("method_definition", methodNode.Type);
        Assert.AreEqual("b() {}", methodNode.Text);
    }

    [TestMethod]
    public void NextSiblingAndPreviousSibling_ReturnsNodesNextAndPreviousSibling()
    {
        _tree = _parser.Parse("x10 + 1000")!;
        var sumNode = _tree.RootNode.FirstChild!.FirstChild!;
        Assert.AreEqual(sumNode.Children[1].Id, sumNode.Children[0].NextSibling!.Id);
        Assert.AreEqual(sumNode.Children[2].Id, sumNode.Children[1].NextSibling!.Id);
        Assert.AreEqual(sumNode.Children[0].Id, sumNode.Children[1].PreviousSibling!.Id);
        Assert.AreEqual(sumNode.Children[1].Id, sumNode.Children[2].PreviousSibling!.Id);
    }

    [TestMethod]
    public void NextNamedSiblingAndPreviousNamedSibling_ReturnsNodesNextAndPreviousNamedSibling()
    {
        _tree = _parser.Parse("x10 + 1000")!;
        var sumNode = _tree.RootNode.FirstChild!.FirstChild!;
        Assert.AreEqual(sumNode.NamedChildren[1].Id, sumNode.NamedChildren[0].NextNamedSibling!.Id);
        Assert.AreEqual(sumNode.NamedChildren[0].Id, sumNode.NamedChildren[1].PreviousNamedSibling!.Id);
    }

    [TestMethod]
    public void DescendantForIndex_ReturnsSmallestNodeThatSpansGivenRange()
    {
        _tree = _parser.Parse("x10 + 1000")!;
        var sumNode = _tree.RootNode.FirstChild!.FirstChild!;
        Assert.AreEqual("identifier", sumNode.GetDescendantForIndex(1, 2)!.Type);
        Assert.AreEqual("+", sumNode.GetDescendantForIndex(4, 4)!.Type);
    }

    [TestMethod]
    public void NamedDescendantForIndex_ReturnsSmallestNamedNodeThatSpansGivenRange()
    {
        _tree = _parser.Parse("x10 + 1000")!;
        var sumNode = _tree.RootNode.FirstChild!.FirstChild!;
        Assert.AreEqual("identifier", sumNode.GetDescendantForIndex(1, 2)!.Type);
        Assert.AreEqual("+", sumNode.GetDescendantForIndex(4, 4)!.Type);
    }

    [TestMethod]
    public void DescendantForPosition_ReturnsSmallestNodeThatSpansGivenRange()
    {
        _tree = _parser.Parse("x10 + 1000")!;
        var sumNode = _tree.RootNode.FirstChild!;

        Assert.AreEqual("identifier", sumNode.GetDescendantForPosition(new Point(0, 1), new Point(0, 2))!.Type);
        Assert.AreEqual("+", sumNode.GetDescendantForPosition(new Point(0, 4))!.Type);
    }

    [TestMethod]
    public void NamedDescendantForPosition_ReturnsSmallestNamedNodeThatSpansGivenRange()
    {
        _tree = _parser.Parse("x10 + 1000")!;
        var sumNode = _tree.RootNode.FirstChild!;

        Assert.AreEqual("identifier", sumNode.GetNamedDescendantForPosition(new Point(0, 1), new Point(0, 2))!.Type);
        Assert.AreEqual("binary_expression", sumNode.GetNamedDescendantForPosition(new Point(0, 4))!.Type);
    }

    [TestMethod]
    public void HasError_ReturnsTrueIfNodeContainsError()
    {
        _tree = _parser.Parse("1 + 2 * * 3")!;
        var node = _tree.RootNode;
        Assert.AreEqual("(program (expression_statement (binary_expression left: (number) right: (binary_expression left: (number) (ERROR) right: (number)))))", node.Expression);

        var sum = node.FirstChild!.FirstChild!;
        Assert.IsTrue(sum.HasError);
        Assert.IsFalse(sum.Children[0].HasError);
        Assert.IsFalse(sum.Children[1].HasError);
        Assert.IsTrue(sum.Children[2].HasError);
    }

    [TestMethod]
    public void IsError_ReturnsTrueIfNodeIsError()
    {
        _tree = _parser.Parse("2 * * 3")!;
        var node = _tree.RootNode;
        Assert.AreEqual("(program (expression_statement (binary_expression left: (number) (ERROR) right: (number))))", node.Expression);

        var multi = node.FirstChild!.FirstChild!;
        Assert.IsTrue(multi.HasError);
        Assert.IsFalse(multi.Children[0].IsError);
        Assert.IsFalse(multi.Children[1].IsError);
        Assert.IsTrue(multi.Children[2].IsError);
        Assert.IsFalse(multi.Children[3].IsError);
    }

    [TestMethod]
    public void IsMissing_ReturnsTrueIfNodeWasInsertedViaErrorRecovery()
    {
        _tree = _parser.Parse("(2 ||)")!;
        var node = _tree.RootNode;
        Assert.AreEqual("(program (expression_statement (parenthesized_expression (binary_expression left: (number) right: (MISSING identifier)))))", node.Expression);

        var sum = node.FirstChild!.FirstChild!.FirstNamedChild!;
        Assert.AreEqual("binary_expression", sum.Type);
        Assert.IsTrue(sum.HasError);
        Assert.IsFalse(sum.Children[0].IsMissing);
        Assert.IsFalse(sum.Children[1].IsMissing);
        Assert.IsTrue(sum.Children[2].IsMissing);
    }

    [TestMethod]
    public void IsExtra_ReturnsTrueIfNodeIsExtraNodeLikeComments()
    {
        _tree = _parser.Parse("foo(/* hi */);")!;
        var node = _tree.RootNode;
        var commentNode = node.GetDescendantForIndex(7, 7)!;

        Assert.AreEqual("program", node.Type);
        Assert.AreEqual("comment", commentNode.Type);
        Assert.IsFalse(node.IsExtra);
        Assert.IsTrue(commentNode.IsExtra);
    }

    [TestMethod]
    public void Text_ReturnsTextOfNode()
    {
        const string text = "α0 / b👎c👎";
        _tree = _parser.Parse(text)!;
        var quotientNode = _tree.RootNode.FirstChild!.FirstChild!;
        var numerator = quotientNode.Children[0];
        var slash = quotientNode.Children[1];
        var denominator = quotientNode.Children[2];

        Assert.AreEqual(text, _tree.RootNode.Text);
        Assert.AreEqual("b👎c👎", denominator.Text);
        Assert.AreEqual(text, quotientNode.Text);
        Assert.AreEqual("α0", numerator.Text);
        Assert.AreEqual("/", slash.Text);
    }

    [TestMethod]
    public void DescendantCount_ReturnsNumberOfDescendants()
    {
        _parser.Language = _json;
        _tree = _parser.Parse(JSON_EXAMPLE)!;
        var valueNode = _tree.RootNode;
        var allNodes = GetAllNodes(_tree);

        Assert.AreEqual(allNodes.Count, valueNode.DescendantCount);

        var cursor = _tree.Walk();
        for (int i = 0; i < allNodes.Count; i++)
        {
            var node = allNodes[i];
            cursor.GotoDescendant(i);
            Assert.AreEqual(node.Id, cursor.CurrentNode.Id);
        }

        for (int i = allNodes.Count - 1; i >= 0; i--)
        {
            var node = allNodes[i];
            cursor.GotoDescendant(i);
            Assert.AreEqual(node.Id, cursor.CurrentNode.Id);
        }
    }

    [TestMethod]
    public void SingleNodeTree_Test()
    {
#if false
        parser.Language = EmbeddedTemplate;
        tree = parser.Parse("hello")!;

        var nodes = GetAllNodes(tree);
        Assert.AreEqual(2, nodes.Count);
        Assert.AreEqual(2, tree.RootNode.DescendantCount);

        var cursor = tree.Walk();

        cursor.GotoDescendant(0);
        Assert.AreEqual(0, cursor.CurrentDepth);
        Assert.AreEqual(nodes[0].Id, cursor.CurrentNode.Id);

        cursor.GotoDescendant(1);
        Assert.AreEqual(1, cursor.CurrentDepth);
        Assert.AreEqual(nodes[1].Id, cursor.CurrentNode.Id);
#endif
    }

    [TestMethod]
    public void RootNodeWithOffset_ReturnsRootNodeWithOffset()
    {
        _tree = _parser.Parse("  if (a) b")!;
        var node = _tree.GetRootNodeWithIndex(6, new Point(2, 2));
        Assert.AreEqual(8, node.StartIndex);
        Assert.AreEqual(16, node.EndIndex);
        Assert.AreEqual(new Point(2, 4), node.StartPosition);
        Assert.AreEqual(new Point(2, 12), node.EndPosition);

        var child = node.FirstChild!.Children[2];
        Assert.AreEqual("expression_statement", child.Type);
        Assert.AreEqual(15, child.StartIndex);
        Assert.AreEqual(16, child.EndIndex);
        Assert.AreEqual(new Point(2, 11), child.StartPosition);
        Assert.AreEqual(new Point(2, 12), child.EndPosition);

        var cursor = node.Walk();
        cursor.GotoFirstChild();
        cursor.GotoFirstChild();
        cursor.GotoNextSibling();
        child = cursor.CurrentNode;
        Assert.AreEqual("parenthesized_expression", child.Type);
        Assert.AreEqual(11, child.StartIndex);
        Assert.AreEqual(14, child.EndIndex);
        Assert.AreEqual(new Point(2, 7), child.StartPosition);
        Assert.AreEqual(new Point(2, 10), child.EndPosition);
    }

    [TestMethod]
    public void ParseStateAndNextParseState_ReturnsNodeParseStateIds()
    {
        const string text = "10 / 5";
        _tree = _parser.Parse(text)!;
        var quotientNode = _tree.RootNode.FirstChild!.FirstChild!;
        var numerator = quotientNode.Children[0];
        var slash = quotientNode.Children[1];
        var denominator = quotientNode.Children[2];

        Assert.AreEqual(0, _tree.RootNode.ParseState);
        Assert.IsTrue(numerator.ParseState > 0);
        Assert.IsTrue(slash.ParseState > 0);
        Assert.IsTrue(denominator.ParseState > 0);
    }

    [TestMethod]
    public void ParseStateAndNextParseState_ReturnsNextParseStateEqualsLanguage()
    {
        const string text = "10 / 5";
        _tree = _parser.Parse(text)!;
        var quotientNode = _tree.RootNode.FirstChild!.FirstChild!;
        foreach (var node in quotientNode.Children)
        {
            Assert.AreEqual(_javaScript.GetNextState(node.ParseState, node.GrammarId), node.NextParseState);
        }
    }

    [TestMethod]
    public void DescendantsOfType_FindsAllDescendantsOfGivenTypeInRange()
    {
#if false
        tree = parser.Parse("a + 1 * b * 2 + c + 3")!;
        var outerSum = tree.RootNode.FirstChild!.FirstChild!;

        var descendants = outerSum.GetDescendantsOfType("number", new Point(0, 2), new Point(0, 15));
        CollectionAssert.AreEqual(new[] { 4, 12 }, descendants.Select(node => node.StartIndex).ToList);
        CollectionAssert.AreEqual(new[] { new Point(0, 5), new Point(0, 13) }, descendants.ConvertAll(node => node.EndPosition));
#endif
    }

    [TestMethod]
    public void FirstChildForIndex_ReturnsFirstChildThatContainsOrStartsAfterGivenIndex()
    {
        _tree = _parser.Parse("x10 + 1000")!;
        var sumNode = _tree.RootNode.FirstChild!.FirstChild!;

        Assert.AreEqual("identifier", sumNode.GetFirstChildForIndex(0)!.Type);
        Assert.AreEqual("identifier", sumNode.GetFirstChildForIndex(1)!.Type);
        Assert.AreEqual("+", sumNode.GetFirstChildForIndex(3)!.Type);
        Assert.AreEqual("number", sumNode.GetFirstChildForIndex(5)!.Type);
    }

    [TestMethod]
    public void FirstNamedChildForIndex_ReturnsFirstChildThatContainsOrStartsAfterGivenIndex()
    {
        _tree = _parser.Parse("x10 + 1000")!;
        var sumNode = _tree.RootNode.FirstChild!.FirstChild!;

        Assert.AreEqual("identifier", sumNode.GetFirstNamedChildForIndex(0)!.Type);
        Assert.AreEqual("identifier", sumNode.GetFirstNamedChildForIndex(1)!.Type);
        Assert.AreEqual("number", sumNode.GetFirstNamedChildForIndex(3)!.Type);
    }

    [TestMethod]
    public void Equals_ReturnsTrueIfNodesAreSame()
    {
        _tree = _parser.Parse("1 + 2")!;

        var sumNode = _tree.RootNode.FirstChild!.FirstChild!;
        var node1 = sumNode.FirstChild!;
        var node2 = sumNode.FirstChild!;
        Assert.IsTrue(node1.Equals(node2));
        Assert.IsTrue(node1 == node2);
    }

    [TestMethod]
    public void Equals_ReturnsFalseIfNodesAreNotSame()
    {
        _tree = _parser.Parse("1 + 2")!;

        var sumNode = _tree.RootNode.FirstChild!.FirstChild!;
        var node1 = sumNode.FirstChild!;
        var node2 = node1.NextSibling!;
        Assert.IsFalse(node1.Equals(node2));
        Assert.IsFalse(node1 == node2);
    }

    [TestMethod]
    public void FieldNameForChild_ReturnsFieldOfChildOrNull()
    {
        _parser.Language = _c;
        _tree = _parser.Parse("int w = x + /* y is special! */ y;")!;

        var translationUnitNode = _tree.RootNode;
        var declarationNode = translationUnitNode.FirstChild!;
        var binaryExpressionNode = declarationNode["declarator"]["value"];

        // -------------------
        // left: (identifier)  0
        // operator: "+"       1 <--- (not a named child)
        // (comment)           2 <--- (is an extra)
        // right: (identifier) 3
        // -------------------

        Assert.AreEqual("left", binaryExpressionNode.GetFieldNameForChild(0));
        Assert.AreEqual("operator", binaryExpressionNode.GetFieldNameForChild(1));
        // The comment should not have a field name, as it's just an extra
        Assert.IsNull(binaryExpressionNode.GetFieldNameForChild(2));
        Assert.AreEqual("right", binaryExpressionNode.GetFieldNameForChild(3));
        // Negative test - Not a valid child index
        Assert.IsNull(binaryExpressionNode.GetFieldNameForChild(4));
    }

    [TestMethod]
    public void FieldNameForNamedChild_ReturnsFieldOfNamedChildOrNull()
    {
        _parser.Language = _c;
        _tree = _parser.Parse("int w = x + /* y is special! */ y;")!;

        var translationUnitNode = _tree.RootNode;
        var declarationNode = translationUnitNode.FirstNamedChild;
        var binaryExpressionNode = declarationNode!
            .GetChildForField("declarator")!
            .GetChildForField("value")!;

        // -------------------
        // left: (identifier)  0
        // operator: "+"       _ <--- (not a named child)
        // (comment)           1 <--- (is an extra)
        // right: (identifier) 2
        // -------------------

        Assert.AreEqual("left", binaryExpressionNode.GetFieldNameForNamedChild(0));
        // The comment should not have a field name, as it's just an extra
        Assert.IsNull(binaryExpressionNode.GetFieldNameForNamedChild(1));
        // The operator is not a named child, so the named child at index 2 is the right child
        Assert.AreEqual("right", binaryExpressionNode.GetFieldNameForNamedChild(2));
        // Negative test - Not a valid child index
        Assert.IsNull(binaryExpressionNode.GetFieldNameForNamedChild(3));
    }

    static List<Node> GetAllNodes(Tree tree)
    {
        List<Node> result = [];

        bool visitedChildren = false;
        var cursor = tree.Walk();
        while (true)
        {
            if (!visitedChildren)
            {
                result.Add(cursor.CurrentNode);
                if (!cursor.GotoFirstChild())
                {
                    visitedChildren = true;
                }
            }
            else if (cursor.GotoNextSibling())
            {
                visitedChildren = false;
            }
            else if (!cursor.GotoParent())
            {
                break;
            }
        }

        return result;
    }
}
