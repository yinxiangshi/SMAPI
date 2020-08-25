using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.Framework.ModLoading.Framework;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters
{
    /// <summary>Rewrites references to methods which only broke because the definition has new optional parameters.</summary>
    internal class MethodWithMissingOptionalParameterRewriter : BaseInstructionHandler
    {
        /*********
        ** Fields
        *********/
        /// <summary>The assembly names to which to rewrite broken references.</summary>
        private readonly HashSet<string> RewriteReferencesToAssemblies;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="rewriteReferencesToAssemblies">The assembly names to which to rewrite broken references.</param>
        public MethodWithMissingOptionalParameterRewriter(string[] rewriteReferencesToAssemblies)
            : base(defaultPhrase: "methods with missing parameters") // ignored since we specify phrases
        {
            this.RewriteReferencesToAssemblies = new HashSet<string>(rewriteReferencesToAssemblies);
        }

        /// <summary>Rewrite a CIL instruction reference if needed.</summary>
        /// <param name="module">The assembly module containing the instruction.</param>
        /// <param name="cil">The CIL processor.</param>
        /// <param name="instruction">The CIL instruction to handle.</param>
        /// <param name="replaceWith">Replaces the CIL instruction with a new one.</param>
        /// <returns>Returns whether the instruction was changed.</returns>
        public override bool Handle(ModuleDefinition module, ILProcessor cil, Instruction instruction, Action<Instruction> replaceWith)
        {
            // get method ref
            MethodReference methodRef = RewriteHelper.AsMethodReference(instruction);
            if (methodRef == null || !this.ShouldValidate(methodRef.DeclaringType))
                return false;

            // skip if not broken
            if (methodRef.Resolve() != null)
                return false;

            // get type
            var type = methodRef.DeclaringType.Resolve();
            if (type == null)
                return false;

            // get method definition
            MethodDefinition method = null;
            foreach (var match in type.Methods.Where(p => p.Name == methodRef.Name))
            {
                // reference matches initial parameters of definition
                if (methodRef.Parameters.Count >= match.Parameters.Count || !this.InitialParametersMatch(methodRef, match))
                    continue;

                // all remaining parameters in definition are optional
                if (!match.Parameters.Skip(methodRef.Parameters.Count).All(p => p.IsOptional))
                    continue;

                method = match;
                break;
            }
            if (method == null)
                return false;

            // add extra parameters
            foreach (ParameterDefinition parameter in method.Parameters.Skip(methodRef.Parameters.Count))
            {
                methodRef.Parameters.Add(new ParameterDefinition(
                    name: parameter.Name,
                    attributes: parameter.Attributes,
                    parameterType: module.ImportReference(parameter.ParameterType)
                ));
            }

            this.Phrases.Add($"{methodRef.DeclaringType.Name}.{methodRef.Name} (added missing optional parameters)");
            return this.MarkRewritten();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Whether references to the given type should be validated.</summary>
        /// <param name="type">The type reference.</param>
        private bool ShouldValidate(TypeReference type)
        {
            return type != null && this.RewriteReferencesToAssemblies.Contains(type.Scope.Name);
        }

        /// <summary>Get whether every parameter in the method reference matches the exact order and type of the parameters in the method definition. This ignores extra parameters in the definition.</summary>
        /// <param name="methodRef">The method reference whose parameters to check.</param>
        /// <param name="method">The method definition whose parameters to check against.</param>
        private bool InitialParametersMatch(MethodReference methodRef, MethodDefinition method)
        {
            if (methodRef.Parameters.Count > method.Parameters.Count)
                return false;

            for (int i = 0; i < methodRef.Parameters.Count; i++)
            {
                if (!RewriteHelper.IsSameType(methodRef.Parameters[i].ParameterType, method.Parameters[i].ParameterType))
                    return false;
            }

            return true;
        }
    }
}
