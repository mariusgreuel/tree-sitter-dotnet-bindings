//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

/// <summary>
/// Represents a user-defined predicate in a query.
/// </summary>
public class UserPredicate
{
    /// <summary>
    /// Creates a new instance of <see cref="UserPredicate"/>.
    /// </summary>
    /// <param name="steps">The list of predicate steps.</param>
    public UserPredicate(List<PredicateStep> steps)
    {
        _steps = steps;
    }

    /// <summary>
    /// Gets the list of predicate steps.
    /// </summary>
    public IReadOnlyList<PredicateStep> Steps => _steps;

    readonly List<PredicateStep> _steps;
}
