//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using System.Diagnostics;

using static TreeSitter.Native;

namespace TreeSitter;

/// <summary>
/// Represents a range of positions in a multi-line text document,
/// both in terms of bytes and of rows and columns.
/// </summary>
[DebuggerDisplay("{StartPosition}-{EndPosition} ({StartIndex}-{EndIndex})")]
public struct Range
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Range"/> struct.
    /// </summary>
    public Range()
    {
        _self.start_point = new() { row = uint.MinValue, column = uint.MinValue };
        _self.end_point = new() { row = uint.MaxValue, column = uint.MaxValue };
        _self.start_byte = uint.MinValue;
        _self.end_byte = uint.MaxValue;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Range"/> struct.
    /// </summary>
    /// <param name="startPosition">The start position of the range.</param>
    /// <param name="endPosition">The end position of the range.</param>
    /// <param name="startIndex">The start index of the range.</param>
    /// <param name="endIndex">The end index of the range.</param>
    public Range(Point startPosition, Point endPosition, int startIndex, int endIndex)
    {
        _self.start_point = startPosition._self;
        _self.end_point = endPosition._self;
        _self.start_byte = IndexToByte(startIndex);
        _self.end_byte = IndexToByte(endIndex);
    }

    internal Range(TSRange self)
    {
        _self = self;
    }

    /// <summary>
    /// The start position of the range.
    /// </summary>
    public Point StartPosition
    {
        readonly get => new(_self.start_point);
        set => _self.start_point = value._self;
    }

    /// <summary>
    /// The end position of the range.
    /// </summary>
    public Point EndPosition
    {
        readonly get => new(_self.end_point);
        set => _self.end_point = value._self;
    }

    /// <summary>
    /// The start index of the range.
    /// </summary>
    public int StartIndex
    {
        readonly get => ByteToIndex(_self.start_byte);
        set => _self.start_byte = IndexToByte(value);
    }

    /// <summary>
    /// The end index of the range.
    /// </summary>
    public int EndIndex
    {
        readonly get => ByteToIndex(_self.end_byte);
        set => _self.end_byte = IndexToByte(value);
    }

    internal TSRange _self;
}
