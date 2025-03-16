//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using System.Diagnostics;

namespace TreeSitter;

/// <summary>
/// Represents a capture predicate step in a query.
/// </summary>
[DebuggerDisplay("@{Name,nq}")]
public class CapturePredicateStep : PredicateStep
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CapturePredicateStep"/> class.
    /// </summary>
    /// <param name="name">The capture name.</param>
    public CapturePredicateStep(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets the type of the predicate step.
    /// </summary>
    public override string Type => "capture";

    /// <summary>
    /// Gets the capture name.
    /// </summary>
    public string Name { get; }
}
