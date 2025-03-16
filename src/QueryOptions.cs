//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

/// <summary>
/// Represents the options for executing a query.
/// </summary>
public class QueryOptions
{
    /// <summary>
    /// Gets or sets the maximum number of matches to be returned.
    /// </summary>
    public uint? MatchLimit { get; set; }

    /// <summary>
    /// Gets or sets the start index of the query.
    /// </summary>
    public int? StartIndex { get; set; }

    /// <summary>
    /// Gets or sets the end index of the query.
    /// </summary>
    public int? EndIndex { get; set; }

    /// <summary>
    /// Gets or sets the start point of the query.
    /// </summary>
    public Point? StartPoint { get; set; }

    /// <summary>
    /// Gets or sets the end point of the query.
    /// </summary>
    public Point? EndPoint { get; set; }
}
