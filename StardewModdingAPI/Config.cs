/*
    Copyright 2016 Zoey (Zoryn)
*/

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StardewModdingAPI
{
    public class Config
    {
        [JsonIgnore]
        public virtual JObject JObject { get; protected set; }

        [JsonIgnore]
        public virtual string ConfigLocation { get; protected set; }

        public static Config Instance
        {
            get { return new Config(); }
        }

        public static Config InitializeConfig(string configLocation, Config baseConfig)
        {
            if (string.IsNullOrEmpty(configLocation))
            {
                Log.Verbose("The location to save the config to must not be empty.");
                return null;
            }

            if (baseConfig == null)
            {
                Log.Verbose("A config must be instantiated before being passed to Initialize.\n\t" + configLocation);
                return null;
            }

            baseConfig.ConfigLocation = configLocation;
            return baseConfig.LoadConfig(baseConfig);
        }

        public virtual Config GenerateBaseConfig(Config baseConfig)
        {
            //Must be implemented in sub-class
            return null;
        }

        public virtual Config LoadConfig(Config baseConfig)
        {
            if (!File.Exists(baseConfig.ConfigLocation))
            {
                var v = (Config)baseConfig.GetType().GetMethod("GenerateBaseConfig", BindingFlags.Public | BindingFlags.Instance).Invoke(baseConfig, new object[] { baseConfig });
                v.WriteConfig();
            }
            else
            {
                var p = baseConfig.ConfigLocation;

                try
                {
                    var j = JObject.Parse(File.ReadAllText(baseConfig.ConfigLocation));
                    baseConfig = (Config)j.ToObject(baseConfig.GetType());
                    baseConfig.ConfigLocation = p;
                    baseConfig.JObject = j;

                    baseConfig = UpdateConfig(baseConfig);
                    baseConfig.ConfigLocation = p;
                    baseConfig.JObject = j;

                    baseConfig.WriteConfig();
                }
                catch
                {
                    Log.Verbose("Invalid JSON Renamed: " + p);
                    if (File.Exists(p))
                        File.Move(p, Path.Combine(Path.GetDirectoryName(p), Path.GetFileNameWithoutExtension(p) + "." + Guid.NewGuid() + ".json")); //Get it out of the way for a new one
                    var v = (Config)baseConfig.GetType().GetMethod("GenerateBaseConfig", BindingFlags.Public | BindingFlags.Instance).Invoke(baseConfig, new object[] { baseConfig });
                    v.WriteConfig();
                }
            }

            return baseConfig;
        }

        public virtual Config UpdateConfig(Config baseConfig)
        {
            try
            {
                //default config with all standard values
                var b = JObject.FromObject(baseConfig.GetType().GetMethod("GenerateBaseConfig", BindingFlags.Public | BindingFlags.Instance).Invoke(baseConfig, new object[] { baseConfig }));
                //user config with their values
                var u = baseConfig.JObject;

                b.Merge(u, new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace });

                return (Config)b.ToObject(baseConfig.GetType());
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            return baseConfig;
        }

        /// <summary>
        /// NOTICE: THIS IS OBSOLETE AND WILL BE REMOVED IN THE FUTURE. 'BaseConfigPath' IS NOW A PROPERTY IN A MOD
        /// </summary>
        /// <param name="theMod"></param>
        /// <returns></returns>
        [Obsolete]
        public static string GetBasePath(Mod theMod)
        {
            return theMod.BaseConfigPath;
        }
    }

    public static class ConfigExtensions
    {
        public static void WriteConfig(this Config baseConfig)
        {
            if (baseConfig == null || string.IsNullOrEmpty(baseConfig.ConfigLocation) || string.IsNullOrEmpty(Path.GetDirectoryName(baseConfig.ConfigLocation)))
            {
                Log.Error("A config attempted to save when it itself or it's location were null.");
                return;
            }

            var toWrite = JsonConvert.SerializeObject(baseConfig, baseConfig.GetType(), Formatting.Indented, new JsonSerializerSettings());
            if (!Directory.Exists(Path.GetDirectoryName(baseConfig.ConfigLocation)))
                Directory.CreateDirectory(Path.GetDirectoryName(baseConfig.ConfigLocation));
            if (!File.Exists(baseConfig.ConfigLocation) || !File.ReadAllText(baseConfig.ConfigLocation).SequenceEqual(toWrite))
                File.WriteAllText(baseConfig.ConfigLocation, toWrite);
            toWrite = null;
        }

        public static Config ReloadConfig(this Config baseConfig)
        {
            return baseConfig.UpdateConfig(baseConfig);
        }
    }
}