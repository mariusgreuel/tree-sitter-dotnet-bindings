//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

/// <summary>
/// Represents a text predicate used in a query.
/// </summary>
internal class TextPredicate
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextPredicate"/> class.
    /// </summary>
    /// <param name="predicate">The predicate function to evaluate.</param>
    internal TextPredicate(Predicate<IEnumerable<QueryCapture>> predicate)
    {
        Predicate = predicate;
    }

    /// <summary>
    /// Gets the predicate function.
    /// </summary>
    internal Predicate<IEnumerable<QueryCapture>> Predicate { get; }
}
