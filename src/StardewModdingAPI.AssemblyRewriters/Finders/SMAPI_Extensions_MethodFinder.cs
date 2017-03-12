using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.AssemblyRewriters.Framework;

namespace StardewModdingAPI.AssemblyRewriters.Finders
{
    /// <summary>Matches CIL instructions that reference the former <c>StardewModdingAPI.Extensions</c> class, which was removed in SMAPI 1.9.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "This class is not meant to be used directly, and is deliberately named to make it easier to know what it changes at a glance.")]
    public class SMAPI_Extensions_MethodFinder : BaseMethodFinder
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the instruction finder matches.</summary>
        public override string NounPhrase { get; } = "obsolete StardewModdingAPI.Extensions API";


        /*********
        ** Protected methods
        *********/
        /// <summary>Get whether a method reference should be rewritten.</summary>
        /// <param name="instruction">The IL instruction.</param>
        /// <param name="methodRef">The method reference.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        protected override bool IsMatch(Instruction instruction, MethodReference methodRef, bool platformChanged)
        {
            return methodRef.DeclaringType.FullName == "StardewModdingAPI.Extensions";
        }
    }
}
