//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

/// <summary>
/// Represents a quantifier used in queries.
/// </summary>
internal enum Quantifier
{
    /// <summary>
    /// Matches zero occurrences.
    /// </summary>
    Zero,

    /// <summary>
    /// Matches zero or one occurrence.
    /// </summary>
    ZeroOrOne,

    /// <summary>
    /// Matches zero or more occurrences.
    /// </summary>
    ZeroOrMore,

    /// <summary>
    /// Matches exactly one occurrence.
    /// </summary>
    One,

    /// <summary>
    /// Matches one or more occurrences.
    /// </summary>
    OneOrMore,
}
