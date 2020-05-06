using System;
using Mono.Cecil;
using StardewModdingAPI.Framework.ModLoading.Finders;
using StardewModdingAPI.Framework.ModLoading.Framework;

namespace StardewModdingAPI.Framework.ModLoading.Rewriters
{
    /// <summary>Rewrites all references to a type.</summary>
    internal class TypeReferenceRewriter : BaseTypeReferenceRewriter
    {
        /*********
        ** Fields
        *********/
        /// <summary>The full type name to which to find references.</summary>
        private readonly string FromTypeName;

        /// <summary>The new type to reference.</summary>
        private readonly Type ToType;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fromTypeFullName">The full type name to which to find references.</param>
        /// <param name="toType">The new type to reference.</param>
        /// <param name="shouldIgnore">A lambda which overrides a matched type.</param>
        public TypeReferenceRewriter(string fromTypeFullName, Type toType, Func<TypeReference, bool> shouldIgnore = null)
            : base(new TypeFinder(fromTypeFullName, InstructionHandleResult.None, shouldIgnore), $"{fromTypeFullName} type")
        {
            this.FromTypeName = fromTypeFullName;
            this.ToType = toType;
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Change a type reference if needed.</summary>
        /// <param name="module">The assembly module containing the instruction.</param>
        /// <param name="type">The type to replace if it matches.</param>
        /// <param name="set">Assign the new type reference.</param>
        protected override bool RewriteIfNeeded(ModuleDefinition module, TypeReference type, Action<TypeReference> set)
        {
            bool rewritten = false;

            // current type
            if (type.FullName == this.FromTypeName)
            {
                set(module.ImportReference(this.ToType));
                return true;
            }

            // recurse into generic arguments
            if (type is GenericInstanceType genericType)
            {
                for (int i = 0; i < genericType.GenericArguments.Count; i++)
                    rewritten |= this.RewriteIfNeeded(module, genericType.GenericArguments[i], typeRef => genericType.GenericArguments[i] = typeRef);
            }

            // recurse into generic parameters (e.g. constraints)
            for (int i = 0; i < type.GenericParameters.Count; i++)
                rewritten |= this.RewriteIfNeeded(module, type.GenericParameters[i], typeRef => type.GenericParameters[i] = new GenericParameter(typeRef));

            return rewritten;
        }
    }
}
