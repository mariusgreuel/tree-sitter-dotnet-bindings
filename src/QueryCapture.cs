//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using System.Diagnostics;

using static TreeSitter.Native;

namespace TreeSitter;

/// <summary>
/// Represents a query capture.
/// </summary>
[DebuggerDisplay("{patternIndex}: {name} => {node.Text}")]
public class QueryCapture
{
    internal QueryCapture(TSQueryCapture queryCapture, int patternIndex, Query query, Tree tree)
    {
        PatternIndex = patternIndex;
        Name = query._captureNames[(int)queryCapture.index];
        Node = new(queryCapture.node, tree);
    }

    /// <summary>
    /// Gets the pattern index.
    /// </summary>
    public int PatternIndex { get; }

    /// <summary>
    /// Gets the capture name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the node that was captured.
    /// </summary>
    public Node Node { get; }

    /// <summary>
    /// Gets the user-defined predicates.
    /// </summary>
    /// <remarks>
    ///  This user-defined predicates include predicates with operators other than:
    ///  - 'match?'
    ///  - 'eq?' and 'not-eq?'
    ///  - 'any-of?' and 'not-any-of?'
    ///  - 'is?' and 'is-not?'
    ///  - 'set!'
    /// </remarks>
    public IReadOnlyList<UserPredicate> UserPredicates => _userPredicates;

    /// <summary>
    /// Get the properties for predicates declared with the operator 'is?'.
    /// </summary>
    public IReadOnlyDictionary<string, string?>? AssertedProperties => _assertedProperties;

    /// <summary>
    /// Get the properties for predicates declared with the operator 'is-not?'.
    /// </summary>
    public IReadOnlyDictionary<string, string?>? RefutedProperties => _refutedProperties;

    /// <summary>
    /// Get the properties for predicates declared with the operator 'set!'.
    /// </summary>
    public IReadOnlyDictionary<string, string?>? SetProperties => _setProperties;

    internal List<UserPredicate> _userPredicates = new();
    internal IReadOnlyDictionary<string, string?>? _assertedProperties;
    internal IReadOnlyDictionary<string, string?>? _refutedProperties;
    internal IReadOnlyDictionary<string, string?>? _setProperties;
}
