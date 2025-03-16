//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using System.Collections;
using System.Diagnostics;

using static TreeSitter.Native;

namespace TreeSitter;

/// <summary>
/// Represents a node within a syntax tree.
/// </summary>
[DebuggerDisplay("{Type}")]
public class Node : IEquatable<Node>
{
    internal Node(TSNode self, Tree tree)
    {
        _self = self;
        _tree = tree;
    }

    internal TSNode Self => _self;

    /// <summary>
    /// Gets the unique ID of this node.
    /// </summary>
    public IntPtr Id => _self.id;

    /// <summary>
    /// Gets the start position in the source code.
    /// </summary>
    public Point StartPosition => new(ts_node_start_point(_self));

    /// <summary>
    /// Gets the end position in the source code.
    /// </summary>
    public Point EndPosition => new(ts_node_end_point(_self));

    /// <summary>
    /// Gets the start index in the source code.
    /// </summary>
    public int StartIndex => ByteToIndex(ts_node_start_byte(_self));

    /// <summary>
    /// Gets the end index in the source code.
    /// </summary>
    public int EndIndex => ByteToIndex(ts_node_end_byte(_self));

    /// <summary>
    /// Get the range in the source code.
    /// </summary>
    public Range Range => new(StartPosition, EndPosition, StartIndex, EndIndex);

    /// <summary>
    /// Gets the tree that this node belongs to.
    /// </summary>
    public Tree Tree => _tree;

    /// <summary>
    /// Gets the node's type.
    /// </summary>
    public string Type => MarshalString(ts_node_type(_self))!;

    /// <summary>
    /// Gets the node's type as a numerical ID.
    /// </summary>
    public ushort TypeId => ts_node_symbol(_self);

    /// <summary>
    /// Gets the node's type as it appears in the grammar ignoring aliases.
    /// </summary>
    public string GrammarName => MarshalString(ts_node_grammar_type(_self))!;

    /// <summary>
    /// Gets the node's type as a numerical ID as it appears in the grammar, ignoring aliases.
    /// </summary>
    /// <remarks>
    /// GrammarId should be used in <see cref="Language.GetNextState"/> instead of <see cref="TypeId"/>.
    /// </remarks>
    public ushort GrammarId => ts_node_grammar_symbol(_self);

    /// <summary>
    /// Gets the string content of this node.
    /// </summary>
    public string Text => _tree.GetText(StartIndex, EndIndex);

    /// <summary>
    /// Gets the node's language.
    /// </summary>
    public Language Language => new(ts_node_language(_self));

    /// <summary>
    /// Checks if the node is named.
    /// </summary>
    /// <remarks>
    /// Named nodes correspond to named rules in the grammar, whereas
    /// anonymous nodes correspond to string literals in the grammar.
    /// </remarks>
    public bool IsNamed => ts_node_is_named(_self);

    /// <summary>
    /// Checks if the node is extra.
    /// </summary>
    /// <remarks>
    /// Extra nodes represent things like comments, which are
    /// not required the grammar, but can appear anywhere.
    /// </remarks>
    public bool IsExtra => ts_node_is_extra(_self);

    /// <summary>
    /// Checks if this node represents a syntax error.
    /// </summary>
    /// <remarks>
    /// Syntax errors represent parts of the code that could not
    /// be incorporated into a valid syntax tree.
    /// </remarks>
    public bool IsError => ts_node_is_error(_self);

    /// <summary>
    /// Checks if the node is missing.
    /// </summary>
    /// <remarks>
    /// Missing nodes are inserted by the parser in order
    /// to recover from certain kinds of syntax errors.
    /// </remarks>
    public bool IsMissing => ts_node_is_missing(_self);

    /// <summary>
    /// Checks if a syntax node has been edited.
    /// </summary>
    public bool HasChanges => ts_node_has_changes(_self);

    /// <summary>
    /// Check if this node represents a syntax error or
    /// contains any syntax errors anywhere within it.
    /// </summary>
    public bool HasError => ts_node_has_error(_self);

