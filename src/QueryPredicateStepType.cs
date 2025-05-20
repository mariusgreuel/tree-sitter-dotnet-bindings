//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

/// <summary>
/// Represents the types of steps in a query predicate.
/// </summary>
internal enum QueryPredicateStepType
{
    /// <summary>
    /// Indicates the end of a predicate.
    /// </summary>
    Done,

    /// <summary>
    /// Represents a capture step in a predicate.
    /// </summary>
    Capture,

    /// <summary>
    /// Represents a string step in a predicate.
    /// </summary>
    String,
}
