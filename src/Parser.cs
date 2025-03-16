//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using static TreeSitter.Native;

namespace TreeSitter;

/// <summary>
/// Represents a parser that can build syntax trees from source code.
/// </summary>
public class Parser : IDisposable, IEquatable<Parser>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Parser"/> class.
    /// </summary>
    public Parser()
    {
        _self = ts_parser_new();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Parser"/> class.
    /// </summary>
    /// <param name="language">The language used for parsing.</param>
    public Parser(Language language) : this()
    {
        Language = language;
    }

    /// <summary>
    /// Finalizes the object.
    /// </summary>
    ~Parser() => Dispose(false);

    internal IntPtr Self => _isDisposed == 0 ? _self : throw new ObjectDisposedException(GetType().FullName);

    /// <summary>
    /// Sets the parser's current logger.
    /// </summary>
    public Logger? Logger
    {
        get
        {
            return _logger;
        }

        set
        {
            _logger = value;

            if (_logger != null)
            {
                var ts_logger = new TSLogger
                {
                    payload = IntPtr.Zero,
                    log = delegate (IntPtr payload, LogType logType, string message)
                    {
                        _logger(logType, message);
                    }
                };
                ts_parser_set_logger(Self, ts_logger);
            }
        }
    }

    /// <summary>
    /// Gets or sets the parser's current language.
    /// </summary>
    public Language? Language
    {
        get
        {
            var language = ts_parser_language(Self);
            return language != IntPtr.Zero ? new Language(language) : null;
        }

        set
        {
            if (!ts_parser_set_language(Self, value != null ? value.Self : IntPtr.Zero))
            {
                throw new InvalidOperationException("The language version is incompatible with this library.");
            }
        }
    }

    /// <summary>
    /// Gets or sets the ranges of text that the parser should include when parsing.
    /// </summary>
    public IReadOnlyList<Range> IncludedRanges
    {
        get
        {
            return new RangeCollection(ts_parser_included_ranges(Self, out var count), count, false);
        }

        set
        {
            var ranges = value.Select(range => range._self).ToArray();
            bool success = ts_parser_set_included_ranges(Self, ranges, (uint)ranges.Length);
            if (!success)
            {
                throw new InvalidOperationException("The given ranges must be ordered from earliest to latest in the document, and they must not overlap.");
            }
        }
    }

    /// <summary>
    /// Parse some source code and create a syntax tree.
    /// </summary>
    /// <param name="source">The source code.</param>
    /// <returns>The syntax tree.</returns>
    public Tree? Parse(string source)
    {
        return Parse(source, null);
    }

    /// <summary>
    /// Parse some source code and update the previous syntax tree.
    /// </summary>
    /// <param name="source">The source code.</param>
    /// <param name="oldTree">The previous syntax tree.</param>
    /// <returns>The syntax tree.</returns>
    /// <remarks>
    /// If you are parsing the document for the first time, pass <see langword="null"/> for
    /// <paramref name="oldTree"/>. Otherwise, if you have already parsed an earlier
    /// version of this document and the document has since been edited, pass the
    /// previous syntax tree so that the unchanged parts of it can be reused.
    /// This will save time and memory. For this to work correctly, you must have
    /// already edited the old syntax tree using the <see cref="Tree.Edit"/> function in a
    /// way that exactly matches the source code changes.
    /// </remarks>
    public Tree? Parse(string source, Tree? oldTree)
    {
        var oldTreePtr = oldTree != null ? oldTree.Self : IntPtr.Zero;
        var tree = ts_parser_parse_string_encoding(Self, oldTreePtr, source, (uint)source.Length * 2, InputEncoding.UTF16LE);
        return tree != IntPtr.Zero ? new Tree(tree, source) : null;
    }

    /// <summary>
    /// Instruct the parser to start the next parse from the beginning.
    /// </summary>
    public void Reset()
    {
        ts_parser_reset(Self);
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
            ts_parser_delete(_self);
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? other)
    {
        return other is Parser query && Equals(query);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Self.GetHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(Parser? other)
    {
        return other is not null && Self == other.Self;
    }

    readonly IntPtr _self;
    int _isDisposed;
    Logger? _logger;
}
