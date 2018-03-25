using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Framework.Reflection;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Projectiles;
using StardewValley.TerrainFeatures;

namespace StardewModdingAPI.Metadata
{
    /// <summary>Propagates changes to core assets to the game state.</summary>
    internal class CoreAssetPropagator
    {
        /*********
        ** Properties
        *********/
        /// <summary>Normalises an asset key to match the cache key.</summary>
        private readonly Func<string, string> GetNormalisedPath;

        /// <summary>Simplifies access to private game code.</summary>
        private readonly Reflector Reflection;


        /*********
        ** Public methods
        *********/
        /// <summary>Initialise the core asset data.</summary>
        /// <param name="getNormalisedPath">Normalises an asset key to match the cache key.</param>
        /// <param name="reflection">Simplifies access to private code.</param>
        public CoreAssetPropagator(Func<string, string> getNormalisedPath, Reflector reflection)
        {
            this.GetNormalisedPath = getNormalisedPath;
            this.Reflection = reflection;
        }

        /// <summary>Reload one of the game's core assets (if applicable).</summary>
        /// <param name="content">The content manager through which to reload the asset.</param>
        /// <param name="key">The asset key to reload.</param>
        /// <returns>Returns whether an asset was reloaded.</returns>
        public bool Propagate(LocalizedContentManager content, string key)
        {
            object result = this.PropagateImpl(content, key);
            if (result is bool b)
                return b;
            return result != null;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Reload one of the game's core assets (if applicable).</summary>
        /// <param name="content">The content manager through which to reload the asset.</param>
        /// <param name="key">The asset key to reload.</param>
        /// <returns>Returns any non-null value to indicate an asset was loaded.</returns>
        private object PropagateImpl(LocalizedContentManager content, string key)
        {
            Reflector reflection = this.Reflection;
            switch (key.ToLower().Replace("/", "\\")) // normalised key so we can compare statically
            {
                /****
                ** Buildings
                ****/
                case "buildings\\houses": // Farm
#if STARDEW_VALLEY_1_3
                    reflection.GetField<Texture2D>(typeof(Farm), nameof(Farm.houseTextures)).SetValue(content.Load<Texture2D>(key));
                    return true;
#else
                    {
                        Farm farm = Game1.getFarm();
                        if (farm == null)
                            return false;
                        return farm.houseTextures = content.Load<Texture2D>(key);
                    }
#endif

                /****
                ** Content\Characters\Farmer
                ****/
                case "characters\\farmer\\accessories": // Game1.loadContent
                    return FarmerRenderer.accessoriesTexture = content.Load<Texture2D>(key);

                case "characters\\farmer\\farmer_base": // Farmer
                    if (Game1.player == null || !Game1.player.isMale)
                        return false;
#if STARDEW_VALLEY_1_3
                    return Game1.player.FarmerRenderer = new FarmerRenderer(key);
#else
                    return Game1.player.FarmerRenderer = new FarmerRenderer(content.Load<Texture2D>(key));
#endif

                case "characters\\farmer\\farmer_girl_base": // Farmer
                    if (Game1.player == null || Game1.player.isMale)
                        return false;
#if STARDEW_VALLEY_1_3
                    return Game1.player.FarmerRenderer = new FarmerRenderer(key);
#else
                    return Game1.player.FarmerRenderer = new FarmerRenderer(content.Load<Texture2D>(key));
#endif

                case "characters\\farmer\\hairstyles": // Game1.loadContent
                    return FarmerRenderer.hairStylesTexture = content.Load<Texture2D>(key);

                case "characters\\farmer\\hats": // Game1.loadContent
                    return FarmerRenderer.hatsTexture = content.Load<Texture2D>(key);

                case "characters\\farmer\\shirts": // Game1.loadContent
                    return FarmerRenderer.shirtsTexture = content.Load<Texture2D>(key);

                /****
                ** Content\Data
                ****/
                case "data\\achievements": // Game1.loadContent
                    return Game1.achievements = content.Load<Dictionary<int, string>>(key);

                case "data\\bigcraftablesinformation": // Game1.loadContent
                    return Game1.bigCraftablesInformation = content.Load<Dictionary<int, string>>(key);

                case "data\\cookingrecipes": // CraftingRecipe.InitShared
                    return CraftingRecipe.cookingRecipes = content.Load<Dictionary<string, string>>(key);

                case "data\\craftingrecipes": // CraftingRecipe.InitShared
                    return CraftingRecipe.craftingRecipes = content.Load<Dictionary<string, string>>(key);

                case "data\\npcgifttastes": // Game1.loadContent
                    return Game1.NPCGiftTastes = content.Load<Dictionary<string, string>>(key);

                case "data\\objectinformation": // Game1.loadContent
                    return Game1.objectInformation = content.Load<Dictionary<int, string>>(key);

                /****
                ** Content\Fonts
                ****/
                case "fonts\\spritefont1": // Game1.loadContent
                    return Game1.dialogueFont = content.Load<SpriteFont>(key);

                case "fonts\\smallfont": // Game1.loadContent
                    return Game1.smallFont = content.Load<SpriteFont>(key);

                case "fonts\\tinyfont": // Game1.loadContent
                    return Game1.tinyFont = content.Load<SpriteFont>(key);

                case "fonts\\tinyfontborder": // Game1.loadContent
                    return Game1.tinyFontBorder = content.Load<SpriteFont>(key);

                /****
                ** Content\Lighting
                ****/
                case "loosesprites\\lighting\\greenlight": // Game1.loadContent
                    return Game1.cauldronLight = content.Load<Texture2D>(key);

                case "loosesprites\\lighting\\indoorwindowlight": // Game1.loadContent
                    return Game1.indoorWindowLight = content.Load<Texture2D>(key);

                case "loosesprites\\lighting\\lantern": // Game1.loadContent
                    return Game1.lantern = content.Load<Texture2D>(key);

                case "loosesprites\\lighting\\sconcelight": // Game1.loadContent
                    return Game1.sconceLight = content.Load<Texture2D>(key);

                case "loosesprites\\lighting\\windowlight": // Game1.loadContent
                    return Game1.windowLight = content.Load<Texture2D>(key);

                /****
                ** Content\LooseSprites
                ****/
                case "loosesprites\\controllermaps": // Game1.loadContent
                    return Game1.controllerMaps = content.Load<Texture2D>(key);

                case "loosesprites\\cursors": // Game1.loadContent
                    return Game1.mouseCursors = content.Load<Texture2D>(key);

                case "loosesprites\\daybg": // Game1.loadContent
                    return Game1.daybg = content.Load<Texture2D>(key);

                case "loosesprites\\font_bold": // Game1.loadContent
                    return SpriteText.spriteTexture = content.Load<Texture2D>(key);

                case "loosesprites\\font_colored": // Game1.loadContent
                    return SpriteText.coloredTexture = content.Load<Texture2D>(key);

                case "loosesprites\\nightbg": // Game1.loadContent
                    return Game1.nightbg = content.Load<Texture2D>(key);

                case "loosesprites\\shadow": // Game1.loadContent
                    return Game1.shadowTexture = content.Load<Texture2D>(key);

                /****
                ** Content\Critters
                ****/
#if !STARDEW_VALLEY_1_3
                case "tilesheets\\critters": // Criter.InitShared
                    return Critter.critterTexture = content.Load<Texture2D>(key);
#endif

                case "tilesheets\\crops": // Game1.loadContent
                    return Game1.cropSpriteSheet = content.Load<Texture2D>(key);

                case "tilesheets\\debris": // Game1.loadContent
                    return Game1.debrisSpriteSheet = content.Load<Texture2D>(key);

                case "tilesheets\\emotes": // Game1.loadContent
                    return Game1.emoteSpriteSheet = content.Load<Texture2D>(key);

                case "tilesheets\\furniture": // Game1.loadContent
                    return Furniture.furnitureTexture = content.Load<Texture2D>(key);

                case "tilesheets\\projectiles": // Game1.loadContent
                    return Projectile.projectileSheet = content.Load<Texture2D>(key);

                case "tilesheets\\rain": // Game1.loadContent
                    return Game1.rainTexture = content.Load<Texture2D>(key);

                case "tilesheets\\tools": // Game1.ResetToolSpriteSheet
                    Game1.ResetToolSpriteSheet();
                    return true;

                case "tilesheets\\weapons": // Game1.loadContent
                    return Tool.weaponsTexture = content.Load<Texture2D>(key);

                /****
                ** Content\Maps
                ****/
                case "maps\\menutiles": // Game1.loadContent
                    return Game1.menuTexture = content.Load<Texture2D>(key);

                case "maps\\springobjects": // Game1.loadContent
                    return Game1.objectSpriteSheet = content.Load<Texture2D>(key);

                case "maps\\walls_and_floors": // Wallpaper
                    return Wallpaper.wallpaperTexture = content.Load<Texture2D>(key);

                /****
                ** Content\Minigames
                ****/
                case "minigames\\clouds": // TitleMenu
                    if (Game1.activeClickableMenu is TitleMenu)
                    {
                        reflection.GetField<Texture2D>(Game1.activeClickableMenu, "cloudsTexture").SetValue(content.Load<Texture2D>(key));
                        return true;
                    }
                    return false;

                case "minigames\\titlebuttons": // TitleMenu
                    if (Game1.activeClickableMenu is TitleMenu titleMenu)
                    {
                        Texture2D texture = content.Load<Texture2D>(key);
                        reflection.GetField<Texture2D>(titleMenu, "titleButtonsTexture").SetValue(texture);
                        foreach (TemporaryAnimatedSprite bird in reflection.GetField<List<TemporaryAnimatedSprite>>(titleMenu, "birds").GetValue())
#if STARDEW_VALLEY_1_3
                            bird.texture = texture;
#else
                            bird.Texture = texture;
#endif
                        return true;
                    }
                    return false;

                /****
                ** Content\TileSheets
                ****/
                case "tilesheets\\animations": // Game1.loadContent
                    return Game1.animations = content.Load<Texture2D>(key);

                case "tilesheets\\buffsicons": // Game1.loadContent
                    return Game1.buffsIcons = content.Load<Texture2D>(key);

                case "tilesheets\\bushes": // new Bush()
#if STARDEW_VALLEY_1_3
                    reflection.GetField<Lazy<Texture2D>>(typeof(Bush), "texture").SetValue(new Lazy<Texture2D>(() => content.Load<Texture2D>(key)));
                    return true;
#else
                    return Bush.texture = content.Load<Texture2D>(key);
#endif

                case "tilesheets\\craftables": // Game1.loadContent
                    return Game1.bigCraftableSpriteSheet = content.Load<Texture2D>(key);

                case "tilesheets\\fruittrees": // FruitTree
                    return FruitTree.texture = content.Load<Texture2D>(key);

                /****
                ** Content\TerrainFeatures
                ****/
                case "terrainfeatures\\flooring": // Flooring
                    return Flooring.floorsTexture = content.Load<Texture2D>(key);

                case "terrainfeatures\\hoedirt": // from HoeDirt
                    return HoeDirt.lightTexture = content.Load<Texture2D>(key);

                case "terrainfeatures\\hoedirtdark": // from HoeDirt
                    return HoeDirt.darkTexture = content.Load<Texture2D>(key);

                case "terrainfeatures\\hoedirtsnow": // from HoeDirt
                    return HoeDirt.snowTexture = content.Load<Texture2D>(key);

                case "terrainfeatures\\mushroom_tree": // from Tree
                    return this.ReloadTreeTextures(content, key, Tree.mushroomTree);

                case "terrainfeatures\\tree_palm": // from Tree
                    return this.ReloadTreeTextures(content, key, Tree.palmTree);

                case "terrainfeatures\\tree1_fall": // from Tree
                case "terrainfeatures\\tree1_spring": // from Tree
                case "terrainfeatures\\tree1_summer": // from Tree
                case "terrainfeatures\\tree1_winter": // from Tree
                    return this.ReloadTreeTextures(content, key, Tree.bushyTree);

                case "terrainfeatures\\tree2_fall": // from Tree
                case "terrainfeatures\\tree2_spring": // from Tree
                case "terrainfeatures\\tree2_summer": // from Tree
                case "terrainfeatures\\tree2_winter": // from Tree
                    return this.ReloadTreeTextures(content, key, Tree.leafyTree);

                case "terrainfeatures\\tree3_fall": // from Tree
                case "terrainfeatures\\tree3_spring": // from Tree
                case "terrainfeatures\\tree3_winter": // from Tree
                    return this.ReloadTreeTextures(content, key, Tree.pineTree);
            }

            // dynamic textures
            if (this.IsInFolder(key, "Buildings"))
                return this.ReloadBuildings(content, key);

            if (this.IsInFolder(key, "Characters"))
                return this.ReloadNpcSprites(content, key, monster: false);

            if (this.IsInFolder(key, "Characters\\Monsters"))
                return this.ReloadNpcSprites(content, key, monster: true);

            if (key.StartsWith(this.GetNormalisedPath("LooseSprites\\Fence"), StringComparison.InvariantCultureIgnoreCase))
                return this.ReloadFenceTextures(content, key);

            if (this.IsInFolder(key, "Portraits"))
                return this.ReloadNpcPortraits(content, key);

            return false;
        }


        /*********
        ** Private methods
        *********/
        /****
        ** Reload methods
        ****/
        /// <summary>Reload building textures.</summary>
        /// <param name="content">The content manager through which to reload the asset.</param>
        /// <param name="key">The asset key to reload.</param>
        /// <returns>Returns whether any textures were reloaded.</returns>
        private bool ReloadBuildings(LocalizedContentManager content, string key)
        {
            // get buildings
            string type = Path.GetFileName(key);
            Building[] buildings = Game1.locations
                .OfType<BuildableGameLocation>()
                .SelectMany(p => p.buildings)
                .Where(p => p.buildingType == type)
                .ToArray();

            // reload buildings
            if (buildings.Any())
            {
                Lazy<Texture2D> texture = new Lazy<Texture2D>(() => content.Load<Texture2D>(key));
                foreach (Building building in buildings)
#if STARDEW_VALLEY_1_3
                    building.texture = texture;
#else
                    building.texture = texture.Value;
#endif
                return true;
            }
            return false;
        }

        /// <summary>Reload the sprites for a fence type.</summary>
        /// <param name="content">The content manager through which to reload the asset.</param>
        /// <param name="key">The asset key to reload.</param>
        /// <returns>Returns whether any textures were reloaded.</returns>
        private bool ReloadFenceTextures(LocalizedContentManager content, string key)
        {
            // get fence type
            if (!int.TryParse(this.GetSegments(key)[1].Substring("Fence".Length), out int fenceType))
                return false;

            // get fences
            Fence[] fences =
                (
                    from location in this.GetLocations()
                    from fence in location.Objects.Values.OfType<Fence>()
                    where fenceType == 1
                        ? fence.isGate
                        : fence.whichType == fenceType
                    select fence
                )
                .ToArray();

            // update fence textures
            foreach (Fence fence in fences)
                fence.reloadSprite();
            return true;
        }

        /// <summary>Reload the sprites for matching NPCs.</summary>
        /// <param name="content">The content manager through which to reload the asset.</param>
        /// <param name="key">The asset key to reload.</param>
        /// <param name="monster">Whether to match monsters (<c>true</c>) or non-monsters (<c>false</c>).</param>
        /// <returns>Returns whether any textures were reloaded.</returns>
        private bool ReloadNpcSprites(LocalizedContentManager content, string key, bool monster)
        {
            // get NPCs
            string name = this.GetNpcNameFromFileName(Path.GetFileName(key));
            NPC[] characters =
                (
                    from location in this.GetLocations()
                    from npc in location.characters
                    where npc.name == name && npc.IsMonster == monster
                    select npc
                )
                .Distinct()
                .ToArray();
            if (!characters.Any())
                return false;

            // update portrait
            Texture2D texture = content.Load<Texture2D>(key);
            foreach (NPC character in characters)
#if STARDEW_VALLEY_1_3
                this.Reflection.GetField<Texture2D>(character.Sprite, "spriteTexture").SetValue(texture);
#else
                character.Sprite.Texture = texture;
#endif
            return true;
        }

        /// <summary>Reload the portraits for matching NPCs.</summary>
        /// <param name="content">The content manager through which to reload the asset.</param>
        /// <param name="key">The asset key to reload.</param>
        /// <returns>Returns whether any textures were reloaded.</returns>
        private bool ReloadNpcPortraits(LocalizedContentManager content, string key)
        {
            // get NPCs
            string name = this.GetNpcNameFromFileName(Path.GetFileName(key));
            NPC[] villagers =
                (
                    from location in this.GetLocations()
                    from npc in location.characters
                    where npc.name == name && npc.isVillager()
                    select npc
                )
                .Distinct()
                .ToArray();
            if (!villagers.Any())
                return false;

            // update portrait
            Texture2D texture = content.Load<Texture2D>(key);
            foreach (NPC villager in villagers)
                villager.Portrait = texture;
            return true;
        }

        /// <summary>Reload tree textures.</summary>
        /// <param name="content">The content manager through which to reload the asset.</param>
        /// <param name="key">The asset key to reload.</param>
        /// <param name="type">The type to reload.</param>
        /// <returns>Returns whether any textures were reloaded.</returns>
        private bool ReloadTreeTextures(LocalizedContentManager content, string key, int type)
        {
            Tree[] trees = Game1.locations
                .SelectMany(p => p.terrainFeatures.Values.OfType<Tree>())
                .Where(tree => tree.treeType == type)
                .ToArray();

            if (trees.Any())
            {
                Lazy<Texture2D> texture = new Lazy<Texture2D>(() => content.Load<Texture2D>(key));
                foreach (Tree tree in trees)
#if STARDEW_VALLEY_1_3
                    this.Reflection.GetField<Lazy<Texture2D>>(tree, "texture").SetValue(texture);
#else
                    this.Reflection.GetField<Texture2D>(tree, "texture").SetValue(texture.Value);
#endif
                return true;
            }

            return false;
        }

        /****
        ** Helpers
        ****/
        /// <summary>Get an NPC name from the name of their file under <c>Content/Characters</c>.</summary>
        /// <param name="name">The file name.</param>
        /// <remarks>Derived from <see cref="NPC.reloadSprite"/>.</remarks>
        private string GetNpcNameFromFileName(string name)
        {
            switch (name)
            {
                case "Mariner":
                    return "Old Mariner";
                case "DwarfKing":
                    return "Dwarf King";
                case "MrQi":
                    return "Mister Qi";
                default:
                    return name;
            }
        }

        /// <summary>Get all locations in the game.</summary>
        private IEnumerable<GameLocation> GetLocations()
        {
            foreach (GameLocation location in Game1.locations)
            {
                yield return location;

                if (location is BuildableGameLocation buildableLocation)
                {
                    foreach (Building building in buildableLocation.buildings)
                    {
                        if (building.indoors != null)
                            yield return building.indoors;
                    }
                }
            }
        }

        /// <summary>Get whether a normalised asset key is in the given folder.</summary>
        /// <param name="key">The normalised asset key (like <c>Animals/cat</c>).</param>
        /// <param name="folder">The key folder (like <c>Animals</c>); doesn't need to be normalised.</param>
        /// <param name="allowSubfolders">Whether to return true if the key is inside a subfolder of the <paramref name="folder"/>.</param>
        private bool IsInFolder(string key, string folder, bool allowSubfolders = false)
        {
            return
                key.StartsWith(this.GetNormalisedPath($"{folder}\\"), StringComparison.InvariantCultureIgnoreCase)
                && (allowSubfolders || this.CountSegments(key) == this.CountSegments(folder) + 1);
        }

        /// <summary>Get the segments in a path (e.g. 'a/b' is 'a' and 'b').</summary>
        /// <param name="path">The path to check.</param>
        private string[] GetSegments(string path)
        {
            if (path == null)
                return new string[0];
            return path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        /// <summary>Count the number of segments in a path (e.g. 'a/b' is 2).</summary>
        /// <param name="path">The path to check.</param>
        private int CountSegments(string path)
        {
            return this.GetSegments(path).Length;
        }
    }
}