    /// <summary>
    /// Gets the parse state.
    /// </summary>
    public ushort ParseState => ts_node_parse_state(_self);

    /// <summary>
    /// Gets the parse state after this node.
    /// </summary>
    public ushort NextParseState => ts_node_next_parse_state(_self);

    /// <summary>
    /// Gets the node's children.
    /// </summary>
    public IReadOnlyList<Node> Children => new ChildCollection(this);

    /// <summary>
    /// Gets the node's named children.
    /// </summary>
    public IReadOnlyList<Node> NamedChildren => new NamedChildCollection(this);

    /// <summary>
    /// Gets the node's children by field name.
    /// </summary>
    public IReadOnlyList<KeyValuePair<string?, Node>> Fields => new FieldCollection(this);

    /// <summary>
    /// Gets the node's immediate parent.
    /// </summary>
    public Node? Parent => NodeOrNull(ts_node_parent(_self));

    /// <summary>
    /// Gets the node's next sibling.
    /// </summary>
    public Node? NextSibling => NodeOrNull(ts_node_next_sibling(_self));

    /// <summary>
    /// Gets the node's previous sibling.
    /// </summary>
    public Node? PreviousSibling => NodeOrNull(ts_node_prev_sibling(_self));

    /// <summary>
    /// Gets the node's next named sibling.
    /// </summary>
    public Node? NextNamedSibling => NodeOrNull(ts_node_next_named_sibling(_self));

    /// <summary>
    /// Gets the node's previous named sibling.
    /// </summary>
    public Node? PreviousNamedSibling => NodeOrNull(ts_node_prev_named_sibling(_self));

    /// <summary>
    /// Gets the node's first child.
    /// </summary>
    public Node? FirstChild => Children.Count > 0 ? Children[0] : null;

    /// <summary>
    /// Gets the node's last child.
    /// </summary>
    public Node? LastChild => Children.Count > 0 ? Children[Children.Count - 1] : null;

    /// <summary>
    /// Gets the node's first named child.
    /// </summary>
    public Node? FirstNamedChild => NamedChildren.Count > 0 ? NamedChildren[0] : null;

    /// <summary>
    /// Gets the node's last named child.
    /// </summary>
    public Node? LastNamedChild => NamedChildren.Count > 0 ? NamedChildren[NamedChildren.Count - 1] : null;

    /// <summary>
    /// Gets the node's number of descendants, including one for the node itself.
    /// </summary>
    public int DescendantCount => (int)ts_node_descendant_count(_self);

    /// <summary>
    /// Gets the S-expression representation of the node.
    /// </summary>
    /// <returns>An S-expression representing the node as a string.</returns>
    public string Expression
    {
        get
        {
            var str = ts_node_string(_self);

            try
            {
                return MarshalString(str)!;
            }
            finally
            {
                ts_free(str);
            }
        }
    }

    /// <summary>
    /// Gets the field name of this node's child at the given index.
    /// </summary>
    /// <param name="index">The child index.</param>
    /// <returns>The field name of the child, or <see langword="null"/>, if no field is found.</returns>
    public string? GetFieldNameForChild(int index)
    {
        return MarshalString(ts_node_field_name_for_child(_self, (uint)index));
    }

    /// <summary>
    /// Gets the field name of this node's named child at the given index.
    /// </summary>
    /// <param name="index">The child index.</param>
    /// <returns>The field name of the child, or <see langword="null"/>, if no field is found.</returns>
    public string? GetFieldNameForNamedChild(int index)
    {
        return MarshalString(ts_node_field_name_for_named_child(_self, (uint)index));
    }

    /// <summary>
    /// Gets the node's child with the given field name.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <returns>The child node with the given field name.</returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public Node this[string name] => GetChildForField(name) ?? throw new KeyNotFoundException($"Field '{name}' does not exist");

    /// <summary>
    /// Gets the node's child with the given field ID.
    /// </summary>
    /// <param name="id">The field ID.</param>
    /// <returns>The child node with the given field ID.</returns>
    public Node this[ushort id] => GetChildForField(id) ?? throw new KeyNotFoundException($"Field '{id}' does not exist");

