//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using static TreeSitter.Native;

namespace TreeSitter;

/// <summary>
/// Represents a query object used to find patters in syntax nodes.
/// </summary>
public class Query : IDisposable, IEquatable<Query>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Query"/> class.
    /// </summary>
    /// <param name="language">The language to be used.</param>
    /// <param name="source">The query string containing one or more S-expression patterns.</param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <remarks>
    /// The query is associated with a particular language, and can
    /// only be run on syntax nodes parsed with that language.
    /// </remarks>
    public Query(Language language, string source)
    {
        _self = ts_query_new(language.Self, ToUTF8(source, out var length), length, out var errorOffset, out var errorType);
        if (_self == IntPtr.Zero)
        {
            throw new InvalidOperationException($"Query error at index {ByteToIndex(errorOffset)}: {errorType}");
        }

        _captureNames = GetCaptureNames();
        _strings = GetStrings();
        _patterns = GetPatterns();
    }

    /// <summary>
    /// Finalizes the object.
    /// </summary>
    ~Query() => Dispose(false);

    internal IntPtr Self => _isDisposed == 0 ? _self : throw new ObjectDisposedException(GetType().FullName);

    /// <summary>
    /// Gets the patterns in this query.
    /// </summary>
    public IReadOnlyList<QueryPattern> Patterns => _patterns;

    /// <summary>
    /// Check if a given pattern is guaranteed to match once a given step is reached.
    /// </summary>
    /// <param name="offset">The byte offset in the query's source code.</param>
    /// <returns></returns>
    internal bool IsPatternGuaranteedAtStep(int offset) => ts_query_is_pattern_guaranteed_at_step(Self, IndexToByte(offset));

    /// <summary>
    /// Disable a capture within a query.
    /// </summary>
    /// <param name="name">The capture name.</param>
    public void DisableCapture(string name)
    {
        ts_query_disable_capture(Self, ToUTF8(name, out var length), length);
    }

    /// <summary>
    /// Execute a query on a given node.
    /// </summary>
    /// <param name="node">The node to execute the query on.</param>
    /// <returns>A query cursor containing the results.</returns>
    public QueryCursor Execute(Node node)
    {
        return Execute(node, new());
    }

    /// <summary>
    /// Execute a query with options on a given node.
    /// </summary>
    /// <param name="node">The node to execute the query on.</param>
    /// <param name="options">The query options.</param>
    /// <returns>A query cursor containing the results.</returns>
    public QueryCursor Execute(Node node, QueryOptions options)
    {
        var queryCursor = new QueryCursor();

        if (options.MatchLimit.HasValue)
        {
            queryCursor.MatchLimit = options.MatchLimit.Value;
        }

        if (options.StartIndex.HasValue && options.EndIndex.HasValue)
        {
            queryCursor.SetRange(options.StartIndex.Value, options.EndIndex.Value);
        }

        if (options.StartPoint.HasValue && options.EndPoint.HasValue)
        {
            queryCursor.SetRange(options.StartPoint.Value, options.EndPoint.Value);
        }

        queryCursor.Execute(this, node);
        return queryCursor;
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
            ts_query_delete(_self);
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? other)
    {
        return other is Query query && Equals(query);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Self.GetHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(Query? other)
    {
        return other is not null && Self == other.Self;
    }

    List<string> GetCaptureNames()
    {
        var count = ts_query_capture_count(Self);

        List<string> captureNames = new((int)count);
        for (uint i = 0; i < count; i++)
        {
            captureNames.Add(MarshalString(ts_query_capture_name_for_id(Self, i, out var length), (int)length)!);
        }

        return captureNames;
    }

    List<string> GetStrings()
    {
        var count = ts_query_string_count(Self);

        List<string> strings = new((int)count);
        for (uint i = 0; i < count; i++)
        {
            strings.Add(MarshalString(ts_query_string_value_for_id(Self, i, out var length), (int)length)!);
        }

        return strings;
    }

    List<QueryPattern> GetPatterns()
    {
        var count = ts_query_pattern_count(Self);

        List<QueryPattern> patterns = new((int)count);
        for (uint i = 0; i < count; i++)
        {
            patterns.Add(new QueryPattern(this, i));
        }

        return patterns;
    }

    readonly IntPtr _self;
    internal readonly List<string> _captureNames;
    internal readonly List<string> _strings;
    internal readonly List<QueryPattern> _patterns;
    int _isDisposed;
}
