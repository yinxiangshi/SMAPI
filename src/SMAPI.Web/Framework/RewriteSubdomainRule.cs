using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;

namespace StardewModdingAPI.Web.Framework
{
    /// <summary>Rewrite requests to prepend the subdomain portion (if any) to the path.</summary>
    /// <remarks>Derived from <a href="https://stackoverflow.com/a/44526747/262123" />.</remarks>
    internal class RewriteSubdomainRule : IRule
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The paths (excluding the hostname portion) to not rewrite.</summary>
        public Regex[] ExceptPaths { get; set; }


        /*********
        ** Public methods
        *********/
        /// <summary>Applies the rule. Implementations of ApplyRule should set the value for <see cref="RewriteContext.Result" /> (defaults to RuleResult.ContinueRules).</summary>
        /// <param name="context">The rewrite context.</param>
        public void ApplyRule(RewriteContext context)
        {
            context.Result = RuleResult.ContinueRules;
            HttpRequest request = context.HttpContext.Request;

            // check ignores
            if (this.ExceptPaths?.Any(pattern => pattern.IsMatch(request.Path)) == true)
                return;

            // get host parts
            string host = request.Host.Host;
            string[] parts = host.Split('.');

            // validate
            if (parts.Length < 2)
                return;
            if (parts.Length < 3 && !"localhost".Equals(parts[1], StringComparison.InvariantCultureIgnoreCase))
                return;

            // prepend to path
            request.Path = $"/{parts[0]}{request.Path}";
        }
    }
}
