using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StardewModdingAPI
{
    /// <summary>A translation string with a fluent API to customise it.</summary>
    public class Translation
    {
        /*********
        ** Properties
        *********/
        /// <summary>The name of the relevant mod for error messages.</summary>
        private readonly string ModName;

        /// <summary>The locale for which the translation was fetched.</summary>
        private readonly string Locale;

        /// <summary>The translation key.</summary>
        private readonly string Key;

        /// <summary>The underlying translation text.</summary>
        private readonly string Text;

        /// <summary>The value to return if the translations is undefined.</summary>
        private readonly string Placeholder;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an isntance.</summary>
        /// <param name="modName">The name of the relevant mod for error messages.</param>
        /// <param name="locale">The locale for which the translation was fetched.</param>
        /// <param name="key">The translation key.</param>
        /// <param name="text">The underlying translation text.</param>
        internal Translation(string modName, string locale, string key, string text)
            : this(modName, locale, key, text, $"(no translation:{key})") { }

        /// <summary>Construct an isntance.</summary>
        /// <param name="modName">The name of the relevant mod for error messages.</param>
        /// <param name="locale">The locale for which the translation was fetched.</param>
        /// <param name="key">The translation key.</param>
        /// <param name="text">The underlying translation text.</param>
        /// <param name="placeholder">The value to return if the translations is undefined.</param>
        internal Translation(string modName, string locale, string key, string text, string placeholder)
        {
            this.ModName = modName;
            this.Locale = locale;
            this.Key = key;
            this.Text = text;
            this.Placeholder = placeholder;
        }

        /// <summary>Throw an exception if the translation text is <c>null</c> or empty.</summary>
        /// <exception cref="KeyNotFoundException">There's no available translation matching the requested key and locale.</exception>
        public Translation Assert()
        {
            if (!this.HasValue())
                throw new KeyNotFoundException($"The '{this.ModName}' mod doesn't have a translation with key '{this.Key}' for the '{this.Locale}' locale or its fallbacks.");
            return this;
        }

        /// <summary>Replace the text if it's <c>null</c> or empty. If you set a <c>null</c> or empty value, the translation will show the fallback "no translation" placeholder (see <see cref="UsePlaceholder"/> if you want to disable that). Returns a new instance if changed.</summary>
        /// <param name="default">The default value.</param>
        public Translation Default(string @default)
        {
            return this.HasValue()
                ? this
                : new Translation(this.ModName, this.Locale, this.Key, @default);
        }

        /// <summary>Whether to return a "no translation" placeholder if the translation is <c>null</c> or empty. Returns a new instance.</summary>
        /// <param name="use">Whether to return a placeholder.</param>
        public Translation UsePlaceholder(bool use)
        {
            return new Translation(this.ModName, this.Locale, this.Key, this.Text, use ? $"(no translation:{this.Key})" : null);
        }

        /// <summary>Replace tokens in the text like <c>{{value}}</c> with the given values. Returns a new instance.</summary>
        /// <param name="tokens">An anonymous object containing token key/value pairs, like <c>new { value = 42, name = "Cranberries" }</c>.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="tokens"/> argument is <c>null</c>.</exception>
        public Translation Tokens(object tokens)
        {
            if (tokens == null)
                throw new ArgumentNullException(nameof(tokens));

            IDictionary<string, object> dictionary = tokens
                .GetType()
                .GetProperties()
                .ToDictionary(
                    p => p.Name,
                    p => p.GetValue(tokens)
                );
            return this.Tokens(dictionary);
        }

        /// <summary>Replace tokens in the text like <c>{{value}}</c> with the given values. Returns a new instance.</summary>
        /// <param name="tokens">A dictionary containing token key/value pairs.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="tokens"/> argument is <c>null</c>.</exception>
        public Translation Tokens(IDictionary<string, object> tokens)
        {
            if (tokens == null)
                throw new ArgumentNullException(nameof(tokens));

            tokens = tokens.ToDictionary(p => p.Key.Trim(), p => p.Value, StringComparer.InvariantCultureIgnoreCase);
            string text = Regex.Replace(this.Text, @"{{([ \w\.\-]+)}}", match =>
            {
                string key = match.Groups[1].Value.Trim();
                return tokens.TryGetValue(key, out object value)
                    ? value?.ToString()
                    : match.Value;
            });
            return new Translation(this.ModName, this.Locale, this.Key, text);
        }

        /// <summary>Get whether the translation has a defined value.</summary>
        public bool HasValue()
        {
            return !string.IsNullOrEmpty(this.Text);
        }

        /// <summary>Get the translation text. Calling this method isn't strictly necessary, since you can assign a <see cref="Translation"/> value directly to a string.</summary>
        public override string ToString()
        {
            return this.Placeholder != null && !this.HasValue()
                ? this.Placeholder
                : this.Text;
        }

        /// <summary>Get a string representation of the given translation.</summary>
        /// <param name="translation">The translation key.</param>
        public static implicit operator string(Translation translation)
        {
            return translation?.ToString();
        }
    }
}
