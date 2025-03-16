//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using System.Collections;

using static TreeSitter.Native;

namespace TreeSitter;

/// <summary>
/// Represents a cursor for executing a given query.
/// </summary>
/// <remarks>
/// The cursor stores the state that is needed to iteratively search
/// for matches. To use the query cursor, first call 'Execute' to
/// start running a given query on a given syntax node. Then, there are
/// two options for consuming the results of the query:
/// 1. Iterate over the matches using the 'Matches' property.
/// 2. Iterate over the captures using the 'Captures' property.
/// If you don't care about consuming all of the results, you can stop iterating at any point.
/// You can then start executing another query on another node by calling 'Execute' again.
/// </remarks>
public class QueryCursor : IDisposable, IEquatable<QueryCursor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryCursor"/> class.
    /// </summary>
    public QueryCursor()
    {
        _self = ts_query_cursor_new();
    }

    /// <summary>
    /// Finalizes the object.
    /// </summary>
    ~QueryCursor() => Dispose(false);

    internal IntPtr Self => _isDisposed == 0 ? _self : throw new ObjectDisposedException(GetType().FullName);

    /// <summary>
    /// Gets or sets the maximum number of in-progress matches allowed.
    /// </summary>
    /// <remarks>
    /// By default, query cursors allow any number of pending matches.
    /// </remarks>
    public uint MatchLimit
    {
        get
        {
            return ts_query_cursor_match_limit(Self);
        }

        set
        {
            ts_query_cursor_set_match_limit(Self, value);
        }
    }

    /// <summary>
    /// Check if, on its last execution, this cursor exceeded its maximum number of in-progress matches.
    /// </summary>
    public bool IsMatchLimitExceeded => ts_query_cursor_did_exceed_match_limit(Self);

    /// <summary>
    /// Gets the matches of the currently running query.
    /// </summary>
    public IEnumerable<QueryMatch> Matches => new MatchCollection(this);

    /// <summary>
    /// Gets the captures of the currently running query.
    /// </summary>
    public IEnumerable<QueryCapture> Captures => new CaptureCollection(this);

    /// <summary>
    /// Set the range of bytes in which the query will be executed.
    /// </summary>
    /// <param name="startIndex">The start index.</param>
    /// <param name="endIndex">The end index.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void SetRange(int startIndex, int endIndex)
    {
        if (!ts_query_cursor_set_byte_range(Self, IndexToByte(startIndex), IndexToByte(endIndex)))
        {
            throw new InvalidOperationException("Start byte is greater than the end byte.");
        }
    }

    /// <summary>
    /// Set the range of (row, column) positions in which the query will be executed.
    /// </summary>
    /// <param name="startPoint">The start point.</param>
    /// <param name="endPoint">The end point.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public void SetRange(Point startPoint, Point endPoint)
    {
        if (!ts_query_cursor_set_point_range(Self, startPoint._self, endPoint._self))
        {
            throw new InvalidOperationException("Start point is greater than the end point.");
        }
    }

    /// <summary>
    /// Execute a query on a given node.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="node">The node to execute the query on.</param>
    public void Execute(Query query, Node node)
    {
        _query = query;
        _tree = node.Tree;
        ts_query_cursor_exec(Self, query.Self, node.Self);
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
            ts_query_cursor_delete(_self);
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? other)
    {
        return other is QueryCursor queryCursor && Equals(queryCursor);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Self.GetHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(QueryCursor? other)
    {
        return other is not null && Self == other.Self;
    }

    class MatchCollection : IEnumerable<QueryMatch>
    {
        public MatchCollection(QueryCursor queryCursor)
        {
            _queryCursor = queryCursor;
        }

        public IEnumerator<QueryMatch> GetEnumerator()
        {
            if (_queryCursor._tree == null || _queryCursor._query == null)
            {
                yield break;
            }

            while (ts_query_cursor_next_match(_queryCursor.Self, out var match))
            {
                var queryMatch = new QueryMatch(match, _queryCursor._query, _queryCursor._tree);
                var pattern = _queryCursor._query.Patterns[queryMatch.PatternIndex];
                if (pattern.MatchesPredicates(queryMatch.Captures))
                {
                    queryMatch._userPredicates = pattern._userPredicates;
                    queryMatch._assertedProperties = pattern._assertedProperties;
                    queryMatch._refutedProperties = pattern._refutedProperties;
                    queryMatch._setProperties = pattern._setProperties;
                    yield return queryMatch;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        readonly QueryCursor _queryCursor;
    }

    class CaptureCollection : IEnumerable<QueryCapture>
    {
        public CaptureCollection(QueryCursor queryCursor)
        {
            _queryCursor = queryCursor;
        }

        public IEnumerator<QueryCapture> GetEnumerator()
        {
            if (_queryCursor._tree == null || _queryCursor._query == null)
            {
                yield break;
            }

            while (ts_query_cursor_next_capture(_queryCursor.Self, out var match, out var captureIndex))
            {
                var queryMatch = new QueryMatch(match, _queryCursor._query, _queryCursor._tree);
                var pattern = _queryCursor._query.Patterns[queryMatch.PatternIndex];
                if (pattern.MatchesPredicates(queryMatch.Captures))
                {
                    var capture = queryMatch.Captures[(int)captureIndex];
                    capture._userPredicates = pattern._userPredicates;
                    capture._assertedProperties = pattern._assertedProperties;
                    capture._refutedProperties = pattern._refutedProperties;
                    capture._setProperties = pattern._setProperties;
                    yield return capture;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        readonly QueryCursor _queryCursor;
    }

    readonly IntPtr _self;
    int _isDisposed;
    Query? _query;
    Tree? _tree;
}
