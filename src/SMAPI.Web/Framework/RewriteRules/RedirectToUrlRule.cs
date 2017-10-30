using System;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;

namespace StardewModdingAPI.Web.Framework.RewriteRules
{
    /// <summary>Redirect requests to an external URL if they match a condition.</summary>
    internal class RedirectToUrlRule : IRule
    {
        /*********
        ** Properties
        *********/
        /// <summary>A predicate which indicates when the rule should be applied.</summary>
        private readonly Func<HttpRequest, bool> ShouldRewrite;

        /// <summary>The new URL to which to redirect.</summary>
        private readonly string NewUrl;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="shouldRewrite">A predicate which indicates when the rule should be applied.</param>
        /// <param name="url">The new URL to which to redirect.</param>
        public RedirectToUrlRule(Func<HttpRequest, bool> shouldRewrite, string url)
        {
            this.ShouldRewrite = shouldRewrite ?? (req => true);
            this.NewUrl = url;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="pathRegex">A case-insensitive regex to match against the path.</param>
        /// <param name="url">The external URL.</param>
        public RedirectToUrlRule(string pathRegex, string url)
        {
            Regex regex = new Regex(pathRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            this.ShouldRewrite = req => req.Path.HasValue && regex.IsMatch(req.Path.Value);
            this.NewUrl = url;
        }

        /// <summary>Applies the rule. Implementations of ApplyRule should set the value for <see cref="RewriteContext.Result" /> (defaults to RuleResult.ContinueRules).</summary>
        /// <param name="context">The rewrite context.</param>
        public void ApplyRule(RewriteContext context)
        {
            HttpRequest request = context.HttpContext.Request;

            // check condition
            if (!this.ShouldRewrite(request))
                return;

            // redirect request
            HttpResponse response = context.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.Redirect;
            response.Headers["Location"] = this.NewUrl;
            context.Result = RuleResult.EndResponse;
        }
    }
}
