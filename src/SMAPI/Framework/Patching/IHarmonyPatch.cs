using HarmonyLib;

namespace StardewModdingAPI.Framework.Patching
{
    /// <summary>A Harmony patch to apply.</summary>
    internal interface IHarmonyPatch
    {
        /*********
        ** Methods
        *********/
        /// <summary>Apply the Harmony patch.</summary>
        /// <param name="harmony">The Harmony instance.</param>
        void Apply(Harmony harmony);
    }
}
