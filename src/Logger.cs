//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

namespace TreeSitter;

/// <summary>
/// Represents a callback for log messages.
/// </summary>
/// <param name="logType">The type of the log message.</param>
/// <param name="message">The log message string.</param>
public delegate void Logger(LogType logType, string message);
