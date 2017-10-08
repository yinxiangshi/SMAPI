namespace StardewModdingApi.Installer
{
    /// <summary>The entry point for SMAPI's install and uninstall console app.</summary>
    internal class Program
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Run the install or uninstall script.</summary>
        /// <param name="args">The command line arguments.</param>
        public static void Main(string[] args)
        {
            var installer = new InteractiveInstaller();
            installer.Run(args);
        }
    }
}
