namespace StardewModdingAPI.Framework.PerformanceCounter
{
    /// <summary>The context for an alert.</summary>
    internal struct AlertContext
    {
        /// <summary>The source which triggered the alert.</summary>
        public readonly string Source;

        /// <summary>The elapsed milliseconds.</summary>
        public readonly double Elapsed;

        /// <summary>Creates a new alert context.</summary>
        /// <param name="source">The source which triggered the alert.</param>
        /// <param name="elapsed">The elapsed milliseconds.</param>
        public AlertContext(string source, double elapsed)
        {
            this.Source = source;
            this.Elapsed = elapsed;
        }

        public override string ToString()
        {
            return $"{this.Source}: {this.Elapsed:F2}ms";
        }
    }
}
