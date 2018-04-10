using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using SMAPI.ModBuildConfig.Analyzer.Tests.Framework;
using StardewModdingAPI.ModBuildConfig.Analyzer;

namespace SMAPI.ModBuildConfig.Analyzer.Tests
{
    /// <summary>Unit tests for the C# analyzers.</summary>
    [TestFixture]
    public class UnitTests : DiagnosticVerifier
    {
        /*********
        ** Properties
        *********/
        /// <summary>Sample C# code which contains a simplified representation of Stardew Valley's <c>Netcode</c> types, and sample mod code with a {{test-code}} placeholder for the code being tested.</summary>
        const string SampleProgram = @"
            using System;
            using StardewValley;
            using Netcode;
            using SObject = StardewValley.Object;

            namespace SampleMod
            {
                class ModEntry
                {
                    public void Entry()
                    {
                        {{test-code}}
                    }
                }
            }
        ";

        /// <summary>The line number where the unit tested code is injected into <see cref="SampleProgram"/>.</summary>
        private const int SampleCodeLine = 13;

        /// <summary>The column number where the unit tested code is injected into <see cref="SampleProgram"/>.</summary>
        private const int SampleCodeColumn = 25;


        /*********
        ** Unit tests
        *********/
        /// <summary>Test that no diagnostics are raised for an empty code block.</summary>
        [TestCase]
        public void EmptyCode_HasNoDiagnostics()
        {
            // arrange
            string test = @"";

            // assert
            this.VerifyCSharpDiagnostic(test);
        }

        /// <summary>Test that the expected diagnostic message is raised for implicit net field comparisons.</summary>
        /// <param name="codeText">The code line to test.</param>
        /// <param name="column">The column within the code line where the diagnostic message should be reported.</param>
        /// <param name="expression">The expression which should be reported.</param>
        /// <param name="fromType">The source type name which should be reported.</param>
        /// <param name="toType">The target type name which should be reported.</param>
        [TestCase("Item item = null; if (item.netIntField < 42);", 22, "item.netIntField", "NetInt", "int")]
        [TestCase("Item item = null; if (item.netIntField <= 42);", 22, "item.netIntField", "NetInt", "int")]
        [TestCase("Item item = null; if (item.netIntField > 42);", 22, "item.netIntField", "NetInt", "int")]
        [TestCase("Item item = null; if (item.netIntField >= 42);", 22, "item.netIntField", "NetInt", "int")]
        [TestCase("Item item = null; if (item.netIntField == 42);", 22, "item.netIntField", "NetInt", "int")]
        [TestCase("Item item = null; if (item.netIntField != 42);", 22, "item.netIntField", "NetInt", "int")]
        [TestCase("Item item = null; if (item?.netIntField != 42);", 22, "item?.netIntField", "NetInt", "int")]
        [TestCase("Item item = null; if (item?.netIntField != null);", 22, "item?.netIntField", "NetInt", "object")]
        [TestCase("Item item = null; if (item.netIntProperty < 42);", 22, "item.netIntProperty", "NetInt", "int")]
        [TestCase("Item item = null; if (item.netIntProperty <= 42);", 22, "item.netIntProperty", "NetInt", "int")]
        [TestCase("Item item = null; if (item.netIntProperty > 42);", 22, "item.netIntProperty", "NetInt", "int")]
        [TestCase("Item item = null; if (item.netIntProperty >= 42);", 22, "item.netIntProperty", "NetInt", "int")]
        [TestCase("Item item = null; if (item.netIntProperty == 42);", 22, "item.netIntProperty", "NetInt", "int")]
        [TestCase("Item item = null; if (item.netIntProperty != 42);", 22, "item.netIntProperty", "NetInt", "int")]
        [TestCase("Item item = null; if (item?.netIntProperty != 42);", 22, "item?.netIntProperty", "NetInt", "int")]
        [TestCase("Item item = null; if (item?.netIntProperty != null);", 22, "item?.netIntProperty", "NetInt", "object")]
        [TestCase("Item item = null; if (item.netRefField == null);", 22, "item.netRefField", "NetRef", "object")]
        [TestCase("Item item = null; if (item.netRefField != null);", 22, "item.netRefField", "NetRef", "object")]
        [TestCase("Item item = null; if (item.netRefProperty == null);", 22, "item.netRefProperty", "NetRef", "object")]
        [TestCase("Item item = null; if (item.netRefProperty != null);", 22, "item.netRefProperty", "NetRef", "object")]
        [TestCase("SObject obj = null; if (obj.netIntField != 42);", 24, "obj.netIntField", "NetInt", "int")] // ↓ same as above, but inherited from base class
        [TestCase("SObject obj = null; if (obj.netIntProperty != 42);", 24, "obj.netIntProperty", "NetInt", "int")]
        [TestCase("SObject obj = null; if (obj.netRefField == null);", 24, "obj.netRefField", "NetRef", "object")]
        [TestCase("SObject obj = null; if (obj.netRefField != null);", 24, "obj.netRefField", "NetRef", "object")]
        [TestCase("SObject obj = null; if (obj.netRefProperty == null);", 24, "obj.netRefProperty", "NetRef", "object")]
        [TestCase("SObject obj = null; if (obj.netRefProperty != null);", 24, "obj.netRefProperty", "NetRef", "object")]
        public void AvoidImplicitNetFieldComparisons_RaisesDiagnostic(string codeText, int column, string expression, string fromType, string toType)
        {
            // arrange
            string code = UnitTests.SampleProgram.Replace("{{test-code}}", codeText);
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = "SMAPI001",
                Message = $"This implicitly converts '{expression}' from {fromType} to {toType}, but {fromType} has unintuitive implicit conversion rules. Consider comparing against the actual value instead to avoid bugs. See https://smapi.io/buildmsg/SMAPI001 for details.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", UnitTests.SampleCodeLine, UnitTests.SampleCodeColumn + column) }
            };

            // assert
            this.VerifyCSharpDiagnostic(code, expected);
        }

        /// <summary>Test that the expected diagnostic message is raised for avoidable net field references.</summary>
        /// <param name="codeText">The code line to test.</param>
        /// <param name="column">The column within the code line where the diagnostic message should be reported.</param>
        /// <param name="expression">The expression which should be reported.</param>
        /// <param name="netType">The net type name which should be reported.</param>
        /// <param name="suggestedProperty">The suggested property name which should be reported.</param>
        [TestCase("Item item = null; int category = item.category;", 33, "item.category", "NetInt", "Category")]
        [TestCase("Item item = null; int category = (item).category;", 33, "(item).category", "NetInt", "Category")]
        [TestCase("Item item = null; int category = ((Item)item).category;", 33, "((Item)item).category", "NetInt", "Category")]
        [TestCase("SObject obj = null; int category = obj.category;", 35, "obj.category", "NetInt", "Category")]
        public void AvoidNetFields_RaisesDiagnostic(string codeText, int column, string expression, string netType, string suggestedProperty)
        {
            // arrange
            string code = UnitTests.SampleProgram.Replace("{{test-code}}", codeText);
            DiagnosticResult expected = new DiagnosticResult
            {
                Id = "SMAPI002",
                Message = $"'{expression}' is a {netType} field; consider using the {suggestedProperty} property instead. See https://smapi.io/buildmsg/SMAPI002 for details.",
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", UnitTests.SampleCodeLine, UnitTests.SampleCodeColumn + column) }
            };

            // assert
            this.VerifyCSharpDiagnostic(code, expected);
        }


        /*********
        ** Helpers
        *********/
        /// <summary>Get the analyzer being tested.</summary>
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new NetFieldAnalyzer();
        }
    }
}
