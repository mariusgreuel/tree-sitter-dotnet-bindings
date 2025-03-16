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
/// Represents a lookahead iterator.
/// </summary>
/// <remarks>
/// Lookahead iterators can be useful to generate suggestions and improve syntax
/// error diagnostics.To get symbols valid in an ERROR node, use the lookahead
/// iterator on its first leaf node state.For `MISSING` nodes, a lookahead
/// iterator created on the previous non-extra leaf node may be appropriate.
/// </remarks>
public class LookaheadIterator : IDisposable, IEquatable<LookaheadIterator>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LookaheadIterator"/> class.
    /// </summary>
    /// <param name="language">The language to be used.</param>
    /// <param name="state">The parse state.</param>
    /// <exception cref="InvalidOperationException"></exception>
    public LookaheadIterator(Language language, ushort state)
    {
        _self = ts_lookahead_iterator_new(language.Self, state);
        if (_self == IntPtr.Zero)
        {
            throw new InvalidOperationException("The parser state is invalid for the language.");
        }
    }

    /// <summary>
    /// Finalizes the object.
    /// </summary>
    ~LookaheadIterator() => Dispose(false);

    internal IntPtr Self => _isDisposed == 0 ? _self : throw new ObjectDisposedException(GetType().FullName);

    /// <summary>
    /// Gets the current language of the lookahead iterator.
    /// </summary>
    public Language Language => new(ts_lookahead_iterator_language(Self));

    /// <summary>
    /// Gets the valid symbols in the given parse state.
    /// </summary>
    public IEnumerable<LookaheadSymbol> Symbols => new LookaheadSymbolCollection(this);

    /// <summary>
    /// Resets the lookahead iterator to another state.
    /// </summary>
    /// <param name="state">The new state.</param>
    /// <returns>
    /// <see langword="true"/> if the iterator was reset to the given state; otherwise <see langword="false"/>.
    /// </returns>
    public bool ResetState(ushort state)
    {
        return ts_lookahead_iterator_reset_state(Self, state);
    }

    /// <summary>
    /// Resets the lookahead iterator.
    /// </summary>
    /// <param name="language">The new language.</param>
    /// <param name="state">The new state.</param>
    /// <returns>
    /// <see langword="true"/> if the iterator was reset to the given state; otherwise <see langword="false"/>.
    /// </returns>
    public bool Reset(Language language, ushort state)
    {
        return ts_lookahead_iterator_reset(Self, language.Self, state);
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
            ts_lookahead_iterator_delete(_self);
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? other)
    {
        return other is LookaheadIterator lookaheadIterator && Equals(lookaheadIterator);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Self.GetHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(LookaheadIterator? other)
    {
        return other is not null && Self == other.Self;
    }

    class LookaheadSymbolCollection : IEnumerable<LookaheadSymbol>
    {
        public LookaheadSymbolCollection(LookaheadIterator lookaheadIterator)
        {
            _lookaheadIterator = lookaheadIterator;
        }

        public IEnumerator<LookaheadSymbol> GetEnumerator()
        {
            while (ts_lookahead_iterator_next(_lookaheadIterator.Self))
            {
                var id = ts_lookahead_iterator_current_symbol(_lookaheadIterator.Self);
                var name = MarshalString(ts_lookahead_iterator_current_symbol_name(_lookaheadIterator.Self)) ?? "<null>";
                yield return new LookaheadSymbol(id, name);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        readonly LookaheadIterator _lookaheadIterator;
    }

    readonly IntPtr _self;
    int _isDisposed;
}
