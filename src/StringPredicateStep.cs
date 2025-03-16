//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using System.Diagnostics;

namespace TreeSitter;

/// <summary>
/// Represents a string predicate step in a query.
/// </summary>
[DebuggerDisplay("{Value}")]
public class StringPredicateStep : PredicateStep
{
    /// <summary>
    /// Creates a new instance of <see cref="StringPredicateStep"/>.
    /// </summary>
    /// <param name="value">The string value.</param>
    public StringPredicateStep(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the type of the predicate step.
    /// </summary>
    public override string Type => "string";

    /// <summary>
    /// Gets the string value.
    /// </summary>
    public string Value { get; }
}
