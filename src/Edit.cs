//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using static TreeSitter.Native;

namespace TreeSitter;

/// <summary>
/// Represents a summary of a change to a text document.
/// </summary>
public class Edit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Edit"/> class.
    /// </summary>
    public Edit()
    {
        _self = new();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Edit"/> class with a native <see cref="TSInputEdit"/> structure.
    /// </summary>
    /// <param name="self">The native <see cref="TSInputEdit"/> structure.</param>
    internal Edit(TSInputEdit self)
    {
        _self = self;
    }

    /// <summary>
    /// The start index of the change.
    /// </summary>
    public int StartIndex
    {
        get => ByteToIndex(_self.start_byte);
        set => _self.start_byte = IndexToByte(value);
    }

    /// <summary>
    /// The end index of the change before the edit.
    /// </summary>
    public int OldEndIndex
    {
        get => ByteToIndex(_self.old_end_byte);
        set => _self.old_end_byte = IndexToByte(value);
    }

    /// <summary>
    /// The end index of the change after the edit.
    /// </summary>
    public int NewEndIndex
    {
        get => ByteToIndex(_self.new_end_byte);
        set => _self.new_end_byte = IndexToByte(value);
    }

    /// <summary>
    /// The start position of the change.
    /// </summary>
    public Point StartPosition
    {
        get => new(_self.start_point);
        set => _self.start_point = value._self;
    }

    /// <summary>
    /// The end position of the change before the edit.
    /// </summary>
    public Point OldEndPosition
    {
        get => new(_self.old_end_point);
        set => _self.old_end_point = value._self;
    }

    /// <summary>
    /// The end position of the change after the edit.
    /// </summary>
    public Point NewEndPosition
    {
        get => new(_self.new_end_point);
        set => _self.new_end_point = value._self;
    }

    internal TSInputEdit _self;
}
