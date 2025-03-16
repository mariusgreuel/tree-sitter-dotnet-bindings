//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using static TreeSitter.Native;

namespace TreeSitter;

/// <summary>
/// Represents a tree cursor for walking a syntax tree efficiently.
/// </summary>
/// <remarks>
///  A tree cursor allows you to walk a syntax tree more efficiently than is
///  possible using the 'Node' functions.It is a mutable object that is always
///  on a certain syntax node, and can be moved imperatively to different nodes.
///  Note that the given node is considered the root of the cursor,
///  and the cursor cannot walk outside this node.
/// </remarks>
public class TreeCursor : IDisposable, ICloneable
{
    TreeCursor(TSTreeCursor self, Tree tree)
    {
        _self = self;
        _tree = tree;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeCursor"/> class.
    /// </summary>
    /// <param name="node">The node to start from.</param>
    public TreeCursor(Node node) : this(ts_tree_cursor_new(node.Self), node.Tree)
    {
    }

    /// <summary>
    /// Finalizes the object.
    /// </summary>
    ~TreeCursor() => Dispose(false);

    ref TSTreeCursor GetSelf()
    { 
        if (_isDisposed != 0)
            throw new ObjectDisposedException(GetType().FullName);

        return ref _self;
    }

    /// <summary>
    /// Gets the tree cursor's current node.
    /// </summary>
    public Node CurrentNode => new(ts_tree_cursor_current_node(ref GetSelf()), _tree);

    /// <summary>
    /// Gets the numerical field ID of the tree cursor's current node,
    /// or zero, if the current node doesn't have a field.
    /// </summary>
    public ushort CurrentFieldId => ts_tree_cursor_current_field_id(ref GetSelf());

    /// <summary>
    /// Gets the field name of the tree cursor's current node,
    /// or <see langword="null"/>, if the current node doesn't have a field.
    /// </summary>
    public string? CurrentFieldName => MarshalString(ts_tree_cursor_current_field_name(ref GetSelf()));

    /// <summary>
    /// Gets the depth of the cursor's current node relative to
    /// the original node that the cursor was constructed with.
    /// </summary>
    public int CurrentDepth => (int)ts_tree_cursor_current_depth(ref GetSelf());

    /// <summary>
    /// Gets the index of the cursor's current node out of all of the descendants of
    /// the original node that the cursor was constructed with.
    /// </summary>
    public int CurrentDescendantIndex => (int)ts_tree_cursor_current_descendant_index(ref GetSelf());

    /// <summary>
    /// Re-initializes the cursor to start at the original node that the cursor was constructed with.
    /// </summary>
    /// <param name="node">The node to start with.</param>
    public void Reset(Node node)
    {
        ts_tree_cursor_reset(ref GetSelf(), node.Self);
    }

    /// <summary>
    /// Re-initializes the cursor to the same position as another cursor.
    /// </summary>
    /// <param name="cursor">The other cursor</param>
    /// <remarks>
    /// Unlike <see cref="Reset"/>, this will not lose parent
    /// information and allows reusing already created cursors.
    /// </remarks>
    public void ResetTo(TreeCursor cursor)
    {
        ts_tree_cursor_reset_to(ref GetSelf(), ref cursor._self);
    }

    /// <summary>
    /// Moves the cursor to the parent of its current node.
    /// </summary>
    /// <returns><see langword="true"/> if the cursor successfully moved; otherwise <see langword="false"/>.</returns>
    public bool GotoParent()
    {
        return ts_tree_cursor_goto_parent(ref GetSelf());
    }

    /// <summary>
    /// Moves the cursor to the first child of its current node.
    /// </summary>
    /// <returns><see langword="true"/> if the cursor successfully moved; otherwise <see langword="false"/>.</returns>
    public bool GotoFirstChild()
    {
        return ts_tree_cursor_goto_first_child(ref GetSelf());
    }

    /// <summary>
    /// Moves the cursor to the last child of its current node.
    /// </summary>
    /// <returns><see langword="true"/> if the cursor successfully moved; otherwise <see langword="false"/>.</returns>
    public bool GotoLastChild()
    {
        return ts_tree_cursor_goto_last_child(ref GetSelf());
    }

    /// <summary>
    /// Moves the cursor to the next sibling of its current node.
    /// </summary>
    /// <returns><see langword="true"/> if the cursor successfully moved; otherwise <see langword="false"/>.</returns>
    public bool GotoNextSibling()
    {
        return ts_tree_cursor_goto_next_sibling(ref GetSelf());
    }

    /// <summary>
    /// Moves the cursor to the previous sibling of its current node.
    /// </summary>
    /// <returns><see langword="true"/> if the cursor successfully moved; otherwise <see langword="false"/>.</returns>
    public bool GotoPreviousSibling()
    {
        return ts_tree_cursor_goto_previous_sibling(ref GetSelf());
    }

    /// <summary>
    /// Move the cursor to the node that is the nth descendant of the original node that the cursor was constructed with.
    /// </summary>
    /// <param name="index">The index of the descendant, where zero represents the original node itself.</param>
    public void GotoDescendant(int index)
    {
        ts_tree_cursor_goto_descendant(ref GetSelf(), (uint)index);
    }

    /// <summary>
    /// Move the cursor to the first child of its current node that contains or starts after the given offset.
    /// </summary>
    /// <param name="goal">The goal offset.</param>
    /// <returns>The index of the child node if one was found, or -1, if no such child was found.</returns>
    public long GotoFirstChild(int goal)
    {
        return ts_tree_cursor_goto_first_child_for_byte(ref GetSelf(), IndexToByte(goal));
    }

    /// <summary>
    /// Move the cursor to the first child of its current node that contains or starts after the given point.
    /// </summary>
    /// <param name="goal">The goal point.</param>
    /// <returns>The index of the child node if one was found, or -1, if no such child was found.</returns>
    public long GotoFirstChild(Point goal)
    {
        return ts_tree_cursor_goto_first_child_for_point(ref GetSelf(), goal._self);
    }

    /// <summary>
    /// Creates a copy of the tree cursor.
    /// </summary>
    /// <returns>The copied tree cursor.</returns>
    public TreeCursor Copy()
    {
        return new(ts_tree_cursor_copy(ref GetSelf()), _tree);
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
            ts_tree_cursor_delete(ref _self);
        }
    }

    /// <inheritdoc/>
    public object Clone()
    {
        return Copy();
    }

    TSTreeCursor _self;
    readonly Tree _tree;
    int _isDisposed;
}
