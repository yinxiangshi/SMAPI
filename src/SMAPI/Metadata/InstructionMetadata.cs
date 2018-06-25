using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Framework.ModLoading;
using StardewModdingAPI.Framework.ModLoading.Finders;
using StardewModdingAPI.Framework.ModLoading.Rewriters;
using StardewModdingAPI.Framework.RewriteFacades;
using StardewValley;

namespace StardewModdingAPI.Metadata
{
    /// <summary>Provides CIL instruction handlers which rewrite mods for compatibility and throw exceptions for incompatible code.</summary>
    internal class InstructionMetadata
    {
        /*********
        ** Properties
        *********/
        /// <summary>The assembly names to which to heuristically detect broken references.</summary>
        /// <remarks>The current implementation only works correctly with assemblies that should always be present.</remarks>
        private readonly string[] ValidateReferencesToAssemblies = { "StardewModdingAPI", "Stardew Valley", "StardewValley" };


        /*********
        ** Public methods
        *********/
        /// <summary>Get rewriters which detect or fix incompatible CIL instructions in mod assemblies.</summary>
        public IEnumerable<IInstructionHandler> GetHandlers()
        {
            return new IInstructionHandler[]
            {
                /****
                ** rewrite CIL to fix incompatible code
                ****/
                // rewrite for crossplatform compatibility
                new MethodParentRewriter(typeof(SpriteBatch), typeof(SpriteBatchMethods), onlyIfPlatformChanged: true),

                // rewrite for SMAPI 2.0
                new VirtualEntryCallRemover(),

                // rewrite for SMAPI 2.6
                new TypeReferenceRewriter("StardewModdingAPI.ISemanticVersion", typeof(ISemanticVersion), type => type.Scope.Name == "StardewModdingAPI"), // moved to SMAPI.Toolkit.CoreInterfaces

                // rewrite for Stardew Valley 1.3
                new StaticFieldToConstantRewriter<int>(typeof(Game1), "tileSize", Game1.tileSize),

                /****
                ** detect incompatible code
                ****/
                // detect broken code
                new ReferenceToMissingMemberFinder(this.ValidateReferencesToAssemblies),
                new ReferenceToMemberWithUnexpectedTypeFinder(this.ValidateReferencesToAssemblies),

                /****
                ** detect code which may impact game stability
                ****/
                new TypeFinder("Harmony.HarmonyInstance", InstructionHandleResult.DetectedGamePatch),
                new TypeFinder("System.Runtime.CompilerServices.CallSite", InstructionHandleResult.DetectedDynamic),
                new FieldFinder(typeof(SaveGame).FullName, nameof(SaveGame.serializer), InstructionHandleResult.DetectedSaveSerialiser),
                new FieldFinder(typeof(SaveGame).FullName, nameof(SaveGame.farmerSerializer), InstructionHandleResult.DetectedSaveSerialiser),
                new FieldFinder(typeof(SaveGame).FullName, nameof(SaveGame.locationSerializer), InstructionHandleResult.DetectedSaveSerialiser),
                new EventFinder(typeof(SpecialisedEvents).FullName, nameof(SpecialisedEvents.UnvalidatedUpdateTick), InstructionHandleResult.DetectedUnvalidatedUpdateTick)
            };
        }
    }
}
