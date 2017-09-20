using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.ModLoading;
using StardewModdingAPI.Framework.ModLoading.Finders;
using StardewModdingAPI.Framework.ModLoading.Rewriters;
using StardewModdingAPI.Framework.ModLoading.Rewriters.Wrappers;
using StardewValley;

namespace StardewModdingAPI.Metadata
{
    /// <summary>Provides CIL instruction handlers which rewrite mods for compatibility and throw exceptions for incompatible code.</summary>
    internal class InstructionMetadata
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Get rewriters which detect or fix incompatible CIL instructions in mod assemblies.</summary>
        public IEnumerable<IInstructionRewriter> GetRewriters()
        {
            return new IInstructionRewriter[]
            {
                /****
                ** Finders throw an exception when incompatible code is found.
                ****/
                // changes in Stardew Valley 1.2 (with no rewriters)
                new FieldFinder("StardewValley.Item", "set_Name"),

                // APIs removed in SMAPI 1.9
                new TypeFinder("StardewModdingAPI.Advanced.ConfigFile"),
                new TypeFinder("StardewModdingAPI.Advanced.IConfigFile"),
                new TypeFinder("StardewModdingAPI.Entities.SPlayer"),
                new TypeFinder("StardewModdingAPI.Extensions"),
                new TypeFinder("StardewModdingAPI.Inheritance.SGame"),
                new TypeFinder("StardewModdingAPI.Inheritance.SObject"),
                new TypeFinder("StardewModdingAPI.LogWriter"),
                new TypeFinder("StardewModdingAPI.Manifest"),
                new TypeFinder("StardewModdingAPI.Version"),
                new EventFinder("StardewModdingAPI.Events.GraphicsEvents", "DrawDebug"),
                new EventFinder("StardewModdingAPI.Events.GraphicsEvents", "DrawTick"),
                new EventFinder("StardewModdingAPI.Events.GraphicsEvents", "OnPostRenderHudEventNoCheck"),
                new EventFinder("StardewModdingAPI.Events.GraphicsEvents", "OnPostRenderGuiEventNoCheck"),
                new EventFinder("StardewModdingAPI.Events.GraphicsEvents", "OnPreRenderHudEventNoCheck"),
                new EventFinder("StardewModdingAPI.Events.GraphicsEvents", "OnPreRenderGuiEventNoCheck"),

                // APIs removed in SMAPI 2.0
#if !SMAPI_1_x
                new TypeFinder("StardewModdingAPI.Command"),
                new TypeFinder("StardewModdingAPI.Config"),
                new TypeFinder("StardewModdingAPI.Log"),
                new EventFinder("StardewModdingAPI.Events.GameEvents", "Initialize"),
                new EventFinder("StardewModdingAPI.Events.GameEvents", "LoadContent"),
                new EventFinder("StardewModdingAPI.Events.GameEvents", "GameLoaded"),
                new EventFinder("StardewModdingAPI.Events.GameEvents", "FirstUpdateTick"),
                new EventFinder("StardewModdingAPI.Events.PlayerEvents", "LoadedGame"),
                new EventFinder("StardewModdingAPI.Events.PlayerEvents", "FarmerChanged"),
                new EventFinder("StardewModdingAPI.Events.TimeEvents", "DayOfMonthChanged"),
                new EventFinder("StardewModdingAPI.Events.TimeEvents", "YearOfGameChanged"),
                new EventFinder("StardewModdingAPI.Events.TimeEvents", "SeasonOfYearChanged"),
                new EventFinder("StardewModdingAPI.Events.TimeEvents", "OnNewDay"),
                new TypeFinder("StardewModdingAPI.Events.EventArgsCommand"),
                new TypeFinder("StardewModdingAPI.Events.EventArgsFarmerChanged"),
                new TypeFinder("StardewModdingAPI.Events.EventArgsLoadedGameChanged"),
                new TypeFinder("StardewModdingAPI.Events.EventArgsNewDay"),
                new TypeFinder("StardewModdingAPI.Events.EventArgsStringChanged"),
                new PropertyFinder("StardewModdingAPI.Mod", "PathOnDisk"),
                new PropertyFinder("StardewModdingAPI.Mod", "BaseConfigPath"),
                new PropertyFinder("StardewModdingAPI.Mod", "PerSaveConfigFolder"),
                new PropertyFinder("StardewModdingAPI.Mod", "PerSaveConfigPath"),
#endif

                /****
                ** Rewriters change CIL as needed to fix incompatible code
                ****/
                // crossplatform
                new MethodParentRewriter(typeof(SpriteBatch), typeof(SpriteBatchWrapper), onlyIfPlatformChanged: true),

                // Stardew Valley 1.2
                new FieldToPropertyRewriter(typeof(Game1), nameof(Game1.activeClickableMenu)),
                new FieldToPropertyRewriter(typeof(Game1), nameof(Game1.currentMinigame)),
                new FieldToPropertyRewriter(typeof(Game1), nameof(Game1.gameMode)),
                new FieldToPropertyRewriter(typeof(Game1), nameof(Game1.player)),
                new FieldReplaceRewriter(typeof(Game1), "borderFont", nameof(Game1.smallFont)),
                new FieldReplaceRewriter(typeof(Game1), "smoothFont", nameof(Game1.smallFont)),

                // SMAPI 1.9
                new TypeReferenceRewriter("StardewModdingAPI.Inheritance.ItemStackChange", typeof(ItemStackChange))
            };
        }
    }
}
