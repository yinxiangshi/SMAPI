using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace StardewModdingAPI.Inheritance
{
    public class SGameLocation : GameLocation
    {
        public GameLocation BaseGameLocation { get; private set; }

        public SerializableDictionary<Vector2, SObject> ModObjects { get; set; }

        public static SGameLocation ConstructFromBaseClass(GameLocation baseClass)
        {
            SGameLocation s = new SGameLocation();
            s.BaseGameLocation = baseClass;
            s.ModObjects = new SerializableDictionary<Vector2, SObject>();
            //s.IsFarm = baseClass.IsFarm;
            //s.IsOutdoors = baseClass.IsOutdoors;
            //s.LightLevel = baseClass.LightLevel;
            //s.Map = baseClass.Map;
            //s.objects = baseClass.objects;
            //s.temporarySprites = baseClass.temporarySprites;
            s.actionObjectForQuestionDialogue = baseClass.actionObjectForQuestionDialogue;
            s.characters = baseClass.characters;
            s.critters = (List<Critter>)typeof(GameLocation).GetField("critters", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(baseClass);
            s.currentEvent = baseClass.currentEvent;
            s.debris = baseClass.debris;
            s.doorSprites = baseClass.doorSprites;
            s.doors = baseClass.doors;
            s.farmers = baseClass.farmers;
            s.fishSplashAnimation = baseClass.fishSplashAnimation;
            s.fishSplashPoint = baseClass.fishSplashPoint;
            s.forceViewportPlayerFollow = baseClass.forceViewportPlayerFollow;
            s.ignoreDebrisWeather = baseClass.ignoreDebrisWeather;
            s.ignoreLights = baseClass.ignoreLights;
            s.ignoreOutdoorLighting = baseClass.ignoreOutdoorLighting;
            s.isFarm = baseClass.isFarm;
            s.isOutdoors = baseClass.isOutdoors;
            s.isStructure = baseClass.isStructure;
            s.largeTerrainFeatures = baseClass.largeTerrainFeatures;
            s.lastQuestionKey = baseClass.lastQuestionKey;
            s.lastTouchActionLocation = baseClass.lastTouchActionLocation;
            s.lightGlows = baseClass.lightGlows;
            s.map = baseClass.map;
            s.name = baseClass.name;
            s.numberOfSpawnedObjectsOnMap = baseClass.numberOfSpawnedObjectsOnMap;
            s.objects = baseClass.objects;
            s.orePanAnimation = baseClass.orePanAnimation;
            s.orePanPoint = baseClass.orePanPoint;
            s.projectiles = baseClass.projectiles;
            s.temporarySprites = baseClass.temporarySprites;
            s.terrainFeatures = baseClass.terrainFeatures;
            s.uniqueName = baseClass.uniqueName;
            s.warps = baseClass.warps;
            s.wasUpdated = (bool)typeof(GameLocation).GetField("wasUpdated", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(baseClass);
            s.waterAnimationIndex = baseClass.waterAnimationIndex;
            s.waterAnimationTimer = baseClass.waterAnimationTimer;
            s.waterColor = baseClass.waterColor;
            s.waterTileFlip = baseClass.waterTileFlip;
            s.waterTiles = baseClass.waterTiles;
            return s;
        }

        public static List<SGameLocation> ConvertGameLocations(List<GameLocation> baseGameLocations)
        {
            return baseGameLocations.Select(ConstructFromBaseClass).ToList();
        }

        public virtual void update(GameTime gameTime)
        {
        }

        public override void draw(SpriteBatch b)
        {
            foreach (var v in ModObjects)
            {
                v.Value.draw(b, (int)v.Key.X, (int)v.Key.Y, 0.999f, 1);
            }
        }
    }
}
