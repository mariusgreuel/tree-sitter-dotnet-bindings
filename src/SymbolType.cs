//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

/// <summary>
/// Specifies the type of a symbol.
/// </summary>
public enum SymbolType
{
    /// <summary>
    /// A regular symbol.
    /// </summary>
    Regular,

    /// <summary>
    /// A anonymous symbol.
    /// </summary>
    Anonymous,

    /// <summary>
    /// A supertype symbol.
    /// </summary>
    Supertype,

    /// <summary>
    /// A auxiliary symbol.
    /// </summary>
    Auxiliary,
}
