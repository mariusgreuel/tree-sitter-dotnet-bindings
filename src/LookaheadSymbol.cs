//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using System.Diagnostics;

namespace TreeSitter;

/// <summary>
/// Represents a lookahead iterator symbol.
/// </summary>
[DebuggerDisplay("{Name,nq}")]
public class LookaheadSymbol
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LookaheadSymbol"/> class.
    /// </summary>
    /// <param name="id">The symbol's identifier.</param>
    /// <param name="name">The symbol's name.</param>
    internal LookaheadSymbol(ushort id, string name)
    {
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Gets the symbol's identifier.
    /// </summary>
    public ushort Id { get; }

    /// <summary>
    /// Gets the symbol's name.
    /// </summary>
    public string Name { get; }
}
