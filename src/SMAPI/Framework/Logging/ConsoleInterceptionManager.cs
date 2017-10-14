using System;

namespace StardewModdingAPI.Framework.Logging
{
    /// <summary>Manages console output interception.</summary>
    internal class ConsoleInterceptionManager : IDisposable
    {
        /*********
        ** Properties
        *********/
        /// <summary>The intercepting console writer.</summary>
        private readonly InterceptingTextWriter Output;


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the current console supports color formatting.</summary>
        public bool SupportsColor { get; }

        /// <summary>The event raised when a message is written to the console directly.</summary>
        public event Action<string> OnMessageIntercepted;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public ConsoleInterceptionManager()
        {
            // redirect output through interceptor
            this.Output = new InterceptingTextWriter(Console.Out);
            this.Output.OnMessageIntercepted += line => this.OnMessageIntercepted?.Invoke(line);
            Console.SetOut(this.Output);

            // test color support
            this.SupportsColor = this.TestColorSupport();
        }

        /// <summary>Get an exclusive lock and write to the console output without interception.</summary>
        /// <param name="action">The action to perform within the exclusive write block.</param>
        public void ExclusiveWriteWithoutInterception(Action action)
        {
            lock (Console.Out)
            {
                try
                {
                    this.Output.ShouldIntercept = false;
                    action();
                }
                finally
                {
                    this.Output.ShouldIntercept = true;
                }
            }
        }

        /// <summary>Release all resources.</summary>
        public void Dispose()
        {
            Console.SetOut(this.Output.Out);
            this.Output.Dispose();
        }


        /*********
        ** private methods
        *********/
        /// <summary>Test whether the current console supports color formatting.</summary>
        private bool TestColorSupport()
        {
            try
            {
                this.ExclusiveWriteWithoutInterception(() =>
                {
                    Console.ForegroundColor = Console.ForegroundColor;
                });
                return true;
            }
            catch (Exception)
            {
                return false; // Mono bug
            }
        }
    }
}
