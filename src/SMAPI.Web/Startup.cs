using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StardewModdingAPI.Web.Framework;
using StardewModdingAPI.Web.Framework.ConfigModels;
using StardewModdingAPI.Web.Framework.RewriteRules;

namespace StardewModdingAPI.Web
{
    /// <summary>The web app startup configuration.</summary>
    internal class Startup
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
                .Add(new BeanstalkEnvPropsConfigProvider())
                .Build();
        }

        /// <summary>The method called by the runtime to add services to the container.</summary>
        /// <param name="services">The service injection container.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .Configure<ModUpdateCheckConfig>(this.Configuration.GetSection("ModUpdateCheck"))
                .Configure<LogParserConfig>(this.Configuration.GetSection("LogParser"))
                .Configure<RouteOptions>(options => options.ConstraintMap.Add("semanticVersion", typeof(VersionConstraint)))
                .AddMemoryCache()
                .AddMvc()
                .ConfigureApplicationPartManager(manager => manager.FeatureProviders.Add(new InternalControllerFeatureProvider()))
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

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app
                .UseCors(policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("https://smapi.io", "https://*.smapi.io", "https://*.edge.smapi.io")
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                )
                .UseRewriter(new RewriteOptions()
                    // redirect to HTTPS (except API for Linux/Mac Mono compatibility)
                    .Add(new ConditionalRedirectToHttpsRule(
                        shouldRewrite: req =>
                            req.Host.Host != "localhost"
                            && !req.Path.StartsWithSegments("/api")
                            && !req.Host.Host.StartsWith("api.")
                    ))

                    // convert subdomain.smapi.io => smapi.io/subdomain for routing
                    .Add(new ConditionalRewriteSubdomainRule(
                        shouldRewrite: req =>
                            req.Host.Host != "localhost"
                            && (req.Host.Host.StartsWith("api.") || req.Host.Host.StartsWith("log."))
                            && !req.Path.StartsWithSegments("/content")
                            && !req.Path.StartsWithSegments("/favicon.ico")
                    ))

                    // shortcut redirects
                    .Add(new RedirectToUrlRule("^/docs$", "https://stardewvalleywiki.com/Modding:Index"))
                    .Add(new RedirectToUrlRule("^/install$", "https://stardewvalleywiki.com/Modding:Installing_SMAPI"))
                )
                .UseStaticFiles() // wwwroot folder
                .UseMvc();
        }
    }
}
