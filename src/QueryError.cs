//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

/// <summary>
/// Represents the possible errors that can occur when working with queries.
/// </summary>
internal enum QueryError
{
    /// <summary>
    /// No error occurred.
    /// </summary>
    None,

    /// <summary>
    /// A syntax error occurred in the query.
    /// </summary>
    Syntax,

    /// <summary>
    /// An invalid node type was specified in the query.
    /// </summary>
    NodeType,

    /// <summary>
    /// An invalid field name was specified in the query.
    /// </summary>
    Field,

    /// <summary>
    /// An invalid capture name was specified in the query.
    /// </summary>
    Capture,

    /// <summary>
    /// The query has an invalid structure.
    /// </summary>
    Structure,

    /// <summary>
    /// The query is not compatible with the language.
    /// </summary>
    Language,
}
