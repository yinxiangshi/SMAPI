using Mono.Cecil;
using Mono.Cecil.Cil;
using StardewModdingAPI.AssemblyRewriters.Framework;

namespace StardewModdingAPI.AssemblyRewriters.Finders
{
    /// <summary>Finds CIL instructions that reference a given event.</summary>
    public sealed class EventFinder : BaseMethodFinder
    {
        /*********
        ** Properties
        *********/
        /// <summary>The full type name for which to find references.</summary>
        private readonly string FullTypeName;

        /// <summary>The event name for which to find references.</summary>
        private readonly string EventName;


        /*********
        ** Accessors
        *********/
        /// <summary>A brief noun phrase indicating what the instruction finder matches.</summary>
        public override string NounPhrase { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="fullTypeName">The full type name for which to find references.</param>
        /// <param name="eventName">The event name for which to find references.</param>
        public EventFinder(string fullTypeName, string eventName)
        {
            this.FullTypeName = fullTypeName;
            this.EventName = eventName;
            this.NounPhrase = $"obsolete {fullTypeName}.{eventName} event";
        }


        /*********
        ** Protected methods
        *********/
        /// <summary>Get whether a method reference should be rewritten.</summary>
        /// <param name="instruction">The IL instruction.</param>
        /// <param name="methodRef">The method reference.</param>
        /// <param name="platformChanged">Whether the mod was compiled on a different platform.</param>
        protected override bool IsMatch(Instruction instruction, MethodReference methodRef, bool platformChanged)
        {
            return methodRef.DeclaringType.FullName == this.FullTypeName
                && (methodRef.Name == "add_" + this.EventName || methodRef.Name == "remove_" + this.EventName);
        }
    }
}
