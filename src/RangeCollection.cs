//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using System.Collections;

using static TreeSitter.Native;

namespace TreeSitter;

internal class RangeCollection : IDisposable, IReadOnlyList<Range>
{
    public RangeCollection(IntPtr ranges, uint count, bool dispose)
    {
        _ranges = ranges;
        _count = count;
        _dispose = dispose;
    }

    /// <summary>
    /// Finalizes the object.
    /// </summary>
    ~RangeCollection() => Dispose(false);

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
        if (_dispose && Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
        {
            ts_free(_ranges);
        }
    }

    public int Count => (int)_count;

    public Range this[int index] => new(MarshalArrayItem<TSRange>(_ranges, index));

    public IEnumerator<Range> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    readonly IntPtr _ranges;
    readonly uint _count;
    readonly bool _dispose;
    int _isDisposed;
}
