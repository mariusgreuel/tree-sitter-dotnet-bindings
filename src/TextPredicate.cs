//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

internal class TextPredicate
{
    internal TextPredicate(Predicate<IEnumerable<QueryCapture>> predicate)
    {
        Predicate = predicate;
    }

    internal Predicate<IEnumerable<QueryCapture>> Predicate { get; }
}
