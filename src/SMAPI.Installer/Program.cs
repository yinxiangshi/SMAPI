using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using StardewModdingAPI.Internal;

namespace StardewModdingApi.Installer
{
    /// <summary>The entry point for SMAPI's install and uninstall console app.</summary>
    internal class Program
    {
        /*********
        ** Properties
        *********/
        /// <summary>The absolute path to search for referenced assemblies.</summary>
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute", Justification = "The assembly location is never null in this context.")]
        private static string DllSearchPath;


        /*********
        ** Public methods
        *********/
        /// <summary>Run the install or uninstall script.</summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            // set up assembly resolution
            string installerPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Program.DllSearchPath = Path.Combine(installerPath, "internal", EnvironmentUtility.DetectPlatform() == Platform.Windows ? "Windows" : "Mono", "smapi-internal");
            AppDomain.CurrentDomain.AssemblyResolve += Program.CurrentDomain_AssemblyResolve;

            // launch installer
            var installer = new InteractiveInstaller();
            installer.Run(args);
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Method called when assembly resolution fails, which may return a manually resolved assembly.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
        {
            try
            {
                AssemblyName name = new AssemblyName(e.Name);
                foreach (FileInfo dll in new DirectoryInfo(Program.DllSearchPath).EnumerateFiles("*.dll"))
                {
                    if (name.Name.Equals(AssemblyName.GetAssemblyName(dll.FullName).Name, StringComparison.InvariantCultureIgnoreCase))
                        return Assembly.LoadFrom(dll.FullName);
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resolving assembly: {ex}");
                return null;
            }
        }
    }
}
