//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

internal static class ListHelpers
{
    /// <summary>
    /// Determines whether some elements in the <see cref="List{T}"/> match
    /// the conditions defined by the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type of the list.</typeparam>
    /// <param name="list">The list to operate on.</param>
    /// <param name="match">The <see cref="Predicate{T}"/> delegate that defines the conditions to check against the elements.</param>
    /// <returns>
    /// <see langword="true"/> if some elements in the <see cref="List{T}"/> match the conditions
    /// defined by the specified predicate; otherwise, <see langword="false"/>. If the list has no elements,
    /// the return value is <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="match"/> is <see langword="null"/>.
    /// </exception>
    internal static bool TrueForSome<T>(this List<T> list, Predicate<T> match)
    {
        if (match == null)
        {
            throw new ArgumentNullException(nameof(match), "The match predicate cannot be null.");
        }

        for (int i = 0; i < list.Count; i++)
        {
            if (match(list[i]))
            {
                return true;
            }
        }

        return false;
    }
}
