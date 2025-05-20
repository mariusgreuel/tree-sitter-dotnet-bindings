//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

/// <summary>
/// Represents the input encoding types supported by Tree-Sitter.
/// </summary>
internal enum InputEncoding
{
    /// <summary>
    /// UTF-8 encoding.
    /// </summary>
    UTF8,

    /// <summary>
    /// UTF-16 Little Endian encoding.
    /// </summary>
    UTF16LE,

    /// <summary>
    /// UTF-16 Big Endian encoding.
    /// </summary>
    UTF16BE,

    /// <summary>
    /// Custom encoding.
    /// </summary>
    Custom,
}
