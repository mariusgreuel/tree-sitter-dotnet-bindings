//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

/// <summary>
/// Represents a step in a query predicate.
/// </summary>
public abstract class PredicateStep
{
    /// <summary>
    /// Gets the type of the predicate step.
    /// </summary>
    public abstract string Type { get; }
}
