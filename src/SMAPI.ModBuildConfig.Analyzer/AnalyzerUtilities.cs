using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StardewModdingAPI.ModBuildConfig.Analyzer
{
    /// <summary>Provides generic utilities for SMAPI's Roslyn analyzers.</summary>
    internal static class AnalyzerUtilities
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get the metadata for a member access expression.</summary>
        /// <param name="node">The member access expression.</param>
        /// <param name="semanticModel">provides methods for asking semantic questions about syntax nodes.</param>
        /// <param name="declaringType">The object type which has the member.</param>
        /// <param name="memberType">The type of the accessed member.</param>
        /// <param name="memberName">The name of the accessed member.</param>
        /// <returns>Returns true if the node is a member access expression, else false.</returns>
        public static bool GetMemberInfo(SyntaxNode node, SemanticModel semanticModel, out ITypeSymbol declaringType, out TypeInfo memberType, out string memberName)
        {
            // simple access
            if (node is MemberAccessExpressionSyntax memberAccess)
            {
                declaringType = semanticModel.GetTypeInfo(memberAccess.Expression).Type;
                memberType = semanticModel.GetTypeInfo(node);
                memberName = memberAccess.Name.Identifier.Text;
                return true;
            }

            // conditional access
            if (node is ConditionalAccessExpressionSyntax conditionalAccess && conditionalAccess.WhenNotNull is MemberBindingExpressionSyntax conditionalBinding)
            {
                declaringType = semanticModel.GetTypeInfo(conditionalAccess.Expression).Type;
                memberType = semanticModel.GetTypeInfo(node);
                memberName = conditionalBinding.Name.Identifier.Text;
                return true;
            }

            // invalid
            declaringType = null;
            memberType = default(TypeInfo);
            memberName = null;
            return false;
        }
    }
}
