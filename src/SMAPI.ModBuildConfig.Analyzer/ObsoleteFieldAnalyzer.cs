using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace StardewModdingAPI.ModBuildConfig.Analyzer
{
    /// <summary>Detects references to a field which has been replaced.</summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ObsoleteFieldAnalyzer : DiagnosticAnalyzer
    {
        /*********
        ** Properties
        *********/
        /// <summary>Maps obsolete fields/properties to their non-obsolete equivalent.</summary>
        private readonly IDictionary<string, string> ReplacedFields = new Dictionary<string, string>
        {
            // Farmer
            ["StardewValley.Farmer::friendships"] = "friendshipData"
        };

        /// <summary>Describes the diagnostic rule covered by the analyzer.</summary>
        private readonly IDictionary<string, DiagnosticDescriptor> Rules = new Dictionary<string, DiagnosticDescriptor>
        {
            ["SMAPI003"] = new DiagnosticDescriptor(
                id: "SMAPI003",
                title: "Reference to obsolete field",
                messageFormat: "The '{0}' field is obsolete and should be replaced with '{1}'. See https://smapi.io/buildmsg/smapi003 for details.",
                category: "SMAPI.CommonErrors",
                defaultSeverity: DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                helpLinkUri: "https://smapi.io/buildmsg/smapi003"
            )
        };


        /*********
        ** Accessors
        *********/
        /// <summary>The descriptors for the diagnostics that this analyzer is capable of producing.</summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public ObsoleteFieldAnalyzer()
        {
            this.SupportedDiagnostics = ImmutableArray.CreateRange(this.Rules.Values);
        }

        /// <summary>Called once at session start to register actions in the analysis context.</summary>
        /// <param name="context">The analysis context.</param>
        public override void Initialize(AnalysisContext context)
        {
            // SMAPI003: avoid obsolete fields
            context.RegisterSyntaxNodeAction(
                this.AnalyzeObsoleteFields,
                SyntaxKind.SimpleMemberAccessExpression
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Analyse a syntax node and add a diagnostic message if it references an obsolete field.</summary>
        /// <param name="context">The analysis context.</param>
        private void AnalyzeObsoleteFields(SyntaxNodeAnalysisContext context)
        {
            try
            {
                // get reference info
                MemberAccessExpressionSyntax node = (MemberAccessExpressionSyntax)context.Node;
                ITypeSymbol declaringType = context.SemanticModel.GetTypeInfo(node.Expression).Type;
                string propertyName = node.Name.Identifier.Text;

                // suggest replacement
                for (ITypeSymbol type = declaringType; type != null; type = type.BaseType)
                {
                    if (this.ReplacedFields.TryGetValue($"{type}::{propertyName}", out string replacement))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(this.Rules["SMAPI003"], context.Node.GetLocation(), $"{type}.{propertyName}", replacement));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed processing expression: '{context.Node}'. Exception details: {ex.ToString().Replace('\r', ' ').Replace('\n', ' ')}");
            }
        }
    }
}
