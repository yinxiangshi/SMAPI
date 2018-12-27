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
        ** Fields
        *********/
        /// <summary>Get the new URL to which to redirect (or <c>null</c> to skip).</summary>
        private readonly Func<HttpRequest, string> NewUrl;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="shouldRewrite">A predicate which indicates when the rule should be applied.</param>
        /// <param name="url">The new URL to which to redirect.</param>
        public RedirectToUrlRule(Func<HttpRequest, bool> shouldRewrite, string url)
        {
            this.NewUrl = req => shouldRewrite(req) ? url : null;
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="pathRegex">A case-insensitive regex to match against the path.</param>
        /// <param name="url">The external URL.</param>
        public RedirectToUrlRule(string pathRegex, string url)
        {
            Regex regex = new Regex(pathRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            this.NewUrl = req => req.Path.HasValue ? regex.Replace(req.Path.Value, url) : null;
        }

        /// <summary>Applies the rule. Implementations of ApplyRule should set the value for <see cref="RewriteContext.Result" /> (defaults to RuleResult.ContinueRules).</summary>
        /// <param name="context">The rewrite context.</param>
        public void ApplyRule(RewriteContext context)
        {
            HttpRequest request = context.HttpContext.Request;

            // check rewrite
            string newUrl = this.NewUrl(request);
            if (newUrl == null || newUrl == request.Path.Value)
                return;

            // redirect request
            HttpResponse response = context.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.Redirect;
            response.Headers["Location"] = newUrl;
            context.Result = RuleResult.EndResponse;
        }
    }
}
