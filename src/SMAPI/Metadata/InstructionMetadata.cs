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
        private readonly string[] ValidateReferencesToAssemblies = { "StardewModdingAPI", "Stardew Valley", "StardewValley", "Netcode" };


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

                // rewrite for Stardew Valley 1.3
                new StaticFieldToConstantRewriter<int>(typeof(Game1), "tileSize", Game1.tileSize),

                /****
                ** detect mod issues
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
                new TypeFinder(typeof(ISpecialisedEvents).FullName, InstructionHandleResult.DetectedUnvalidatedUpdateTick),
#if !SMAPI_3_0_STRICT
                new EventFinder(typeof(SpecialisedEvents).FullName, nameof(SpecialisedEvents.UnvalidatedUpdateTick), InstructionHandleResult.DetectedUnvalidatedUpdateTick),
#endif
                
                /****
                ** detect paranoid issues
                ****/
                // filesystem access
                new TypeFinder(typeof(System.IO.File).FullName, InstructionHandleResult.DetectedFilesystemAccess),
                new TypeFinder(typeof(System.IO.FileStream).FullName, InstructionHandleResult.DetectedFilesystemAccess),
                new TypeFinder(typeof(System.IO.FileInfo).FullName, InstructionHandleResult.DetectedFilesystemAccess),
                new TypeFinder(typeof(System.IO.Directory).FullName, InstructionHandleResult.DetectedFilesystemAccess),
                new TypeFinder(typeof(System.IO.DirectoryInfo).FullName, InstructionHandleResult.DetectedFilesystemAccess),
                new TypeFinder(typeof(System.IO.DriveInfo).FullName, InstructionHandleResult.DetectedFilesystemAccess),
                new TypeFinder(typeof(System.IO.FileSystemWatcher).FullName, InstructionHandleResult.DetectedFilesystemAccess),

                // shell access
                new TypeFinder(typeof(System.Diagnostics.Process).FullName, InstructionHandleResult.DetectedShellAccess)
            };
        }
    }
}
