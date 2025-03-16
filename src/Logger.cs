//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

/// <summary>
/// Callback for log messages.
/// </summary>
/// <param name="logType">The message type.</param>
/// <param name="message">The message string.</param>
public delegate void Logger(LogType logType, string message);
