//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

/// <summary>
/// Specifies the type of a log message.
/// </summary>
public enum LogType
{
    /// <summary>
    /// A parser message.
    /// </summary>
    Parse,

    /// <summary>
    /// A lexer message.
    /// </summary>
    Lex,
}
