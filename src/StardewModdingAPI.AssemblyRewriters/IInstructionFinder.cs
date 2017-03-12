using Mono.Cecil.Cil;

namespace StardewModdingAPI.AssemblyRewriters
{
    /// <summary>Finds CIL instructions considered incompatible.</summary>
    public interface IInstructionFinder
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the instruction finder matches.</summary>
        string NounPhrase { get; }


        /*********
        ** Methods
        *********/
        /// <summary>Get whether a CIL instruction matches.</summary>
        /// <param name="instruction">The IL instruction.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        bool IsMatch(Instruction instruction, bool platformChanged);
    }
}
