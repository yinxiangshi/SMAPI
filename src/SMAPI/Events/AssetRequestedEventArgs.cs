using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.Content;
using xTile;

namespace StardewModdingAPI.Events
{
    /// <summary>Event arguments for an <see cref="IContentEvents.AssetRequested"/> event.</summary>
    public class AssetRequestedEventArgs : EventArgs
    {
        /*********
        ** Fields
        *********/
        /// <summary>The mod handling the event.</summary>
        private readonly IModMetadata Mod;

        /// <summary>Get the mod metadata for a content pack, if it's a valid content pack for the mod.</summary>
        private readonly Func<IModMetadata, string, string, IModMetadata> GetOnBehalfOf;


        /*********
        ** Accessors
        *********/
        /// <summary>The name of the asset being requested.</summary>
        public IAssetName Name { get; }

        /// <summary>The <see cref="Name"/> with any locale codes stripped.</summary>
        /// <remarks>For example, if <see cref="Name"/> contains a locale like <c>Data/Bundles.fr-FR</c>, this will be the name without locale like <c>Data/Bundles</c>. If the name has no locale, this field is equivalent.</remarks>
        public IAssetName NameWithoutLocale { get; }

        /// <summary>The load operations requested by the event handler.</summary>
        internal IList<AssetLoadOperation> LoadOperations { get; } = new List<AssetLoadOperation>();

        /// <summary>The edit operations requested by the event handler.</summary>
        internal IList<AssetEditOperation> EditOperations { get; } = new List<AssetEditOperation>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="mod">The mod handling the event.</param>
        /// <param name="name">The name of the asset being requested.</param>
        /// <param name="nameWithoutLocale">The <paramref name="name"/> with any locale codes stripped.</param>
        /// <param name="getOnBehalfOf">Get the mod metadata for a content pack, if it's a valid content pack for the mod.</param>
        internal AssetRequestedEventArgs(IModMetadata mod, IAssetName name, IAssetName nameWithoutLocale, Func<IModMetadata, string, string, IModMetadata> getOnBehalfOf)
        {
            this.Mod = mod;
            this.Name = name;
            this.NameWithoutLocale = nameWithoutLocale;
            this.GetOnBehalfOf = getOnBehalfOf;
        }

        /// <summary>Provide the initial instance for the asset, instead of trying to load it from the game's <c>Content</c> folder.</summary>
        /// <param name="load">Get the initial instance of an asset.</param>
        /// <param name="priority">If there are multiple loads that apply to the same asset, the priority with which this one should be applied.</param>
        /// <param name="onBehalfOf">The content pack ID on whose behalf you're applying the change. This is only valid for content packs for your mod.</param>
        /// <remarks>
        /// Usage notes:
        /// <list type="bullet">
        ///   <item>The asset doesn't need to exist in the game's <c>Content</c> folder. If any mod loads the asset, the game will see it as an existing asset as if it was in that folder.</item>
        ///   <item>Each asset can logically only have one initial instance. If multiple loads apply at the same time, SMAPI will use the <paramref name="priority"/> parameter to decide what happens. If you're making changes to the existing asset instead of replacing it, you should use <see cref="Edit"/> instead to avoid those limitations and improve mod compatibility.</item>
        /// </list>
        /// </remarks>
        public void LoadFrom(Func<object> load, AssetLoadPriority priority, string onBehalfOf = null)
        {
            this.LoadOperations.Add(
                new AssetLoadOperation(
                    mod: this.Mod,
                    priority: priority,
                    onBehalfOf: this.GetOnBehalfOf(this.Mod, onBehalfOf, "load assets"),
                    getData: _ => load()
                )
            );
        }

        /// <summary>Provide the initial instance for the asset from a file in your mod folder, instead of trying to load it from the game's <c>Content</c> folder.</summary>
        /// <typeparam name="TAsset">The expected data type. The main supported types are <see cref="Map"/>, <see cref="Texture2D"/>, dictionaries, and lists; other types may be supported by the game's content pipeline.</typeparam>
        /// <param name="relativePath">The relative path to the file in your mod folder.</param>
        /// <param name="priority">If there are multiple loads that apply to the same asset, the priority with which this one should be applied.</param>
        /// <remarks>
        /// Usage notes:
        /// <list type="bullet">
        ///   <item>The asset doesn't need to exist in the game's <c>Content</c> folder. If any mod loads the asset, the game will see it as an existing asset as if it was in that folder.</item>
        ///   <item>Each asset can logically only have one initial instance. If multiple loads apply at the same time, SMAPI will raise an error and ignore all of them. If you're making changes to the existing asset instead of replacing it, you should use <see cref="Edit"/> instead to avoid those limitations and improve mod compatibility.</item>
        /// </list>
        /// </remarks>
        public void LoadFromModFile<TAsset>(string relativePath, AssetLoadPriority priority)
        {
            this.LoadOperations.Add(
                new AssetLoadOperation(
                    mod: this.Mod,
                    priority: priority,
                    onBehalfOf: null,
                    _ => this.Mod.Mod.Helper.Content.Load<TAsset>(relativePath))
            );
        }

        /// <summary>Edit the asset after it's loaded.</summary>
        /// <param name="apply">Apply changes to the asset.</param>
        /// <param name="priority">If there are multiple edits that apply to the same asset, the priority with which this one should be applied.</param>
        /// <param name="onBehalfOf">The content pack ID on whose behalf you're applying the change. This is only valid for content packs for your mod.</param>
        /// <remarks>
        /// Usage notes:
        /// <list type="bullet">
        ///   <item>Editing an asset which doesn't exist has no effect. This is applied after the asset is loaded from the game's <c>Content</c> folder, or from any mod's <see cref="LoadFrom"/> or <see cref="LoadFromModFile{TAsset}"/>.</item>
        ///   <item>You can apply any number of edits to the asset. Each edit will be applied on top of the previous one (i.e. it'll see the merged asset from all previous edits as its input).</item>
        /// </list>
        /// </remarks>
        public void Edit(Action<IAssetData> apply, AssetEditPriority priority = AssetEditPriority.Default, string onBehalfOf = null)
        {
            this.EditOperations.Add(
                new AssetEditOperation(
                    mod: this.Mod,
                    priority: priority,
                    onBehalfOf: this.GetOnBehalfOf(this.Mod, onBehalfOf, "edit assets"),
                    apply
                )
            );
        }
    }
}
