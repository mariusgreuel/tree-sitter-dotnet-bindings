//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using static TreeSitter.Native;

namespace TreeSitter;

/// <summary>
/// Represents the syntactic structure of a source code file.
/// </summary>
public class Tree : IDisposable, ICloneable
{
    internal Tree(IntPtr self, string source)
    {
        _self = self;
        _source = source;
    }

    /// <summary>
    /// Finalizes the object.
    /// </summary>
    ~Tree() => Dispose(false);

    internal IntPtr Self => _isDisposed == 0 ? _self : throw new ObjectDisposedException(GetType().FullName);

    /// <summary>
    /// Gets the source code that this syntax tree was parsed from.
    /// </summary>
    public string Source => _source;

    /// <summary>
    /// Gets the root node of the syntax tree.
    /// </summary>
    public Node RootNode => new(ts_tree_root_node(Self), this);

    /// <summary>
    /// Gets the language that was used to parse the syntax tree.
    /// </summary>
    public Language Language => new(ts_tree_language(Self));

    /// <summary>
    /// Gets an array of included ranges that was used to parse the syntax tree.
    /// </summary>
    public IReadOnlyList<Range> IncludedRanges => new RangeCollection(ts_tree_included_ranges(Self, out var count), count, true);

    /// <summary>
    /// Gets the string content of a range in the text buffer.
    /// </summary>
    /// <param name="startIndex">The start of the range.</param>
    /// <param name="endIndex">The end of the range.</param>
    /// <returns>The string content.</returns>
    public string GetText(int startIndex, int endIndex)
    {
        return _source.Substring(startIndex, endIndex - startIndex);
    }

    /// <summary>
    /// Creates a copy of the syntax tree.
    /// </summary>
    /// <returns>The copied syntax tree.</returns>
    public Tree Copy()
    {
        return new(ts_tree_copy(Self), _source);
    }

    /// <summary>
    /// Gets the root node of the syntax tree, but with its position shifted forward by the given offset.
    /// </summary>
    /// <param name="index">The index to the source code.</param>
    /// <param name="offsetExtent">The offset extent.</param>
    /// <returns></returns>
    public Node GetRootNodeWithIndex(int index, Point offsetExtent)
    {
        return new(ts_tree_root_node_with_offset(Self, IndexToByte(index), offsetExtent._self), this);
    }

    /// <summary>
    /// Gets an array of included ranges that was used to parse the syntax tree.
    /// </summary>
    /// <param name="other">The tree that includes the changes made to this tree.</param>
    public IReadOnlyList<Range> GetChangedRanges(Tree other) => new RangeCollection(ts_tree_get_changed_ranges(Self, other.Self, out var count), count, true);

    /// <summary>
    /// Edits the syntax tree to keep it in sync with source code that has been edited.
    /// </summary>
    /// <param name="edit">The changes to be made.</param>
    public void Edit(Edit edit)
    {
        ts_tree_edit(Self, ref edit._self);
    }

    /// <summary>
    /// Walks the syntax tree.
    /// </summary>
    /// <returns>A tree cursor starting at the root node.</returns>
    public TreeCursor Walk()
    {
        return new(RootNode);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the object.
    /// </summary>
    /// <param name="disposing">Set to <see langword="true"/> to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
        {
            ts_tree_delete(_self);
        }
    }

    /// <inheritdoc/>
    public object Clone()
    {
        return Copy();
    }

    readonly IntPtr _self;
    readonly string _source;
    int _isDisposed;
}
