using System;
using System.Collections.Generic;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mongo2Go;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using StardewModdingAPI.Toolkit.Serialization;
using StardewModdingAPI.Web.Framework;
using StardewModdingAPI.Web.Framework.Caching;
using StardewModdingAPI.Web.Framework.Caching.Mods;
using StardewModdingAPI.Web.Framework.Caching.Wiki;
using StardewModdingAPI.Web.Framework.Clients.Chucklefish;
using StardewModdingAPI.Web.Framework.Clients.CurseForge;
using StardewModdingAPI.Web.Framework.Clients.GitHub;
using StardewModdingAPI.Web.Framework.Clients.ModDrop;
using StardewModdingAPI.Web.Framework.Clients.Nexus;
using StardewModdingAPI.Web.Framework.Clients.Pastebin;
using StardewModdingAPI.Web.Framework.Compression;
using StardewModdingAPI.Web.Framework.ConfigModels;
using StardewModdingAPI.Web.Framework.RewriteRules;
using StardewModdingAPI.Web.Framework.Storage;

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
        public Startup(IWebHostEnvironment env)
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
            // init basic services
            services
                .Configure<ApiClientsConfig>(this.Configuration.GetSection("ApiClients"))
                .Configure<BackgroundServicesConfig>(this.Configuration.GetSection("BackgroundServices"))
                .Configure<ModCompatibilityListConfig>(this.Configuration.GetSection("ModCompatibilityList"))
                .Configure<ModUpdateCheckConfig>(this.Configuration.GetSection("ModUpdateCheck"))
                .Configure<StorageConfig>(this.Configuration.GetSection("Storage"))
                .Configure<SiteConfig>(this.Configuration.GetSection("Site"))
                .Configure<RouteOptions>(options => options.ConstraintMap.Add("semanticVersion", typeof(VersionConstraint)))
                .AddLogging()
                .AddMemoryCache();
            StorageConfig storageConfig = this.Configuration.GetSection("Storage").Get<StorageConfig>();
            StorageMode storageMode = storageConfig.Mode;

            // init MVC
            services
                .AddControllers()
                .AddNewtonsoftJson(options => this.ConfigureJsonNet(options.SerializerSettings))
                .ConfigureApplicationPartManager(manager => manager.FeatureProviders.Add(new InternalControllerFeatureProvider()));
            services
                .AddRazorPages();

            // init storage
            switch (storageMode)
            {
                case StorageMode.InMemory:
                    services.AddSingleton<IModCacheRepository>(new ModCacheMemoryRepository());
                    services.AddSingleton<IWikiCacheRepository>(new WikiCacheMemoryRepository());
                    break;

                case StorageMode.Mongo:
                case StorageMode.MongoInMemory:
                    {
                        // local MongoDB instance
                        services.AddSingleton<MongoDbRunner>(_ => storageMode == StorageMode.MongoInMemory
                            ? MongoDbRunner.Start()
                            : throw new NotSupportedException($"The in-memory MongoDB runner isn't available in storage mode {storageMode}.")
                        );

                        // MongoDB
                        services.AddSingleton<IMongoDatabase>(serv =>
                        {
                            BsonSerializer.RegisterSerializer(new UtcDateTimeOffsetSerializer());
                            return new MongoClient(this.GetMongoDbConnectionString(serv, storageConfig))
                                .GetDatabase(storageConfig.Database);
                        });

                        // repositories
                        services.AddSingleton<IModCacheRepository>(serv => new ModCacheMongoRepository(serv.GetRequiredService<IMongoDatabase>()));
                        services.AddSingleton<IWikiCacheRepository>(serv => new WikiCacheMongoRepository(serv.GetRequiredService<IMongoDatabase>()));
                    }
                    break;

                default:
                    throw new NotSupportedException($"Unhandled storage mode '{storageMode}'.");
            }

            // init Hangfire
            services
                .AddHangfire((serv, config) =>
                {
                    config
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings();

                    switch (storageMode)
                    {
                        case StorageMode.InMemory:
                            config.UseMemoryStorage();
                            break;

                        case StorageMode.MongoInMemory:
                        case StorageMode.Mongo:
                            string connectionString = this.GetMongoDbConnectionString(serv, storageConfig);
                            config.UseMongoStorage(MongoClientSettings.FromConnectionString(connectionString), $"{storageConfig.Database}-hangfire", new MongoStorageOptions
                            {
                                MigrationOptions = new MongoMigrationOptions(MongoMigrationStrategy.Drop),
                                CheckConnection = false // error on startup takes down entire process
                            });
                            break;
                    }
                });

            // init background service
            {
                BackgroundServicesConfig config = this.Configuration.GetSection("BackgroundServices").Get<BackgroundServicesConfig>();
                if (config.Enabled)
                    services.AddHostedService<BackgroundService>();
            }

            // init API clients
            {
                ApiClientsConfig api = this.Configuration.GetSection("ApiClients").Get<ApiClientsConfig>();
                string version = this.GetType().Assembly.GetName().Version.ToString(3);
                string userAgent = string.Format(api.UserAgent, version);

                services.AddSingleton<IChucklefishClient>(new ChucklefishClient(
                    userAgent: userAgent,
                    baseUrl: api.ChucklefishBaseUrl,
                    modPageUrlFormat: api.ChucklefishModPageUrlFormat
                ));

                services.AddSingleton<ICurseForgeClient>(new CurseForgeClient(
                    userAgent: userAgent,
                    apiUrl: api.CurseForgeBaseUrl
                ));

                services.AddSingleton<IGitHubClient>(new GitHubClient(
                    baseUrl: api.GitHubBaseUrl,
                    userAgent: userAgent,
                    acceptHeader: api.GitHubAcceptHeader,
                    username: api.GitHubUsername,
                    password: api.GitHubPassword
                ));

                services.AddSingleton<IModDropClient>(new ModDropClient(
                    userAgent: userAgent,
                    apiUrl: api.ModDropApiUrl,
                    modUrlFormat: api.ModDropModPageUrl
                ));

                services.AddSingleton<INexusClient>(new NexusClient(
                    webUserAgent: userAgent,
                    webBaseUrl: api.NexusBaseUrl,
                    webModUrlFormat: api.NexusModUrlFormat,
                    webModScrapeUrlFormat: api.NexusModScrapeUrlFormat,
                    apiAppVersion: version,
                    apiKey: api.NexusApiKey
                ));

                services.AddSingleton<IPastebinClient>(new PastebinClient(
                    baseUrl: api.PastebinBaseUrl,
                    userAgent: userAgent
                ));
            }

            // init helpers
            services
                .AddSingleton<IGzipHelper>(new GzipHelper())
                .AddSingleton<IStorageProvider>(serv => new StorageProvider(
                    serv.GetRequiredService<IOptions<ApiClientsConfig>>(),
                    serv.GetRequiredService<IPastebinClient>(),
                    serv.GetRequiredService<IGzipHelper>()
                ));
        }

        /// <summary>The method called by the runtime to configure the HTTP request pipeline.</summary>
        /// <param name="app">The application builder.</param>
        public void Configure(IApplicationBuilder app)
        {
            // basic config
            app.UseDeveloperExceptionPage();
            app
                .UseCors(policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("https://smapi.io")
                )
                .UseRewriter(this.GetRedirectRules())
                .UseStaticFiles() // wwwroot folder
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(p =>
                {
                    p.MapControllers();
                    p.MapRazorPages();
                });

            // enable Hangfire dashboard
            app.UseHangfireDashboard("/tasks", new DashboardOptions
            {
                IsReadOnlyFunc = context => !JobDashboardAuthorizationFilter.IsLocalRequest(context),
                Authorization = new[] { new JobDashboardAuthorizationFilter() }
            });
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Configure a Json.NET serializer.</summary>
        /// <param name="settings">The serializer settings to edit.</param>
        private void ConfigureJsonNet(JsonSerializerSettings settings)
        {
            foreach (JsonConverter converter in new JsonHelper().JsonSettings.Converters)
                settings.Converters.Add(converter);

            settings.Formatting = Formatting.Indented;
            settings.NullValueHandling = NullValueHandling.Ignore;
        }

        /// <summary>Get the MongoDB connection string for the given storage configuration.</summary>
        /// <param name="services">The service provider.</param>
        /// <param name="storageConfig">The storage configuration</param>
        /// <exception cref="NotSupportedException">There's no MongoDB instance in the given storage mode.</exception>
        private string GetMongoDbConnectionString(IServiceProvider services, StorageConfig storageConfig)
        {
            return storageConfig.Mode switch
            {
                StorageMode.Mongo => storageConfig.ConnectionString,
                StorageMode.MongoInMemory => services.GetRequiredService<MongoDbRunner>().ConnectionString,
                _ => throw new NotSupportedException($"There's no MongoDB instance in storage mode {storageConfig.Mode}.")
            };
        }

        /// <summary>Get the redirect rules to apply.</summary>
        private RewriteOptions GetRedirectRules()
        {
            var redirects = new RewriteOptions();

            // redirect to HTTPS (except API for Linux/Mac Mono compatibility)
            redirects.Add(new ConditionalRedirectToHttpsRule(
                shouldRewrite: req =>
                    req.Host.Host != "localhost"
                    && !req.Path.StartsWithSegments("/api")
            ));

            // shortcut redirects
            redirects.Add(new RedirectToUrlRule(@"^/3\.0\.?$", "https://stardewvalleywiki.com/Modding:Migrate_to_SMAPI_3.0"));
            redirects.Add(new RedirectToUrlRule(@"^/(?:buildmsg|package)(?:/?(.*))$", "https://github.com/Pathoschild/SMAPI/blob/develop/docs/technical/mod-package.md#$1")); // buildmsg deprecated, remove when SDV 1.4 is released
            redirects.Add(new RedirectToUrlRule(@"^/community\.?$", "https://stardewvalleywiki.com/Modding:Community"));
            redirects.Add(new RedirectToUrlRule(@"^/compat\.?$", "https://smapi.io/mods"));
            redirects.Add(new RedirectToUrlRule(@"^/docs\.?$", "https://stardewvalleywiki.com/Modding:Index"));
            redirects.Add(new RedirectToUrlRule(@"^/install\.?$", "https://stardewvalleywiki.com/Modding:Player_Guide/Getting_Started#Install_SMAPI"));
            redirects.Add(new RedirectToUrlRule(@"^/troubleshoot(.*)$", "https://stardewvalleywiki.com/Modding:Player_Guide/Troubleshooting$1"));
            redirects.Add(new RedirectToUrlRule(@"^/xnb\.?$", "https://stardewvalleywiki.com/Modding:Using_XNB_mods"));

            // redirect legacy canimod.com URLs
            var wikiRedirects = new Dictionary<string, string[]>
            {
                ["Modding:Index#Migration_guides"] = new[] { "^/for-devs/updating-a-smapi-mod", "^/guides/updating-a-smapi-mod" },
                ["Modding:Modder_Guide"] = new[] { "^/for-devs/creating-a-smapi-mod", "^/guides/creating-a-smapi-mod", "^/for-devs/creating-a-smapi-mod-advanced-config" },
                ["Modding:Player_Guide"] = new[] { "^/for-players/install-smapi", "^/guides/using-mods", "^/for-players/faqs", "^/for-players/intro", "^/for-players/use-mods", "^/guides/asking-for-help", "^/guides/smapi-faq" },

                ["Modding:Editing_XNB_files"] = new[] { "^/for-devs/creating-an-xnb-mod", "^/guides/creating-an-xnb-mod" },
                ["Modding:Event_data"] = new[] { "^/for-devs/events", "^/guides/events" },
                ["Modding:Gift_taste_data"] = new[] { "^/for-devs/npc-gift-tastes", "^/guides/npc-gift-tastes" },
                ["Modding:IDE_reference"] = new[] { "^/for-devs/creating-a-smapi-mod-ide-primer" },
                ["Modding:Object_data"] = new[] { "^/for-devs/object-data", "^/guides/object-data" },
                ["Modding:Weather_data"] = new[] { "^/for-devs/weather", "^/guides/weather" }
            };
            foreach ((string page, string[] patterns) in wikiRedirects)
            {
                foreach (string pattern in patterns)
                    redirects.Add(new RedirectToUrlRule(pattern, "https://stardewvalleywiki.com/" + page));
            }

            return redirects;
        }
    }
}
