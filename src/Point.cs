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
/// Represents a position in a multi-line text document, in terms of rows and columns.
/// </summary>
[DebuggerDisplay("({Row}, {Column})")]
public struct Point : IEquatable<Point>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Point"/> struct.
    /// </summary>
    public Point()
    {
        _self.row = 0;
        _self.column = 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Point"/> struct.
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    public Point(int row, int column)
    {
        _self.row = IntToUInt(row);
        _self.column = IndexToByte(column);
    }

    internal Point(TSPoint self)
    {
        _self = self;
    }

    /// <summary>
    /// The zero-based row number.
    /// </summary>
    public int Row
    {
        readonly get => UIntToInt(_self.row);
        set => _self.row = IntToUInt(value);
    }

    /// <summary>
    /// The zero-based column number.
    /// </summary>
    public int Column
    {
        readonly get => ByteToIndex(_self.column);
        set => _self.column = IndexToByte(value);
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? other)
    {
        return other is Point point && Equals(point);
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        return _self.row.GetHashCode() ^ _self.column.GetHashCode();
    }

    /// <inheritdoc/>
    public readonly bool Equals(Point other)
    {
        return _self.row == other._self.row && _self.column == other._self.column;
    }

    /// <summary>
    /// Compares two <see cref="Point"/> objects for equality.
    /// </summary>
    /// <param name="left">The first point.</param>
    /// <param name="right">The second point.</param>
    /// <returns><see langword="true"/> if the points are equal; otherwise <see langword="false"/>.</returns>
    public static bool operator ==(Point left, Point right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Compares two <see cref="Point"/> objects for inequality.
    /// </summary>
    /// <param name="left">The first point.</param>
    /// <param name="right">The second point.</param>
    /// <returns><see langword="true"/> if the points are not equal; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(Point left, Point right)
    {
        return !(left == right);
    }

    internal TSPoint _self;
}
