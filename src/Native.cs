//
// .NET bindings for tree-sitter
// Copyright (c) 2025 Marius Greuel
// SPDX-License-Identifier: MIT
// https://github.com/mariusgreuel/tree-sitter-dotnet-bindings
//

using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace TreeSitter;

#pragma warning disable IDE1006 // Naming Styles

internal static class Native
{
    internal static partial class Libraries
    {
        internal const string TreeSitter = "tree-sitter";
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void TSLogCallback(IntPtr payload, LogType logType, [MarshalAs(UnmanagedType.LPStr)] string buffer);

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSPoint
    {
        internal uint row;
        internal uint column;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSRange
    {
        internal TSPoint start_point;
        internal TSPoint end_point;
        internal uint start_byte;
        internal uint end_byte;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSInput
    {
        internal IntPtr payload;
        internal IntPtr read;
        internal InputEncoding encoding;
        internal IntPtr decode;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSParseState
    {
        internal IntPtr payload;
        internal uint current_byte_offset;
        [MarshalAs(UnmanagedType.I1)]
        internal bool has_error;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSParseOptions
    {
        internal IntPtr payload;
        internal IntPtr progress_callback;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSLogger
    {
        internal IntPtr payload;
        internal TSLogCallback log;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSInputEdit
    {
        internal uint start_byte;
        internal uint old_end_byte;
        internal uint new_end_byte;
        internal TSPoint start_point;
        internal TSPoint old_end_point;
        internal TSPoint new_end_point;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSNode
    {
        internal uint context0;
        internal uint context1;
        internal uint context2;
        internal uint context3;
        internal IntPtr id;
        internal IntPtr tree;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSTreeCursor
    {
        internal IntPtr tree;
        internal IntPtr id;
        internal uint context0;
        internal uint context1;
        internal uint context2;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSQueryCapture
    {
        internal TSNode node;
        internal uint index;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSQueryMatch
    {
        internal uint id;
        internal ushort pattern_index;
        internal ushort capture_count;
        internal IntPtr captures;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSQueryPredicateStep
    {
        internal QueryPredicateStepType type;
        internal uint value_id;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSQueryCursorState
    {
        internal IntPtr payload;
        internal uint current_byte_offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TSQueryCursorOptions
    {
        internal IntPtr payload;
        internal IntPtr progress_callback;
    }

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_parser_new();

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_parser_delete(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_parser_language(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_parser_set_language(IntPtr self, IntPtr language);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_parser_set_included_ranges(IntPtr self, [MarshalAs(UnmanagedType.LPArray)] TSRange[] ranges, uint count);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_parser_included_ranges(IntPtr self, out uint count);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_parser_parse_string_encoding(IntPtr self, IntPtr old_tree, [MarshalAs(UnmanagedType.LPWStr)] string text, uint length, InputEncoding encoding);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_parser_reset(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_parser_set_logger(IntPtr self, TSLogger logger);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_tree_copy(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_tree_delete(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_tree_root_node(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_tree_root_node_with_offset(IntPtr self, uint offset_bytes, TSPoint offset_extent);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_tree_language(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_tree_included_ranges(IntPtr self, out uint length);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_tree_edit(IntPtr self, ref TSInputEdit edit);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_tree_get_changed_ranges(IntPtr old_tree, IntPtr new_tree, out uint length);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_tree_print_dot_graph(IntPtr self, IntPtr file_descriptor);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_node_type(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ushort ts_node_symbol(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_node_language(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_node_grammar_type(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ushort ts_node_grammar_symbol(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_node_start_byte(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSPoint ts_node_start_point(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_node_end_byte(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSPoint ts_node_end_point(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_node_string(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_node_is_null(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_node_is_named(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_node_is_missing(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_node_is_extra(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_node_has_changes(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_node_has_error(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_node_is_error(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ushort ts_node_parse_state(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ushort ts_node_next_parse_state(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_parent(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_child_with_descendant(TSNode self, TSNode descendant);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_child(TSNode self, uint child_index);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_node_field_name_for_child(TSNode self, uint child_index);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_node_field_name_for_named_child(TSNode self, uint named_child_index);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_node_child_count(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_named_child(TSNode self, uint child_index);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_node_named_child_count(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_child_by_field_name(TSNode self, byte[] name, uint name_length);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_child_by_field_id(TSNode self, ushort field_id);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_next_sibling(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_prev_sibling(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_next_named_sibling(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_prev_named_sibling(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_first_child_for_byte(TSNode self, uint byte_offset);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_first_named_child_for_byte(TSNode self, uint byte_offset);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_node_descendant_count(TSNode self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_descendant_for_byte_range(TSNode self, uint start, uint end);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_descendant_for_point_range(TSNode self, TSPoint start, TSPoint end);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_named_descendant_for_byte_range(TSNode self, uint start, uint end);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_node_named_descendant_for_point_range(TSNode self, TSPoint start, TSPoint end);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_node_edit(TSNode self, ref TSInputEdit edit);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_node_eq(TSNode self, TSNode other);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSTreeCursor ts_tree_cursor_new(TSNode node);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_tree_cursor_delete(ref TSTreeCursor self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_tree_cursor_reset(ref TSTreeCursor self, TSNode node);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_tree_cursor_reset_to(ref TSTreeCursor dst, ref TSTreeCursor src);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSNode ts_tree_cursor_current_node(ref TSTreeCursor self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_tree_cursor_current_field_name(ref TSTreeCursor self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ushort ts_tree_cursor_current_field_id(ref TSTreeCursor self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_tree_cursor_goto_parent(ref TSTreeCursor self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_tree_cursor_goto_next_sibling(ref TSTreeCursor self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_tree_cursor_goto_previous_sibling(ref TSTreeCursor self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_tree_cursor_goto_first_child(ref TSTreeCursor self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_tree_cursor_goto_last_child(ref TSTreeCursor self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern void ts_tree_cursor_goto_descendant(ref TSTreeCursor self, uint goal_descendant_index);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_tree_cursor_current_descendant_index(ref TSTreeCursor self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_tree_cursor_current_depth(ref TSTreeCursor self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern long ts_tree_cursor_goto_first_child_for_byte(ref TSTreeCursor self, uint goal_byte);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern long ts_tree_cursor_goto_first_child_for_point(ref TSTreeCursor self, TSPoint goal_point);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern TSTreeCursor ts_tree_cursor_copy(ref TSTreeCursor self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_query_new(IntPtr language, byte[] source, uint source_len, out uint error_offset, out QueryError error_type);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_query_delete(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_query_pattern_count(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_query_capture_count(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_query_string_count(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_query_start_byte_for_pattern(IntPtr self, uint pattern_index);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_query_end_byte_for_pattern(IntPtr self, uint pattern_index);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_query_predicates_for_pattern(IntPtr self, uint pattern_index, out uint step_count);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_query_is_pattern_rooted(IntPtr self, uint pattern_index);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_query_is_pattern_non_local(IntPtr self, uint pattern_index);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_query_is_pattern_guaranteed_at_step(IntPtr self, uint byte_offset);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_query_capture_name_for_id(IntPtr self, uint index, out uint length);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern Quantifier ts_query_capture_quantifier_for_id(IntPtr self, uint pattern_index, uint capture_index);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_query_string_value_for_id(IntPtr self, uint index, out uint length);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_query_disable_capture(IntPtr self, byte[] name, uint length);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_query_disable_pattern(IntPtr self, uint pattern_index);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_query_cursor_new();

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_query_cursor_delete(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_query_cursor_exec(IntPtr self, IntPtr query, TSNode node);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_query_cursor_exec_with_options(IntPtr self, IntPtr query, TSNode node, ref TSQueryCursorOptions query_options);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_query_cursor_did_exceed_match_limit(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_query_cursor_match_limit(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_query_cursor_set_match_limit(IntPtr self, uint limit);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_query_cursor_set_byte_range(IntPtr self, uint start_byte, uint end_byte);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_query_cursor_set_point_range(IntPtr self, TSPoint start_point, TSPoint end_point);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_query_cursor_next_match(IntPtr self, out TSQueryMatch match);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_query_cursor_remove_match(IntPtr self, uint match_id);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_query_cursor_next_capture(IntPtr self, out TSQueryMatch match, out uint capture_index);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_language_copy(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_language_delete(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_language_symbol_count(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_language_state_count(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ushort ts_language_symbol_for_name(IntPtr self, byte[] name, uint name_length, bool is_named);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_language_field_count(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_language_field_name_for_id(IntPtr self, ushort id);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ushort ts_language_field_id_for_name(IntPtr self, byte[] name, uint name_length);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_language_supertypes(IntPtr self, out uint length);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_language_subtypes(IntPtr self, ushort supertype, out uint length);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_language_symbol_name(IntPtr self, ushort symbol);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern SymbolType ts_language_symbol_type(IntPtr self, ushort symbol);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern uint ts_language_abi_version(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ushort ts_language_next_state(IntPtr self, ushort state, ushort symbol);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_language_name(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_lookahead_iterator_new(IntPtr language, ushort state);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern void ts_lookahead_iterator_delete(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_lookahead_iterator_reset_state(IntPtr self, ushort state);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_lookahead_iterator_reset(IntPtr self, IntPtr language, ushort state);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_lookahead_iterator_language(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    internal static extern bool ts_lookahead_iterator_next(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern ushort ts_lookahead_iterator_current_symbol(IntPtr self);

    [DllImport(Libraries.TreeSitter, CallingConvention = CallingConvention.Cdecl)]
    internal static extern IntPtr ts_lookahead_iterator_current_symbol_name(IntPtr self);

    internal static void ts_free(IntPtr ptr)
    {
        s_ts_current_free(ptr);
    }

    internal static string? MarshalString(IntPtr ptr)
    {
        return Marshal.PtrToStringAnsi(ptr);
    }

    internal static string? MarshalString(IntPtr ptr, int length)
    {
        return Marshal.PtrToStringAnsi(ptr, length);
    }

    internal static T MarshalArrayItem<T>(IntPtr array, int index) where T : struct
    {
        return Marshal.PtrToStructure<T>(array + index * Marshal.SizeOf(typeof(T)));
    }

    internal static byte[] ToUTF8(string text, out uint length)
    {
        var buffer = Encoding.UTF8.GetBytes(text);
        length = (uint)buffer.Length;
        return buffer;
    }

    internal static uint IntToUInt(int value)
    {
        return value >= 0 && value < int.MaxValue ? (uint)value : uint.MaxValue;
    }

    internal static int UIntToInt(uint value)
    {
        return value < int.MaxValue ? (int)value : int.MaxValue;
    }

    internal static int ByteToIndex(uint value)
    {
        return value < int.MaxValue ? (int)(value / 2) : int.MaxValue;
    }

    internal static uint IndexToByte(int value)
    {
        return value >= 0 && value < int.MaxValue ? (uint)value * 2 : uint.MaxValue;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void FreeFunction(IntPtr ptr);

    static FreeFunction GetFreeFunction()
    {
        var handle = NativeLibrary.Load(Libraries.TreeSitter, Assembly.GetExecutingAssembly(), null);
        var ts_current_free = NativeLibrary.GetExport(handle, "ts_current_free");
        return Marshal.GetDelegateForFunctionPointer<FreeFunction>(Marshal.ReadIntPtr(ts_current_free));
    }

    static readonly FreeFunction s_ts_current_free = GetFreeFunction();
}
