using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StardewModdingAPI.Framework.Logging
{
    /// <summary>A text writer which allows intercepting output.</summary>
    internal class InterceptingTextWriter : TextWriter
    {
        /*********
        ** Properties
        *********/
        /// <summary>The current line being intercepted.</summary>
        private readonly List<char> Line = new List<char>();


        /*********
        ** Accessors
        *********/
        /// <summary>The underlying console output.</summary>
        public TextWriter Out { get; }

        /// <summary>The character encoding in which the output is written.</summary>
        public override Encoding Encoding => this.Out.Encoding;

        /// <summary>Whether to intercept console output.</summary>
        public bool ShouldIntercept { get; set; }

        /// <summary>The event raised when a line of text is intercepted.</summary>
        public event Action<string> OnLineIntercepted;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="output">The underlying output writer.</param>
        public InterceptingTextWriter(TextWriter output)
        {
            this.Out = output;
        }

        /// <summary>Writes a character to the text string or stream.</summary>
        /// <param name="ch">The character to write to the text stream.</param>
        public override void Write(char ch)
        {
            // intercept
            if (this.ShouldIntercept)
            {
                switch (ch)
                {
                    case '\r':
                        return;

                    case '\n':
                        this.OnLineIntercepted?.Invoke(new string(this.Line.ToArray()));
                        this.Line.Clear();
                        break;

                    default:
                        this.Line.Add(ch);
                        break;
                }
            }

            // pass through
            else
                this.Out.Write(ch);
        }

        /// <summary>Releases the unmanaged resources used by the <see cref="T:System.IO.TextWriter" /> and optionally releases the managed resources.</summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            this.OnLineIntercepted = null;
        }
    }
}
