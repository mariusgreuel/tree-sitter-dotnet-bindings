//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

internal enum QueryPredicateStepType
{
    Done,
    Capture,
    String,
}
