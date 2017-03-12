using StardewModdingAPI.AssemblyRewriters.Framework;

namespace StardewModdingAPI.AssemblyRewriters.Finders
{
    /// <summary>Base class for a type reference finder.</summary>
    public class GenericTypeFinder : BaseTypeFinder
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the instruction finder matches.</summary>
        public override string NounPhrase { get; }

        /// <summary>The full type name to match.</summary>
        public override string FullTypeName { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fullTypeName">The full type name to match.</param>
        /// <param name="nounPhrase">A brief noun phrase indicating what the instruction finder matches.</param>
        public GenericTypeFinder(string fullTypeName, string nounPhrase)
        {
            this.FullTypeName = fullTypeName;
            this.NounPhrase = nounPhrase;
        }
    }
}
