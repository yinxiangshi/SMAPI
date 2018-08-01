using Microsoft.AspNetCore.Routing.Constraints;
using StardewModdingAPI.Toolkit;

namespace StardewModdingAPI.Web.Framework
{
    /// <summary>Constrains a route value to a valid semantic version.</summary>
    internal class VersionConstraint : RegexRouteConstraint
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public VersionConstraint()
            : base(SemanticVersion.Regex) { }
    }
}
