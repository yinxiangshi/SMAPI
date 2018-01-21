using StardewModdingAPI.Framework.Models;

namespace StardewModdingAPI.Framework.Serialisation.SmapiConverters
{
    /// <summary>Handles deserialisation of <see cref="ModDataID"/>.</summary>
    internal class ModDataIdConverter : SimpleReadOnlyConverter<ModDataID>
    {
        /*********
        ** Protected methods
        *********/
        /// <summary>Read a JSON string.</summary>
        /// <param name="str">The JSON string value.</param>
        /// <param name="path">The path to the current JSON node.</param>
        protected override ModDataID ReadString(string str, string path)
        {
            return new ModDataID(str);
        }
    }
}
