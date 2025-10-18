//
// Tests for .NET bindings for tree-sitter
// Adapted from official tree-sitter web bindings tests 'language.test.ts'
// Copyright (c) 2025 Marius Greuel
// Copyright (c) 2018-2024 Max Brunsfeld
// SPDX-License-Identifier: MIT
//

namespace TreeSitter.Tests;

[TestClass]
public class LanguageTests
{
    [TestMethod]
    public void ReturnsTheNameAndVersionOfTheLanguage()
    {
        //Assert.AreEqual("javascript", JavaScript.Name);
        Assert.AreEqual(15u, _javaScript.AbiVersion);
    }

    [TestMethod]
    public void ConvertsBetweenTheStringAndIntegerRepresentationsOfFields()
    {
        var nameId = _javaScript.GetFieldIdFromName("name");
        var bodyId = _javaScript.GetFieldIdFromName("body");
        Assert.IsTrue(nameId < _javaScript.Fields.Count);
        Assert.IsTrue(bodyId < _javaScript.Fields.Count);
        Assert.AreEqual("name", _javaScript.Fields[nameId].Name);
        Assert.AreEqual("body", _javaScript.Fields[bodyId].Name);
    }

    [TestMethod]
    public void HandlesInvalidFieldInputs()
    {
        Assert.AreEqual(0, _javaScript.GetFieldIdFromName("namezzz"));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _javaScript.Fields[-3]);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _javaScript.Fields[10000]);
    }

    [TestMethod]
    public void ConvertsBetweenTheStringAndIntegerRepresentationsOfANodeType()
    {
        var exportStatementId = _javaScript.GetSymbolIdFromName("export_statement", true);
        var starId = _javaScript.GetSymbolIdFromName("*", false);
        Assert.IsTrue(exportStatementId < _javaScript.Symbols.Count);
        Assert.IsTrue(starId < _javaScript.Symbols.Count);
        Assert.AreEqual("export_statement", _javaScript.Symbols[exportStatementId].Name);
        Assert.AreEqual("*", _javaScript.Symbols[starId].Name);
    }

    [TestMethod]
    public void HandlesInvalidSymbolInputs()
    {
        Assert.AreEqual(0, _javaScript.GetSymbolIdFromName("export_statement", false));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _javaScript.Symbols[-3]);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => _javaScript.Symbols[10000]);
    }

    [TestMethod]
    [Ignore("SupertypeSymbols not implemented")]
    public void GetsTheSupertypesAndSubtypesOfAParser()
    {
        Language rust = new("rust");
        var supertypes = rust.SupertypeSymbols;
        var names = supertypes.Select(supertype => supertype.Name).ToList();
        CollectionAssert.AreEqual(new[]
        {
            "_expression",
            "_literal",
            "_literal_pattern",
            "_pattern",
            "_type"
        }, names);

        foreach (var supertype in supertypes)
        {
            var name = supertype.Name;
            var subtypes = supertype.SubtypeSymbols;
            var subtypeNames = subtypes.Select(subtype => subtype.Name).Distinct().OrderBy(n => n).ToList();
            switch (name)
            {
                case "_literal":
                    CollectionAssert.AreEqual(new[]
                    {
                        "boolean_literal",
                        "char_literal",
                        "float_literal",
                        "integer_literal",
                        "raw_string_literal",
                        "string_literal"
                    }, subtypeNames);
                    break;
                case "_pattern":
                    CollectionAssert.AreEqual(new[]
                    {
                        "_",
                        "_literal_pattern",
                        "captured_pattern",
                        "const_block",
                        "identifier",
                        "macro_invocation",
                        "mut_pattern",
                        "or_pattern",
                        "range_pattern",
                        "ref_pattern",
                        "reference_pattern",
                        "remaining_field_pattern",
                        "scoped_identifier",
                        "slice_pattern",
                        "struct_pattern",
                        "tuple_pattern",
                        "tuple_struct_pattern"
                    }, subtypeNames);
                    break;
                case "_type":
                    CollectionAssert.AreEqual(new[]
                    {
                        "abstract_type",
                        "array_type",
                        "bounded_type",
                        "dynamic_type",
                        "function_type",
                        "generic_type",
                        "macro_invocation",
                        "metavariable",
                        "never_type",
                        "pointer_type",
                        "primitive_type",
                        "reference_type",
                        "removed_trait_bound",
                        "scoped_type_identifier",
                        "tuple_type",
                        "type_identifier",
                        "unit_type"
                    }, subtypeNames);
                    break;
            }
        }
    }

    readonly Language _javaScript = new("JavaScript");
}
