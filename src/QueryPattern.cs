//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using System.Text.RegularExpressions;

using static TreeSitter.Native;

namespace TreeSitter;

/// <summary>
/// Represents a pattern in a query.
/// </summary>
public class QueryPattern
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryPattern"/> class.
    /// </summary>
    /// <param name="query">The query associated with the pattern.</param>
    /// <param name="index">The index of the pattern in the query.</param>
    internal QueryPattern(Query query, uint index)
    {
        _query = query;
        _index = index;
        ParsePredicateSteps();
    }

    /// <summary>
    /// Gets the pattern index in the query.
    /// </summary>
    public int Index => (int)_index;

    /// <summary>
    /// Gets the offset where the given pattern starts in the query's source.
    /// </summary>
    public int StartIndex => (int)ts_query_start_byte_for_pattern(_query.Self, _index);

    /// <summary>
    /// Gets the offset where the given pattern ends in the query's source.
    /// </summary>
    public int EndIndex => (int)ts_query_end_byte_for_pattern(_query.Self, _index);

    /// <summary>
    /// Check if the given pattern in the query has a single root node.
    /// </summary>
    public bool IsRooted => ts_query_is_pattern_rooted(_query.Self, _index);

    /// <summary>
    /// Check if the given pattern in the query is 'non local'.
    /// </summary>
    public bool IsNonLocal => ts_query_is_pattern_non_local(_query.Self, _index);

    /// <summary>
    /// Disable the pattern within a query.
    /// </summary>
    /// <remarks>
    /// This prevents the pattern from matching and removes most of the overhead
    /// associated with the pattern. Currently, there is no way to undo this.
    /// </remarks>
    public void Disable()
    {
        ts_query_disable_pattern(_query.Self, _index);
    }

    internal bool MatchesPredicates(IEnumerable<QueryCapture> captures)
    {
        return _textPredicates.TrueForAll(predicate => predicate.Predicate(captures));
    }

    void ParsePredicateSteps()
    {
        List<PredicateStep> steps = [];

        var predicateSteps = ts_query_predicates_for_pattern(_query.Self, _index, out var count);
        for (int i = 0; i < (int)count; i++)
        {
            var predicateStep = MarshalArrayItem<TSQueryPredicateStep>(predicateSteps, i);
            switch (predicateStep.type)
            {
                case QueryPredicateStepType.String:
                    steps.Add(new StringPredicateStep(_query._strings[(int)predicateStep.value_id]));
                    break;
                case QueryPredicateStepType.Capture:
                    steps.Add(new CapturePredicateStep(_query._captureNames[(int)predicateStep.value_id]));
                    break;
                case QueryPredicateStepType.Done:
                    if (steps.Count > 0)
                    {
                        ParsePredicate(steps);
                        steps.Clear();
                    }
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected query predicate step type: {predicateStep.type}");
            }
        }
    }

    void ParsePredicate(List<PredicateStep> steps)
    {
        if (steps[0] is not StringPredicateStep step)
        {
            throw new InvalidOperationException("Predicates must begin with a string literal value.");
        }

        switch (step.Value)
        {
            case "any-not-eq?":
            case "not-eq?":
            case "any-eq?":
            case "eq?":
                ParseAnyPredicate(steps, step.Value);
                break;
            case "any-not-match?":
            case "not-match?":
            case "any-match?":
            case "match?":
                ParseMatchPredicate(steps, step.Value);
                break;
            case "not-any-of?":
            case "any-of?":
                ParseAnyOfPredicate(steps, step.Value);
                break;
            case "is?":
            case "is-not?":
                ParseIsPredicate(steps, step.Value);
                break;
            case "set!":
                ParseSetDirective(steps, step.Value);
                break;
            default:
                _userPredicates.Add(new(steps));
                break;
        }
    }

    void ParseAnyPredicate(List<PredicateStep> steps, string type)
    {
        if (steps.Count != 3)
        {
            throw new InvalidOperationException($"Wrong number of arguments to '#{type}' predicate. Expected 2, got {steps.Count - 1}");
        }

        if (steps[1] is not CapturePredicateStep capture1)
        {
            throw new InvalidOperationException($"First argument of '#{type}' predicate must be a capture, got '{steps[1].Type}'");
        }

        var isPositive = type == "eq?" || type == "any-eq?";
        var matchAll = !type.StartsWith("any-");
        bool Equals(string text1, string text2) => string.Equals(text1, text2) == isPositive;

        if (steps[2] is CapturePredicateStep capture2)
        {
            _textPredicates.Add(new(captures =>
            {
                var nodes1 = FindCaptureNodesByName(captures, capture1.Name);
                var nodes2 = FindCaptureNodesByName(captures, capture2.Name);

                if (matchAll)
                {
                    return nodes1.TrueForAll((node1) => nodes2.TrueForSome((node2) => Equals(node1.Text, node2.Text)));
                }
                else
                {
                    return nodes1.TrueForSome((node1) => nodes2.TrueForSome((node2) => Equals(node1.Text, node2.Text)));
                }
            }));
        }
        else if (steps[2] is StringPredicateStep string2)
        {
            _textPredicates.Add(new(captures =>
            {
                var nodes = FindCaptureNodesByName(captures, capture1.Name);

                if (matchAll)
                {
                    return nodes.TrueForAll(node => Equals(node.Text, string2.Value));
                }
                else
                {
                    return nodes.TrueForSome(node => Equals(node.Text, string2.Value));
                }
            }));
        }
        else
        {
            throw new InvalidOperationException("Unexpected step type.");
        }
    }

    void ParseMatchPredicate(List<PredicateStep> steps, string type)
    {
        if (steps.Count != 3)
        {
            throw new InvalidOperationException($"Wrong number of arguments to '#{type}' predicate. Expected 2, got {steps.Count - 1}");
        }

        if (steps[1] is not CapturePredicateStep capture1)
        {
            throw new InvalidOperationException($"First argument of '#{type}' predicate must be a capture, got '{steps[1].Type}'");
        }

        if (steps[2] is not StringPredicateStep string2)
        {
            throw new InvalidOperationException($"Second argument of '#{type}' predicate must be a string, got '{steps[2].Type}'");
        }

        var isPositive = type == "match?" || type == "any-match?";
        var matchAll = !type.StartsWith("any-");
        var regex = new Regex(string2.Value);
        bool IsMatch(string text) => isPositive ? regex.IsMatch(text) : !regex.IsMatch(text);

        _textPredicates.Add(new(captures =>
        {
            var nodes = FindCaptureNodesByName(captures, capture1.Name);

            if (matchAll)
            {
                return nodes.TrueForAll(node => IsMatch(node.Text));
            }
            else
            {
                return nodes.TrueForSome(node => IsMatch(node.Text));
            }
        }));
    }

    void ParseAnyOfPredicate(List<PredicateStep> steps, string type)
    {
        if (steps.Count < 2)
        {
            throw new InvalidOperationException($"Wrong number of arguments to '#{type}' predicate. Expected at least 1, got {steps.Count - 1}");
        }

        if (steps[1] is not CapturePredicateStep capture1)
        {
            throw new InvalidOperationException($"First argument of '#{type}' predicate must be a capture, got '{steps[1].Type}'");
        }

        var stringSteps = steps.GetRange(2, steps.Count - 2);
        if (!stringSteps.TrueForAll(step => step is StringPredicateStep))
        {
            throw new InvalidOperationException($"Arguments to '#{type}' predicate must be strings.");
        }

        var isPositive = type == "any-of?";

        _textPredicates.Add(new(captures =>
        {
            var nodes = FindCaptureNodesByName(captures, capture1.Name);
            if (nodes.Count == 0)
            {
                return !isPositive;
            }

            return nodes.TrueForAll(node => stringSteps.TrueForSome(step => ((StringPredicateStep)step).Value == node.Text) == isPositive);
        }));
    }

    void ParseIsPredicate(List<PredicateStep> steps, string type)
    {
        if (steps.Count < 2 || steps.Count > 3)
        {
            throw new InvalidOperationException($"Wrong number of arguments to '#{type}' predicate. Expected 1 or 2, got {steps.Count - 1}");
        }

        if (steps[1] is not StringPredicateStep string1)
        {
            throw new InvalidOperationException($"First argument of '#{type}' predicate must be a string, got '{steps[1].Type}'");
        }

        string? value2 = null;
        if (steps.Count > 2)
        {
            if (steps[2] is not StringPredicateStep string2)
            {
                throw new InvalidOperationException($"Second argument of '#{type}' predicate must be a string or empty, got '{steps[2].Type}'");
            }

            value2 = string2.Value;
        }

        var properties = type == "is?" ? _assertedProperties : _refutedProperties;

        properties[string1.Value] = value2;
    }

    void ParseSetDirective(List<PredicateStep> steps, string type)
    {
        if (steps.Count < 2 || steps.Count > 3)
        {
            throw new InvalidOperationException($"Wrong number of arguments to '#{type}' predicate. Expected 1 or 2, got {steps.Count - 1}");
        }

        if (steps[1] is not StringPredicateStep string1)
        {
            throw new InvalidOperationException($"First argument of '#{type}' predicate must be a string, got '{steps[1].Type}'");
        }

        string? value2 = null;
        if (steps.Count > 2)
        {
            if (steps[2] is not StringPredicateStep string2)
            {
                throw new InvalidOperationException($"Second argument of '#{type}' predicate must be a string or empty, got '{steps[2].Type}'");
            }

            value2 = string2.Value;
        }

        _setProperties[string1.Value] = value2;
    }

    static List<Node> FindCaptureNodesByName(IEnumerable<QueryCapture> captures, string name)
    {
        List<Node> nodes = [];

        foreach (var capture in captures)
        {
            if (capture.Name == name)
            {
                nodes.Add(capture.Node);
            }
        }

        return nodes;
    }

    readonly Query _query;
    readonly uint _index;
    internal readonly List<TextPredicate> _textPredicates = [];
    internal readonly List<UserPredicate> _userPredicates = [];
    internal readonly Dictionary<string, string?> _assertedProperties = [];
    internal readonly Dictionary<string, string?> _refutedProperties = [];
    internal readonly Dictionary<string, string?> _setProperties = [];
}
