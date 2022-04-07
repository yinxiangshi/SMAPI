#nullable disable

using System;
using Microsoft.Xna.Framework.Content;
using StardewModdingAPI.Framework.Content;
using StardewModdingAPI.Framework.ContentManagers;
using StardewModdingAPI.Framework.Exceptions;
using StardewModdingAPI.Utilities;

namespace StardewModdingAPI.Framework.ModHelpers
{
    /// <inheritdoc cref="IModContentHelper"/>
    internal class ModContentHelper : BaseHelper, IModContentHelper
    {
        /*********
        ** Fields
        *********/
        /// <summary>SMAPI's core content logic.</summary>
        private readonly ContentCoordinator ContentCore;

        /// <summary>A content manager for this mod which manages files from the mod's folder.</summary>
        private readonly ModContentManager ModContentManager;

        /// <summary>The friendly mod name for use in errors.</summary>
        private readonly string ModName;

        /// <summary>A case-insensitive lookup of relative paths within the <see cref="ContentManager.RootDirectory"/>.</summary>
        private readonly CaseInsensitivePathCache RelativePathCache;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="contentCore">SMAPI's core content logic.</param>
        /// <param name="modFolderPath">The absolute path to the mod folder.</param>
        /// <param name="modID">The unique ID of the relevant mod.</param>
        /// <param name="modName">The friendly mod name for use in errors.</param>
        /// <param name="gameContentManager">The game content manager used for map tilesheets not provided by the mod.</param>
        /// <param name="relativePathCache">A case-insensitive lookup of relative paths within the <paramref name="relativePathCache"/>.</param>
        public ModContentHelper(ContentCoordinator contentCore, string modFolderPath, string modID, string modName, IContentManager gameContentManager, CaseInsensitivePathCache relativePathCache)
            : base(modID)
        {
            string managedAssetPrefix = contentCore.GetManagedAssetPrefix(modID);

            this.ContentCore = contentCore;
            this.ModContentManager = contentCore.CreateModContentManager(managedAssetPrefix, modName, modFolderPath, gameContentManager);
            this.ModName = modName;
            this.RelativePathCache = relativePathCache;
        }

        /// <inheritdoc />
        public T Load<T>(string relativePath)
        {
            relativePath = this.RelativePathCache.GetAssetName(relativePath);

            IAssetName assetName = this.ContentCore.ParseAssetName(relativePath, allowLocales: false);

            try
            {
                return this.ModContentManager.LoadExact<T>(assetName, useCache: false);
            }
            catch (Exception ex) when (ex is not SContentLoadException)
            {
                throw new SContentLoadException($"{this.ModName} failed loading content asset '{relativePath}' from its mod folder.", ex);
            }
        }

        /// <inheritdoc />
        public IAssetName GetInternalAssetName(string relativePath)
        {
            relativePath = this.RelativePathCache.GetAssetName(relativePath);
            return this.ModContentManager.GetInternalAssetKey(relativePath);
        }

        /// <inheritdoc />
        public IAssetData GetPatchHelper<T>(T data, string relativePath = null)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data), "Can't get a patch helper for a null value.");

            relativePath = relativePath != null
                ? this.RelativePathCache.GetAssetName(relativePath)
                : $"temp/{Guid.NewGuid():N}";

            return new AssetDataForObject(this.ContentCore.GetLocale(), this.ContentCore.ParseAssetName(relativePath, allowLocales: false), data, key => this.ContentCore.ParseAssetName(key, allowLocales: false).Name);
        }
    }
}
