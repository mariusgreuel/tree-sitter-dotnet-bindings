//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using static TreeSitter.Native;

namespace TreeSitter;

/// <summary>
/// Represents a language used for parsing source code.
/// </summary>
public class Language : IDisposable, IEquatable<Language>, ICloneable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Language"/> class.
    /// </summary>
    /// <param name="self">A pointer to a raw language object.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="self"/> is <see cref="IntPtr.Zero"/>.</exception>
    public Language(IntPtr self)
    {
        PreloadTreesitterLibrary();

        if (self == IntPtr.Zero)
        {
            throw new ArgumentException("Pointer to the language object cannot be null.", nameof(self));
        }

        _self = self;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Language"/> class.
    /// </summary>
    /// <param name="id">The language identifier.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or empty.</exception>
    /// <remarks>
    /// The language identifier determines the library and function to load.
    /// The library is determined by prefixing the identifier with "tree-sitter-".
    /// The function name is determined by prefixing the identifier with "tree_sitter_".
    /// </remarks>
    public Language(string id) : this(GetLibraryNameFromId(id), GetFunctionNameFromId(id))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Language"/> class.
    /// </summary>
    /// <param name="library">The library to load containing the parser language.</param>
    /// <param name="function">The function name of the function that creates a new language object.</param>
    public Language(string library, string function) : this(LoadLanguage(library, function))
    {
    }

    /// <summary>
    /// Finalizes the object.
    /// </summary>
    ~Language() => Dispose(false);

    internal IntPtr Self => _isDisposed == 0 ? _self : throw new ObjectDisposedException(GetType().FullName);

    /// <summary>
    /// Gets the language's name.
    /// </summary>
    public string Name => MarshalString(ts_language_name(Self)) ?? "<null>";

    /// <summary>
    /// Gets the language's ABI version.
    /// </summary>
    public uint AbiVersion => ts_language_abi_version(Self);

    /// <summary>
    /// Gets the number of valid states in this language.
    /// </summary>
    public int StateCount => (int)ts_language_state_count(Self);

    /// <summary>
    /// Gets a collection of distinct node types in the language.
    /// </summary>
    public IReadOnlyList<Symbol> Symbols
    {
        get
        {
            if (_symbols == null)
            {
                var count = ts_language_symbol_count(Self);

                _symbols = new((int)count);
                for (ushort i = 0; i < count; i++)
                {
                    var name = MarshalString(ts_language_symbol_name(Self, i)) ?? "<null>";
                    var type = ts_language_symbol_type(Self, i);
                    _symbols.Add(new Symbol(i, name, type));
                }
            }

            return _symbols;
        }
    }

    /// <summary>
    /// Gets a collection of all supertype symbols for the language.
    /// </summary>
    public IReadOnlyList<SupertypeSymbol> SupertypeSymbols
    {
        get
        {
            if (_supertypeSymbols == null)
            {
                var supertypes = ts_language_supertypes(Self, out var count);

                _supertypeSymbols = new((int)count);
                for (int i = 0; i < (int)count; i++)
                {
                    var id = MarshalArrayItem<ushort>(supertypes, i);
                    _supertypeSymbols.Add(new(Symbols[id], GetSubtypeSymbols(id)));
                }
            }

            return _supertypeSymbols;

            List<Symbol> GetSubtypeSymbols(ushort supertypeId)
            {
                var subtypes = ts_language_subtypes(Self, supertypeId, out var count);

                List<Symbol> subtypeSymbols = new((int)count);
                for (int i = 0; i < (int)count; i++)
                {
                    var id = MarshalArrayItem<ushort>(subtypes, i);
                    subtypeSymbols.Add(Symbols[id]);
                }

                return subtypeSymbols;
            }
        }
    }

    /// <summary>
    /// Gets a collection of distinct field names in the language.
    /// </summary>
    public IReadOnlyList<Field> Fields
    {
        get
        {
            if (_fields == null)
            {
                var count = ts_language_field_count(Self);

                _fields = new((int)count);
                for (ushort i = 0; i < count; i++)
                {
                    var name = MarshalString(ts_language_field_name_for_id(Self, i)) ?? "<null>";
                    _fields.Add(new Field(i, name));
                }
            }

            return _fields;
        }
    }

    /// <summary>
    /// Creates a copy of the language.
    /// </summary>
    /// <returns>The copied language.</returns>
    public Language Copy()
    {
        return new(ts_language_copy(Self));
    }

    /// <summary>
    /// Gets the numerical id for the given node type string.
    /// </summary>
    /// <param name="name">The symbol name.</param>
    /// <param name="isNamed"><see langword="true"/> if node is named.</param>
    /// <returns>The symbol ID.</returns>
    public ushort GetSymbolIdFromName(string name, bool isNamed)
    {
        return ts_language_symbol_for_name(Self, ToUTF8(name, out var length), length, isNamed);
    }

    /// <summary>
    /// Gets the numerical id for the given field name string.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <returns>The field ID.</returns>
    public ushort GetFieldIdFromName(string name)
    {
        return ts_language_field_id_for_name(Self, ToUTF8(name, out var length), length);
    }

    /// <summary>
    /// Gets the next parse state.
    /// </summary>
    /// <param name="state">The nodes state.</param>
    /// <param name="symbol">The nodes grammar type.</param>
    /// <returns>
    /// Combine this with lookahead iterators to generate
    /// completion suggestions or valid symbols in error nodes.
    /// Use <see cref="Node.GrammarId"/> for valid symbols.
    /// </returns>
    public ushort GetNextState(ushort state, ushort symbol)
    {
        return ts_language_next_state(Self, state, symbol);
    }

    /// <summary>
    /// Creates a new query.
    /// </summary>
    /// <param name="source">The query string containing one or more S-expression patterns.</param>
    /// <returns>The new query.</returns>
    public Query CreateQuery(string source)
    {
        return new(this, source);
    }

    /// <summary>
    /// Creates a new lookahead iterator.
    /// </summary>
    /// <param name="state">The parse state.</param>
    /// <returns>The new lookahead iterator.</returns>
    public LookaheadIterator CreateLookaheadIterator(ushort state)
    {
        return new(this, state);
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
            ts_language_delete(_self);
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? other)
    {
        return other is Language language && Equals(language);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Self.GetHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(Language? other)
    {
        return other is not null && Self == other.Self;
    }

    /// <inheritdoc/>
    public object Clone()
    {
        return Copy();
    }

    delegate IntPtr LanguageFunction();

    static string GetLibraryNameFromId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Language ID cannot be null or empty.", nameof(id));
        }

        return "tree-sitter-" + MapLanguageId(id);
    }

    static string GetFunctionNameFromId(string id)
    {
        return "tree_sitter_" + MapLanguageId(id).Replace('-', '_');
    }

    static string MapLanguageId(string id)
    {
        return id switch
        {
            "C++" => "cpp",
            "C#" => "c-sharp",
            _ => id.ToLowerInvariant(),
        };
    }

    static IntPtr LoadLanguage(string library, string function)
    {
        var handle = NativeLibrary.Load(library, Assembly.GetExecutingAssembly(), null);
        var pointer = NativeLibrary.GetExport(handle, function);
        return Marshal.GetDelegateForFunctionPointer<LanguageFunction>(pointer)();
    }

    /// <summary>
    /// Represents a symbol in the language.
    /// </summary>
    [DebuggerDisplay("{Name,nq}")]
    public class Symbol
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Symbol"/> class.
        /// </summary>
        /// <param name="id">The symbol ID.</param>
        /// <param name="name">The symbol name.</param>
        /// <param name="type">The symbol type.</param>
        public Symbol(ushort id, string name, SymbolType type)
        {
            Id = id;
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Gets the symbol's identifier.
        /// </summary>
        public ushort Id { get; }

        /// <summary>
        /// Gets the symbol's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the symbol's type.
        /// </summary>
        public SymbolType Type { get; }
    }

    /// <summary>
    /// Represents a supertype symbol in the language.
    /// </summary>
    public class SupertypeSymbol : Symbol
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SupertypeSymbol"/> class.
        /// </summary>
        /// <param name="symbol">The symbol base.</param>
        /// <param name="subtypeSymbols">A list of subtype symbols.</param>
        public SupertypeSymbol(Symbol symbol, List<Symbol> subtypeSymbols) : base(symbol.Id, symbol.Name, symbol.Type)
        {
            _subtypeSymbols = subtypeSymbols;
        }

        /// <summary>
        /// Gets a list of subtype symbols.
        /// </summary>
        public IReadOnlyList<Symbol> SubtypeSymbols => _subtypeSymbols;

        readonly List<Symbol> _subtypeSymbols;
    }

    /// <summary>
    /// Represents a field in the language.
    /// </summary>
    [DebuggerDisplay("{Name,nq}")]
    public class Field
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        /// <param name="id">The field ID.</param>
        /// <param name="name">The field name.</param>
        public Field(ushort id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// Gets the field's identifier.
        /// </summary>
        public ushort Id { get; }

        /// <summary>
        /// Gets the field's name.
        /// </summary>
        public string Name { get; }
    }

    readonly IntPtr _self;
    int _isDisposed;
    List<Symbol>? _symbols;
    List<SupertypeSymbol>? _supertypeSymbols;
    List<Field>? _fields;
}
