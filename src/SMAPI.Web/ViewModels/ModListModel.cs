using System.Collections.Generic;
using System.Linq;

namespace StardewModdingAPI.Web.ViewModels
{
    /// <summary>Metadata for the mod list page.</summary>
    public class ModListModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The current stable version of the game.</summary>
        public string StableVersion { get; set; }

        /// <summary>The current beta version of the game (if any).</summary>
        public string BetaVersion { get; set; }

        /// <summary>The mods to display.</summary>
        public ModModel[] Mods { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="stableVersion">The current stable version of the game.</param>
        /// <param name="betaVersion">The current beta version of the game (if any).</param>
        /// <param name="mods">The mods to display.</param>
        public ModListModel(string stableVersion, string betaVersion, IEnumerable<ModModel> mods)
        {
            this.StableVersion = stableVersion;
            this.BetaVersion = betaVersion;
            this.Mods = mods.ToArray();
        }
    }
}
