using System.Diagnostics.CodeAnalysis;
using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.AssemblyRewriters.Framework;
using StardewValley;

namespace StardewModdingAPI.AssemblyRewriters.Finders
{
    /// <summary>Finds CIL instructions that reference the former <c>Game1.borderFont</c> field, which was removed in Stardew Valley 1.2.3–1.2.6.</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "This class is not meant to be used directly, and is deliberately named to make it easier to know what it changes at a glance.")]
    public class Game1_borderFont_FieldFinder : BaseFieldFinder
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the instruction finder matches.</summary>
        public override string NounPhrase { get; } = $"obsolete {nameof(Game1)}.borderFont field";


        /*********
        ** Protected methods
        *********/
        /// <summary>Get whether a field reference should be rewritten.</summary>
        /// <param name="instruction">The IL instruction.</param>
        /// <param name="fieldRef">The field reference.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        protected override bool IsMatch(Instruction instruction, FieldReference fieldRef, bool platformChanged)
        {
            return
                this.IsStaticField(instruction)
                && fieldRef.DeclaringType.FullName == typeof(Game1).FullName
                && fieldRef.Name == "borderFont";
        }
    }
}
