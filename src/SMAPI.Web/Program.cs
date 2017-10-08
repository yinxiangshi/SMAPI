using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace StardewModdingAPI.Web
{
    /// <summary>The main app entry point.</summary>
    public class Program
    {
        /*********
        ** Public methods
        *********/
        /// <summary>The main app entry point.</summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args)
        {
            // configure web server
            new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}
