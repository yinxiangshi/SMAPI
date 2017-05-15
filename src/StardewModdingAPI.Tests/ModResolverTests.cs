using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StardewModdingAPI.Framework.Models;
using StardewModdingAPI.Framework.ModLoading;
using StardewModdingAPI.Framework.Serialisation;
using StardewModdingAPI.Tests.Framework;

namespace StardewModdingAPI.Tests
{
    [TestFixture]
    public class ModResolverTests
    {
        /*********
        ** Unit tests
        *********/
        /****
        ** ReadManifests
        ****/
        [Test(Description = "Assert that the resolver correctly returns an empty list if there are no mods installed.")]
        public void ReadBasicManifest_NoMods_ReturnsEmptyList()
        {
            // arrange
            string rootFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(rootFolder);

            // act
            IModMetadata[] mods = new ModResolver().ReadManifests(rootFolder, new JsonHelper(), new ModCompatibility[0]).ToArray();

            // assert
            Assert.AreEqual(0, mods.Length, 0, $"Expected to find zero manifests, found {mods.Length} instead.");
        }

        [Test(Description = "Assert that the resolver correctly returns a failed metadata if there's an empty mod folder.")]
        public void ReadBasicManifest_EmptyModFolder_ReturnsFailedManifest()
        {
            // arrange
            string rootFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            string modFolder = Path.Combine(rootFolder, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(modFolder);

            // act
            IModMetadata[] mods = new ModResolver().ReadManifests(rootFolder, new JsonHelper(), new ModCompatibility[0]).ToArray();
            IModMetadata mod = mods.FirstOrDefault();

            // assert
            Assert.AreEqual(1, mods.Length, 0, $"Expected to find one manifest, found {mods.Length} instead.");
            Assert.AreEqual(ModMetadataStatus.Failed, mod.Status, "The mod metadata was not marked failed.");
            Assert.IsNotNull(mod.Error, "The mod metadata did not have an error message set.");
        }

        [Test(Description = "Assert that the resolver correctly reads manifest data from a randomised file.")]
        public void ReadBasicManifest_CanReadFile()
        {
            // create manifest data
            IDictionary<string, object> originalDependency = new Dictionary<string, object>
            {
                [nameof(IManifestDependency.UniqueID)] = Sample.String()
            };
            IDictionary<string, object> original = new Dictionary<string, object>
            {
                [nameof(IManifest.Name)] = Sample.String(),
                [nameof(IManifest.Author)] = Sample.String(),
                [nameof(IManifest.Version)] = new SemanticVersion(Sample.Int(), Sample.Int(), Sample.Int(), Sample.String()),
                [nameof(IManifest.Description)] = Sample.String(),
                [nameof(IManifest.UniqueID)] = $"{Sample.String()}.{Sample.String()}",
                [nameof(IManifest.EntryDll)] = $"{Sample.String()}.dll",
                [nameof(IManifest.MinimumApiVersion)] = $"{Sample.Int()}.{Sample.Int()}-{Sample.String()}",
                [nameof(IManifest.Dependencies)] = new[] { originalDependency },
                ["ExtraString"] = Sample.String(),
                ["ExtraInt"] = Sample.Int()
            };

            // write to filesystem
            string rootFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            string modFolder = Path.Combine(rootFolder, Guid.NewGuid().ToString("N"));
            string filename = Path.Combine(modFolder, "manifest.json");
            Directory.CreateDirectory(modFolder);
            File.WriteAllText(filename, JsonConvert.SerializeObject(original));

            // act
            IModMetadata[] mods = new ModResolver().ReadManifests(rootFolder, new JsonHelper(), new ModCompatibility[0]).ToArray();
            IModMetadata mod = mods.FirstOrDefault();

            // assert
            Assert.AreEqual(1, mods.Length, 0, "Expected to find one manifest.");
            Assert.IsNotNull(mod, "The loaded manifest shouldn't be null.");
            Assert.AreEqual(null, mod.Compatibility, "The compatibility record should be null since we didn't provide one.");
            Assert.AreEqual(modFolder, mod.DirectoryPath, "The directory path doesn't match.");
            Assert.AreEqual(ModMetadataStatus.Found, mod.Status, "The status doesn't match.");
            Assert.AreEqual(null, mod.Error, "The error should be null since parsing should have succeeded.");

            Assert.AreEqual(original[nameof(IManifest.Name)], mod.DisplayName, "The display name should use the manifest name.");
            Assert.AreEqual(original[nameof(IManifest.Name)], mod.Manifest.Name, "The manifest's name doesn't match.");
            Assert.AreEqual(original[nameof(IManifest.Author)], mod.Manifest.Author, "The manifest's author doesn't match.");
            Assert.AreEqual(original[nameof(IManifest.Description)], mod.Manifest.Description, "The manifest's description doesn't match.");
            Assert.AreEqual(original[nameof(IManifest.EntryDll)], mod.Manifest.EntryDll, "The manifest's entry DLL doesn't match.");
            Assert.AreEqual(original[nameof(IManifest.MinimumApiVersion)], mod.Manifest.MinimumApiVersion, "The manifest's minimum API version doesn't match.");
            Assert.AreEqual(original[nameof(IManifest.Version)]?.ToString(), mod.Manifest.Version?.ToString(), "The manifest's version doesn't match.");

            Assert.IsNotNull(mod.Manifest.ExtraFields, "The extra fields should not be null.");
            Assert.AreEqual(2, mod.Manifest.ExtraFields.Count, "The extra fields should contain two values.");
            Assert.AreEqual(original["ExtraString"], mod.Manifest.ExtraFields["ExtraString"], "The manifest's extra fields should contain an 'ExtraString' value.");
            Assert.AreEqual(original["ExtraInt"], mod.Manifest.ExtraFields["ExtraInt"], "The manifest's extra fields should contain an 'ExtraInt' value.");

            Assert.IsNotNull(mod.Manifest.Dependencies, "The dependencies field should not be null.");
            Assert.AreEqual(1, mod.Manifest.Dependencies.Length, "The dependencies field should contain one value.");
            Assert.AreEqual(originalDependency[nameof(IManifestDependency.UniqueID)], mod.Manifest.Dependencies[0].UniqueID, "The first dependency's unique ID doesn't match.");
        }

        /****
        ** ValidateManifests
        ****/
        [Test(Description = "Assert that validation doesn't fail if there are no mods installed.")]
        public void ValidateManifests_NoMods_DoesNothing()
        {
            new ModResolver().ValidateManifests(new ModMetadata[0], apiVersion: new SemanticVersion("1.0"));
        }

        [Test(Description = "Assert that validation skips manifests that have already failed without calling any other properties.")]
        public void ValidateManifests_Skips_Failed()
        {
            // arrange
            Mock<IModMetadata> mock = new Mock<IModMetadata>(MockBehavior.Strict);
            mock.Setup(p => p.Status).Returns(ModMetadataStatus.Failed);

            // act
            new ModResolver().ValidateManifests(new[] { mock.Object }, apiVersion: new SemanticVersion("1.0"));

            // assert
            mock.VerifyGet(p => p.Status, Times.Once, "The validation did not check the manifest status.");
        }

        [Test(Description = "Assert that validation fails if the mod has 'assume broken' compatibility.")]
        public void ValidateManifests_ModCompatibility_AssumeBroken_Fails()
        {
            // arrange
            Mock<IModMetadata> mock = new Mock<IModMetadata>(MockBehavior.Strict);
            mock.Setup(p => p.Status).Returns(ModMetadataStatus.Found);
            mock.Setup(p => p.Compatibility).Returns(new ModCompatibility { Compatibility = ModCompatibilityType.AssumeBroken });
            mock.Setup(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<string>())).Returns(() => mock.Object);

            // act
            new ModResolver().ValidateManifests(new[] { mock.Object }, apiVersion: new SemanticVersion("1.0"));

            // assert
            mock.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<string>()), Times.Once, "The validation did not fail the metadata.");
        }

        [Test(Description = "Assert that validation fails when the minimum API version is higher than the current SMAPI version.")]
        public void ValidateManifests_MinimumApiVersion_Fails()
        {
            // arrange
            Mock<IModMetadata> mock = new Mock<IModMetadata>(MockBehavior.Strict);
            mock.Setup(p => p.Status).Returns(ModMetadataStatus.Found);
            mock.Setup(p => p.Compatibility).Returns(() => null);
            mock.Setup(p => p.Manifest).Returns(this.GetRandomManifest(m => m.MinimumApiVersion = "1.1"));
            mock.Setup(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<string>())).Returns(() => mock.Object);

            // act
            new ModResolver().ValidateManifests(new[] { mock.Object }, apiVersion: new SemanticVersion("1.0"));

            // assert
            mock.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<string>()), Times.Once, "The validation did not fail the metadata.");
        }

        [Test(Description = "Assert that validation fails when the manifest references a DLL that does not exist.")]
        public void ValidateManifests_MissingEntryDLL_Fails()
        {
            // arrange
            Mock<IModMetadata> mock = new Mock<IModMetadata>(MockBehavior.Strict);
            mock.Setup(p => p.Status).Returns(ModMetadataStatus.Found);
            mock.Setup(p => p.Compatibility).Returns(() => null);
            mock.Setup(p => p.Manifest).Returns(this.GetRandomManifest());
            mock.Setup(p => p.DirectoryPath).Returns(Path.GetTempPath());
            mock.Setup(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<string>())).Returns(() => mock.Object);

            // act
            new ModResolver().ValidateManifests(new[] { mock.Object }, apiVersion: new SemanticVersion("1.0"));

            // assert
            mock.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<string>()), Times.Once, "The validation did not fail the metadata.");
        }

        [Test(Description = "Assert that validation fails when the manifest references a DLL that does not exist.")]
        public void ValidateManifests_Valid_Passes()
        {
            // set up manifest
            IManifest manifest = this.GetRandomManifest();

            // create DLL
            string modFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(modFolder);
            File.WriteAllText(Path.Combine(modFolder, manifest.EntryDll), "");

            // arrange
            Mock<IModMetadata> mock = new Mock<IModMetadata>(MockBehavior.Strict);
            mock.Setup(p => p.Status).Returns(ModMetadataStatus.Found);
            mock.Setup(p => p.Compatibility).Returns(() => null);
            mock.Setup(p => p.Manifest).Returns(manifest);
            mock.Setup(p => p.DirectoryPath).Returns(modFolder);

            // act
            new ModResolver().ValidateManifests(new[] { mock.Object }, apiVersion: new SemanticVersion("1.0"));

            // assert
            // if Moq doesn't throw a method-not-setup exception, the validation didn't override the status.
        }

        /****
        ** ProcessDependencies
        ****/
        [Test(Description = "Assert that processing dependencies doesn't fail if there are no mods installed.")]
        public void ProcessDependencies_NoMods_DoesNothing()
        {
            // act
            IModMetadata[] mods = new ModResolver().ProcessDependencies(new IModMetadata[0]).ToArray();

            // assert
            Assert.AreEqual(0, mods.Length, 0, "Expected to get an empty list of mods.");
        }

        [Test(Description = "Assert that processing dependencies doesn't change the order if there are no mod dependencies.")]
        public void ProcessDependencies_NoDependencies_DoesNothing()
        {
            // arrange
            // A B C
            Mock<IModMetadata> modA = this.GetMetadataForDependencyTest("Mod A");
            Mock<IModMetadata> modB = this.GetMetadataForDependencyTest("Mod B");
            Mock<IModMetadata> modC = this.GetMetadataForDependencyTest("Mod C");

            // act
            IModMetadata[] mods = new ModResolver().ProcessDependencies(new[] { modA.Object, modB.Object, modC.Object }).ToArray();

            // assert
            Assert.AreEqual(3, mods.Length, 0, "Expected to get the same number of mods input.");
            Assert.AreSame(modA.Object, mods[0], "The load order unexpectedly changed with no dependencies.");
            Assert.AreSame(modB.Object, mods[1], "The load order unexpectedly changed with no dependencies.");
            Assert.AreSame(modC.Object, mods[2], "The load order unexpectedly changed with no dependencies.");
        }

        [Test(Description = "Assert that simple dependencies are reordered correctly.")]
        public void ProcessDependencies_Reorders_SimpleDependencies()
        {
            // arrange
            // A ◀── B
            // ▲     ▲
            // │     │
            // └─ C ─┘
            Mock<IModMetadata> modA = this.GetMetadataForDependencyTest("Mod A");
            Mock<IModMetadata> modB = this.GetMetadataForDependencyTest("Mod B", dependencies: new[] { "Mod A" });
            Mock<IModMetadata> modC = this.GetMetadataForDependencyTest("Mod C", dependencies: new[] { "Mod A", "Mod B" });

            // act
            IModMetadata[] mods = new ModResolver().ProcessDependencies(new[] { modC.Object, modA.Object, modB.Object }).ToArray();

            // assert
            Assert.AreEqual(3, mods.Length, 0, "Expected to get the same number of mods input.");
            Assert.AreSame(modA.Object, mods[0], "The load order is incorrect: mod A should be first since the other mods depend on it.");
            Assert.AreSame(modB.Object, mods[1], "The load order is incorrect: mod B should be second since it needs mod A, and is needed by mod C.");
            Assert.AreSame(modC.Object, mods[2], "The load order is incorrect: mod C should be third since it needs both mod A and mod B.");
        }

        [Test(Description = "Assert that simple dependency chains are reordered correctly.")]
        public void ProcessDependencies_Reorders_DependencyChain()
        {
            // arrange
            // A ◀── B ◀── C ◀── D
            Mock<IModMetadata> modA = this.GetMetadataForDependencyTest("Mod A");
            Mock<IModMetadata> modB = this.GetMetadataForDependencyTest("Mod B", dependencies: new[] { "Mod A" });
            Mock<IModMetadata> modC = this.GetMetadataForDependencyTest("Mod C", dependencies: new[] { "Mod B" });
            Mock<IModMetadata> modD = this.GetMetadataForDependencyTest("Mod D", dependencies: new[] { "Mod C" });

            // act
            IModMetadata[] mods = new ModResolver().ProcessDependencies(new[] { modC.Object, modA.Object, modB.Object, modD.Object }).ToArray();

            // assert
            Assert.AreEqual(4, mods.Length, 0, "Expected to get the same number of mods input.");
            Assert.AreSame(modA.Object, mods[0], "The load order is incorrect: mod A should be first since it's needed by mod B.");
            Assert.AreSame(modB.Object, mods[1], "The load order is incorrect: mod B should be second since it needs mod A, and is needed by mod C.");
            Assert.AreSame(modC.Object, mods[2], "The load order is incorrect: mod C should be third since it needs mod B, and is needed by mod D.");
            Assert.AreSame(modD.Object, mods[3], "The load order is incorrect: mod D should be fourth since it needs mod C.");
        }

        [Test(Description = "Assert that overlapping dependency chains are reordered correctly.")]
        public void ProcessDependencies_Reorders_OverlappingDependencyChain()
        {
            // arrange
            // A ◀── B ◀── C ◀── D
            //       ▲     ▲
            //       │     │
            //       E ◀── F
            Mock<IModMetadata> modA = this.GetMetadataForDependencyTest("Mod A");
            Mock<IModMetadata> modB = this.GetMetadataForDependencyTest("Mod B", dependencies: new[] { "Mod A" });
            Mock<IModMetadata> modC = this.GetMetadataForDependencyTest("Mod C", dependencies: new[] { "Mod B" });
            Mock<IModMetadata> modD = this.GetMetadataForDependencyTest("Mod D", dependencies: new[] { "Mod C" });
            Mock<IModMetadata> modE = this.GetMetadataForDependencyTest("Mod E", dependencies: new[] { "Mod B" });
            Mock<IModMetadata> modF = this.GetMetadataForDependencyTest("Mod F", dependencies: new[] { "Mod C", "Mod E" });

            // act
            IModMetadata[] mods = new ModResolver().ProcessDependencies(new[] { modC.Object, modA.Object, modB.Object, modD.Object, modF.Object, modE.Object }).ToArray();

            // assert
            Assert.AreEqual(6, mods.Length, 0, "Expected to get the same number of mods input.");
            Assert.AreSame(modA.Object, mods[0], "The load order is incorrect: mod A should be first since it's needed by mod B.");
            Assert.AreSame(modB.Object, mods[1], "The load order is incorrect: mod B should be second since it needs mod A, and is needed by mod C.");
            Assert.AreSame(modC.Object, mods[2], "The load order is incorrect: mod C should be third since it needs mod B, and is needed by mod D.");
            Assert.AreSame(modD.Object, mods[3], "The load order is incorrect: mod D should be fourth since it needs mod C.");
            Assert.AreSame(modE.Object, mods[4], "The load order is incorrect: mod E should be fifth since it needs mod B, but is specified after C which also needs mod B.");
            Assert.AreSame(modF.Object, mods[5], "The load order is incorrect: mod F should be last since it needs mods E and C.");
        }

        [Test(Description = "Assert that mods with circular dependency chains are skipped, but any other mods are loaded in the correct order.")]
        public void ProcessDependencies_Skips_CircularDependentMods()
        {
            // arrange
            // A ◀── B ◀── C ──▶ D
            //             ▲     │
            //             │     ▼
            //             └──── E
            Mock<IModMetadata> modA = this.GetMetadataForDependencyTest("Mod A");
            Mock<IModMetadata> modB = this.GetMetadataForDependencyTest("Mod B", dependencies: new[] { "Mod A" });
            Mock<IModMetadata> modC = this.GetMetadataForDependencyTest("Mod C", dependencies: new[] { "Mod B", "Mod D" }, allowStatusChange: true);
            Mock<IModMetadata> modD = this.GetMetadataForDependencyTest("Mod D", dependencies: new[] { "Mod E" }, allowStatusChange: true);
            Mock<IModMetadata> modE = this.GetMetadataForDependencyTest("Mod E", dependencies: new[] { "Mod C" }, allowStatusChange: true);

            // act
            IModMetadata[] mods = new ModResolver().ProcessDependencies(new[] { modC.Object, modA.Object, modB.Object, modD.Object, modE.Object }).ToArray();

            // assert
            Assert.AreEqual(5, mods.Length, 0, "Expected to get the same number of mods input.");
            Assert.AreSame(modA.Object, mods[0], "The load order is incorrect: mod A should be first since it's needed by mod B.");
            Assert.AreSame(modB.Object, mods[1], "The load order is incorrect: mod B should be second since it needs mod A.");
            modC.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<string>()), Times.Once, "Mod C was expected to fail since it's part of a dependency loop.");
            modD.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<string>()), Times.Once, "Mod D was expected to fail since it's part of a dependency loop.");
            modE.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<string>()), Times.Once, "Mod E was expected to fail since it's part of a dependency loop.");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get a randomised basic manifest.</summary>
        /// <param name="adjust">Adjust the generated manifest.</param>
        private Manifest GetRandomManifest(Action<Manifest> adjust = null)
        {
            Manifest manifest = new Manifest
            {
                Name = Sample.String(),
                Author = Sample.String(),
                Version = new SemanticVersion(Sample.Int(), Sample.Int(), Sample.Int(), Sample.String()),
                Description = Sample.String(),
                UniqueID = $"{Sample.String()}.{Sample.String()}",
                EntryDll = $"{Sample.String()}.dll"
            };
            adjust?.Invoke(manifest);
            return manifest;
        }

        /// <summary>Get a randomised basic manifest.</summary>
        /// <param name="uniqueID">The mod's name and unique ID.</param>
        /// <param name="dependencies">The dependencies this mod requires.</param>
        /// <param name="allowStatusChange">Whether the code being tested is allowed to change the mod status.</param>
        private Mock<IModMetadata> GetMetadataForDependencyTest(string uniqueID, string[] dependencies = null, bool allowStatusChange = false)
        {
            Mock<IModMetadata> mod = new Mock<IModMetadata>(MockBehavior.Strict);
            mod.Setup(p => p.Status).Returns(ModMetadataStatus.Found);
            mod.Setup(p => p.DisplayName).Returns(uniqueID);
            mod.Setup(p => p.Manifest).Returns(
                this.GetRandomManifest(manifest =>
                {
                    manifest.Name = uniqueID;
                    manifest.UniqueID = uniqueID;
                    manifest.Dependencies = dependencies?.Select(dependencyID => (IManifestDependency)new ManifestDependency(dependencyID)).ToArray();
                })
            );
            if (allowStatusChange)
            {
                mod
                    .Setup(p => p.SetStatus(It.IsAny<ModMetadataStatus>(), It.IsAny<string>()))
                    .Callback<ModMetadataStatus, string>((status, message) => Console.WriteLine($"<{uniqueID} changed status: [{status}] {message}"))
                    .Returns(mod.Object);
            }
            return mod;
        }
    }
}
