using Microsoft.AspNetCore.Routing.Constraints;

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
            : base(@"^v(?>(?<major>0|[1-9]\d*))\.(?>(?<minor>0|[1-9]\d*))(?>(?:\.(?<patch>0|[1-9]\d*))?)(?:-(?<prerelease>(?>[a-z0-9]+[\-\.]?)+))?$") { }
    }
}
