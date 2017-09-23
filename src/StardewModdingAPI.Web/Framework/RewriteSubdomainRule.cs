using System;
using Microsoft.AspNetCore.Rewrite;

namespace StardewModdingAPI.Web.Framework
{
    /// <summary>Rewrite requests to prepend the subdomain portion (if any) to the path.</summary>
    /// <remarks>Derived from <a href="https://stackoverflow.com/a/44526747/262123" />.</remarks>
    public class RewriteSubdomainRule : IRule
    {
        /// <summary>Applies the rule. Implementations of ApplyRule should set the value for <see cref="RewriteContext.Result" /> (defaults to RuleResult.ContinueRules).</summary>
        /// <param name="context">The rewrite context.</param>
        public void ApplyRule(RewriteContext context)
        {
            context.Result = RuleResult.ContinueRules;

            // get host parts
            string host = context.HttpContext.Request.Host.Host;
            string[] parts = host.Split('.');

            // validate
            if (parts.Length < 2)
                return;
            if (parts.Length < 3 && !"localhost".Equals(parts[1], StringComparison.InvariantCultureIgnoreCase))
                return;

            // prepend to path
            context.HttpContext.Request.Path = $"/{parts[0]}{context.HttpContext.Request.Path}";
        }
    }
}