    /// <summary>
    /// Gets the node's child with the given field name.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <returns>The child node with the given field name, or <see langword="null"/>, if no field is found.</returns>
    public Node? GetChildForField(string name)
    {
        return NodeOrNull(ts_node_child_by_field_name(_self, ToUTF8(name, out var length), length));
    }

    /// <summary>
    /// Gets the node's child with the given field ID.
    /// </summary>
    /// <param name="id">The field ID.</param>
    /// <returns>The child node with the given field ID, or <see langword="null"/>, if no field is found.</returns>
    public Node? GetChildForField(ushort id)
    {
        return NodeOrNull(ts_node_child_by_field_id(_self, id));
    }

    /// <summary>
    /// Gets the node's children with the given field name.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <returns>A collection of child nodes with the given field name.</returns>
    public IReadOnlyList<Node> GetChildrenForField(string name)
    {
        return GetChildrenForField(Language.GetFieldIdFromName(name));
    }

    /// <summary>
    /// Gets the node's children with the given field ID.
    /// </summary>
    /// <param name="id">The field ID.</param>
    /// <returns>A collection of child nodes with the given field ID.</returns>
    public IReadOnlyList<Node> GetChildrenForField(ushort id)
    {
        List<Node> children = new();

        var cursor = Walk();
        if (cursor.GotoFirstChild())
        {
            do
            {
                if (cursor.CurrentFieldId == id)
                {
                    children.Add(cursor.CurrentNode);
                }
            }
            while (cursor.GotoNextSibling());
        }

        return children;
    }

    /// <summary>
    /// Gets the node's first child that contains or starts after the given offset in the text buffer.
    /// </summary>
    /// <param name="index">The offset in the text buffer.</param>
    /// <returns>The child node at the given offset.</returns>
    public Node? GetFirstChildForIndex(int index)
    {
        return NodeOrNull(ts_node_first_child_for_byte(_self, IndexToByte(index)));
    }

    /// <summary>
    /// Gets the node's first named child that contains or starts after the given offset in the text buffer.
    /// </summary>
    /// <param name="index">The offset in the text buffer.</param>
    /// <returns>The child node at the given offset.</returns>
    public Node? GetFirstNamedChildForIndex(int index)
    {
        return NodeOrNull(ts_node_first_named_child_for_byte(_self, IndexToByte(index)));
    }

    /// <summary>
    /// Gets the node that contains 'descendant'.
    /// </summary>
    /// <returns>The child node that contains 'descendant'.</returns>
    public Node? GetChildWithDescendant(Node descendant)
    {
        return NodeOrNull(ts_node_child_with_descendant(_self, descendant._self));
    }

    /// <summary>
    /// Get the smallest node within this node that spans the given range.
    /// </summary>
    /// <param name="startIndex">The start index of the range.</param>
    /// <param name="endIndex">The end index of the range.</param>
    /// <returns>The node that spans the given range.</returns>
    public Node? GetDescendantForIndex(int startIndex, int endIndex)
    {
        return NodeOrNull(ts_node_descendant_for_byte_range(_self, IndexToByte(startIndex), IndexToByte(endIndex)));
    }

    /// <summary>
    /// Get the smallest named node within this node that spans the given range.
    /// </summary>
    /// <param name="startIndex">The start index of the range.</param>
    /// <param name="endIndex">The end index of the range.</param>
    /// <returns>The node that spans the given range.</returns>
    public Node? GetNamedDescendantForRange(int startIndex, int endIndex)
    {
        return NodeOrNull(ts_node_named_descendant_for_byte_range(_self, IndexToByte(startIndex), IndexToByte(endIndex)));
    }

    /// <summary>
    /// Gets the smallest node within this node that spans the given position.
    /// </summary>
    /// <param name="position">The start position of the range.</param>
    /// <returns>The node that spans the given range.</returns>
    public Node? GetDescendantForPosition(Point position)
    {
        return GetDescendantForPosition(position, position);
    }

