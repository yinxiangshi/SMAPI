using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;

namespace StardewModdingAPI.Web.Framework.RewriteRules
{
    /// <summary>Rewrite requests to prepend the subdomain portion (if any) to the path.</summary>
    /// <remarks>Derived from <a href="https://stackoverflow.com/a/44526747/262123" />.</remarks>
    internal class ConditionalRewriteSubdomainRule : IRule
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A predicate which indicates when the rule should be applied.</summary>
        private readonly Func<HttpRequest, bool> ShouldRewrite;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="shouldRewrite">A predicate which indicates when the rule should be applied.</param>
        public ConditionalRewriteSubdomainRule(Func<HttpRequest, bool> shouldRewrite = null)
        {
            this.ShouldRewrite = shouldRewrite;
        }

        /// <summary>Applies the rule. Implementations of ApplyRule should set the value for <see cref="RewriteContext.Result" /> (defaults to RuleResult.ContinueRules).</summary>
        /// <param name="context">The rewrite context.</param>
        public void ApplyRule(RewriteContext context)
        {
            HttpRequest request = context.HttpContext.Request;

            // check condition
            if (this.ShouldRewrite != null && !this.ShouldRewrite(request))
                return;

            // get host parts
            string host = request.Host.Host;
            string[] parts = host.Split('.');
            if (parts.Length < 2)
                return;

            // prepend to path
            request.Path = $"/{parts[0]}{request.Path}";
        }
    }
}
