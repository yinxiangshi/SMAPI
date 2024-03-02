using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI.Framework.ModLoading.Framework;
using StardewValley;
using StardewValley.Tools;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member: This is internal code to support rewriters and shouldn't be called directly.

namespace StardewModdingAPI.Framework.ModLoading.Rewriters.StardewValley_1_6
{
    /// <summary>Maps Stardew Valley 1.5.6's <c>ToolFactory</c> methods to their newer form to avoid breaking older mods.</summary>
    /// <remarks>This is public to support SMAPI rewriting and should never be referenced directly by mods. See remarks on <see cref="ReplaceReferencesRewriter"/> for more info.</remarks>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = SuppressReasons.MatchesOriginal)]
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = SuppressReasons.UsedViaRewriting)]
    public class ToolFactoryFacade : IRewriteFacade
    {
        /*********
        ** Accessors
        *********/
        public const byte axe = 0;
        public const byte hoe = 1;
        public const byte fishingRod = 2;
        public const byte pickAxe = 3;
        public const byte wateringCan = 4;
        public const byte meleeWeapon = 5;
        public const byte slingshot = 6;


        /*********
        ** Public methods
        *********/
        public static Tool getToolFromDescription(byte index, int upgradeLevel)
        {
            Tool? t = null;
            switch (index)
            {
                case axe: t = new Axe(); break;
                case hoe: t = new Hoe(); break;
                case fishingRod: t = new FishingRod(); break;
                case pickAxe: t = new Pickaxe(); break;
                case wateringCan: t = new WateringCan(); break;
                case meleeWeapon: t = new MeleeWeapon("0"); break;
                case slingshot: t = new Slingshot(); break;
            }
            t.UpgradeLevel = upgradeLevel;
            return t;
        }


        /*********
        ** Private methods
        *********/
        private ToolFactoryFacade()
        {
            RewriteHelper.ThrowFakeConstructorCalled();
        }
    }
}