    /// <summary>
    /// Gets the smallest node within this node that spans the given range.
    /// </summary>
    /// <param name="startPoint">The start position of the range.</param>
    /// <param name="endPoint">The end position of the range.</param>
    /// <returns>The node that spans the given range.</returns>
    public Node? GetDescendantForPosition(Point startPoint, Point endPoint)
    {
        return NodeOrNull(ts_node_descendant_for_point_range(_self, startPoint._self, endPoint._self));
    }

    /// <summary>
    /// Gets the smallest named node within this node that spans the given position.
    /// </summary>
    /// <param name="position">The start position of the range.</param>
    /// <returns>The node that spans the given range.</returns>
    public Node? GetNamedDescendantForPosition(Point position)
    {
        return GetNamedDescendantForPosition(position, position);
    }

    /// <summary>
    /// Gets the smallest named node within this node that spans the given range.
    /// </summary>
    /// <param name="start">The start position of the range.</param>
    /// <param name="end">The end position of the range.</param>
    /// <returns>The node that spans the given range.</returns>
    public Node? GetNamedDescendantForPosition(Point start, Point end)
    {
        return NodeOrNull(ts_node_named_descendant_for_point_range(_self, start._self, end._self));
    }

    /// <summary>
    /// Edits the node to keep it in-sync with source code that has been edited.
    /// </summary>
    /// <param name="edit">The changes to be made.</param>
    public void Edit(Edit edit)
    {
        ts_node_edit(_self, ref edit._self);
    }

    /// <summary>
    /// Walks the syntax tree.
    /// </summary>
    /// <returns>A tree cursor starting at the current node.</returns>
    public TreeCursor Walk()
    {
        return new(this);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Expression;
    }

    /// <inheritdoc/>
    public override bool Equals(object? other)
    {
        return other is Node node && Equals(node);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return _self.id.GetHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(Node? other)
    {
        return other is not null && ts_node_eq(_self, other._self);
    }

    /// <summary>
    /// Compares two <see cref="Node"/> objects for equality.
    /// </summary>
    /// <param name="left">The first node.</param>
    /// <param name="right">The second node.</param>
    /// <returns><see langword="true"/> if the nodes are equal; otherwise <see langword="false"/>.</returns>
    public static bool operator ==(Node left, Node right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Compares two <see cref="Node"/> objects for inequality.
    /// </summary>
    /// <param name="left">The first node.</param>
    /// <param name="right">The second node.</param>
    /// <returns><see langword="true"/> if the nodes are not equal; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(Node left, Node right)
    {
        return !(left == right);
    }

    Node? NodeOrNull(TSNode self)
    {
        return !ts_node_is_null(self) ? new Node(self, _tree) : null;
    }

    class ChildCollection : IReadOnlyList<Node>
    {
        public ChildCollection(Node node)
        {
            _node = node;
        }

        public int Count => (int)ts_node_child_count(_node._self);

        public Node this[int index]
        {
            get
            {
                var child = ts_node_child(_node._self, (uint)index);
                if (ts_node_is_null(child))
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return new(child, _node._tree);
            }
        }

        public IEnumerator<Node> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        readonly Node _node;
    }

    class NamedChildCollection : IReadOnlyList<Node>
    {
        public NamedChildCollection(Node node)
        {
            _node = node;
        }

        public int Count => (int)ts_node_named_child_count(_node._self);

        public Node this[int index]
        {
            get
            {
                var child = ts_node_named_child(_node._self, (uint)index);
                if (ts_node_is_null(child))
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return new(child, _node._tree);
            }
        }

        public IEnumerator<Node> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        readonly Node _node;
    }

    class FieldCollection : IReadOnlyList<KeyValuePair<string?, Node>>
    {
        public FieldCollection(Node node)
        {
            _node = node;
        }

        public int Count => (int)ts_node_child_count(_node._self);

        public KeyValuePair<string?, Node> this[int index] => new(MarshalString(ts_node_field_name_for_child(_node._self, (uint)index)), _node.Children[index]);

        public IEnumerator<KeyValuePair<string?, Node>> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        readonly Node _node;
    }

    readonly TSNode _self;
    readonly Tree _tree;
}
