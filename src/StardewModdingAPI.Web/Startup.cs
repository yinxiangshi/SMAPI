using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StardewModdingAPI.Web.Framework;
using StardewModdingAPI.Web.Framework.ConfigModels;

namespace StardewModdingAPI.Web
{
    /// <summary>The web app startup configuration.</summary>
    public class Startup
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The web app configuration.</summary>
        public IConfigurationRoot Configuration { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="env">The hosting environment.</param>
        public Startup(IHostingEnvironment env)
        {
            this.Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        /// <summary>The method called by the runtime to add services to the container.</summary>
        /// <param name="services">The service injection container.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .Configure<ModUpdateCheckConfig>(this.Configuration.GetSection("ModUpdateCheck"))
                .AddMemoryCache()
                .AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Formatting = Formatting.Indented;
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });
        }

        /// <summary>The method called by the runtime to configure the HTTP request pipeline.</summary>
        /// <param name="app">The application builder.</param>
        /// <param name="env">The hosting environment.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            app
                .UseRewriter(new RewriteOptions().Add(new RewriteSubdomainRule())) // convert subdomain.smapi.io => smapi.io/subdomain for routing
                .UseMvc(route =>
                {
                    route.MapRoute(
                        name: "API",
                        template: "api/{version}/{controller}/{action?}",
                        defaults: new
                        {
                            action = "GetAsync"
                        },
                        constraints: new
                        {
                            // version regex from SMAPI's SemanticVersion implementation
                            version = @"^v(?>(?<major>0|[1-9]\d*))\.(?>(?<minor>0|[1-9]\d*))(?>(?:\.(?<patch>0|[1-9]\d*))?)(?:-(?<prerelease>(?>[a-z0-9]+[\-\.]?)+))?$"
                        }
                    );
                });
        }
    }
}
