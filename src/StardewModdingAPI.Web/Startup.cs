using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StardewModdingAPI.Web.Framework;

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
                .AddMvc(options =>
                {
                    // add support for comma-delimited parameters
                    ArrayModelBinderProvider arrayModelBinderProvider = options.ModelBinderProviders.OfType<ArrayModelBinderProvider>().First();
                    options.ModelBinderProviders.Insert(options.ModelBinderProviders.IndexOf(arrayModelBinderProvider), new CommaDelimitedModelBinderProvider());
                })
                .AddJsonOptions(options =>
                {
                    // suppress null values in JSON responses
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
            app.UseMvc();
        }
    }
}
