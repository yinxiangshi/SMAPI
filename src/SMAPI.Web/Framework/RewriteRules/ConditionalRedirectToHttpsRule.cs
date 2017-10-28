using System;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;

namespace StardewModdingAPI.Web.Framework.RewriteRules
{
    /// <summary>Redirect requests to HTTPS.</summary>
    /// <remarks>Derived from <a href="https://stackoverflow.com/a/44526747/262123" /> and <see cref="Microsoft.AspNetCore.Rewrite.Internal.RedirectToHttpsRule"/>.</remarks>
    internal class ConditionalRedirectToHttpsRule : IRule
    {
        /*********
        ** Properties
        *********/
        /// <summary>A predicate which indicates when the rule should be applied.</summary>
        private readonly Func<HttpRequest, bool> ShouldRewrite;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="shouldRewrite">A predicate which indicates when the rule should be applied.</param>
        public ConditionalRedirectToHttpsRule(Func<HttpRequest, bool> shouldRewrite = null)
        {
            this.ShouldRewrite = shouldRewrite ?? (req => true);
        }

        /// <summary>Applies the rule. Implementations of ApplyRule should set the value for <see cref="RewriteContext.Result" /> (defaults to RuleResult.ContinueRules).</summary>
        /// <param name="context">The rewrite context.</param>
        public void ApplyRule(RewriteContext context)
        {
            HttpRequest request = context.HttpContext.Request;

            // check condition
            if (this.IsSecure(request) || !this.ShouldRewrite(request))
                return;

            // redirect request
            HttpResponse response = context.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.RedirectKeepVerb;
            response.Headers["Location"] = new StringBuilder()
                .Append("https://")
                .Append(request.Host.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
            context.Result = RuleResult.EndResponse;
        }

        /// <summary>Get whether the request was received over HTTPS.</summary>
        /// <param name="request">The request to check.</param>
        public bool IsSecure(HttpRequest request)
        {
            return
                request.IsHttps // HTTPS to server
                || string.Equals(request.Headers["x-forwarded-proto"], "HTTPS", StringComparison.OrdinalIgnoreCase); // HTTPS to AWS load balancer
        }
    }
}
