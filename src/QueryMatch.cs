//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using static TreeSitter.Native;

namespace TreeSitter;

/// <summary>
/// Represents a query match.
/// </summary>
public class QueryMatch
{
    internal QueryMatch(TSQueryMatch queryMatch, Query query, Tree tree)
    {
        Id = queryMatch.id;
        PatternIndex = queryMatch.pattern_index;

        _captures = new(queryMatch.capture_count);
        for (int i = 0; i < queryMatch.capture_count; i++)
        {
            var queryCapture = MarshalArrayItem<TSQueryCapture>(queryMatch.captures, i);
            _captures.Add(new(queryCapture, PatternIndex, query, tree));
        }
    }

    /// <summary>
    /// Gets the match ID.
    /// </summary>
    public uint Id { get; }

    /// <summary>
    /// Gets the pattern index of the match.
    /// </summary>
    public int PatternIndex { get; }

    /// <summary>
    /// Gets the captures of the query.
    /// </summary>
    public IReadOnlyList<QueryCapture> Captures => _captures;

    /// <summary>
    /// Gets the user-defined predicates.
    /// </summary>
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

    readonly List<QueryCapture> _captures;
    internal List<UserPredicate> _userPredicates = new();
    internal Dictionary<string, string?>? _assertedProperties;
    internal Dictionary<string, string?>? _refutedProperties;
    internal Dictionary<string, string?>? _setProperties;
}
