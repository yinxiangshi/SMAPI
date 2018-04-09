using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace StardewModdingAPI.ModBuildConfig.Analyzer
{
    /// <summary>Detects implicit conversion from Stardew Valley's <c>Netcode</c> types. These have very unintuitive implicit conversion rules, so mod authors should always explicitly convert the type with appropriate null checks.</summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ImplicitNetFieldCastAnalyzer : DiagnosticAnalyzer
    {
        /*********
        ** Properties
        *********/
        /// <summary>The namespace for Stardew Valley's <c>Netcode</c> types.</summary>
        private const string NetcodeNamespace = "Netcode";

        /// <summary>Describes the diagnostic rule covered by the analyzer.</summary>
        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: "SMAPI001",
            title: "Netcode types shouldn't be implicitly converted",
            messageFormat: "This implicitly converts '{0}' from {1} to {2}, but {1} has unintuitive implicit conversion rules. Consider comparing against the actual value instead to avoid bugs. See https://smapi.io/buildmsg/SMAPI001 for details.",
            category: "SMAPI.CommonErrors",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "",
            helpLinkUri: "https://smapi.io/buildmsg/SMAPI001"
        );


        /*********
        ** Accessors
        *********/
        /// <summary>The descriptors for the diagnostics that this analyzer is capable of producing.</summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public ImplicitNetFieldCastAnalyzer()
        {
            this.SupportedDiagnostics = ImmutableArray.Create(ImplicitNetFieldCastAnalyzer.Rule);
        }

        /// <summary>Called once at session start to register actions in the analysis context.</summary>
        /// <param name="context">The analysis context.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(
                this.Analyse,
                SyntaxKind.EqualsExpression,
                SyntaxKind.NotEqualsExpression,
                SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression,
                SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression
            );
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Analyse a syntax node and add a diagnostic message if applicable.</summary>
        /// <param name="context">The analysis context.</param>
        private void Analyse(SyntaxNodeAnalysisContext context)
        {
            try
            {
                BinaryExpressionSyntax node = (BinaryExpressionSyntax)context.Node;
                bool leftHasWarning = this.Analyze(context, node.Left);
                if (!leftHasWarning)
                    this.Analyze(context, node.Right);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed processing expression: '{context.Node}'. Exception details: {ex.ToString().Replace('\r', ' ').Replace('\n', ' ')}");
            }
        }

        /// <summary>Analyse one operand in a binary expression (like <c>a</c> and <c>b</c> in <c>a == b</c>) and add a diagnostic message if applicable.</summary>
        /// <param name="context">The analysis context.</param>
        /// <param name="operand">The operand expression.</param>
        /// <returns>Returns whether a diagnostic message was raised.</returns>
        private bool Analyze(SyntaxNodeAnalysisContext context, ExpressionSyntax operand)
        {
            const string netcodeNamespace = ImplicitNetFieldCastAnalyzer.NetcodeNamespace;

            TypeInfo operandType = context.SemanticModel.GetTypeInfo(operand);
            string fromNamespace = operandType.Type?.ContainingNamespace?.Name;
            string toNamespace = operandType.ConvertedType?.ContainingNamespace?.Name;
            if (fromNamespace == netcodeNamespace && fromNamespace != toNamespace && toNamespace != null)
            {
                string fromTypeName = operandType.Type.Name;
                string toTypeName = operandType.ConvertedType.Name;
                context.ReportDiagnostic(Diagnostic.Create(ImplicitNetFieldCastAnalyzer.Rule, context.Node.GetLocation(), operand, fromTypeName, toTypeName));
                return true;
            }

            return false;
        }
    }
}
