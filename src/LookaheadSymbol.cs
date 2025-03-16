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
