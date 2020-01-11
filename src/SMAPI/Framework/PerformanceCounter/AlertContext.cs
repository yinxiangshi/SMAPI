namespace StardewModdingAPI.Framework.PerformanceCounter
{
    public struct AlertContext
    {
        public string Source;
        public double Elapsed;

        public AlertContext(string source, double elapsed)
        {
            this.Source = source;
            this.Elapsed = elapsed;
        }
    }
}
