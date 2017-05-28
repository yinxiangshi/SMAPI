using System;
using System.Collections.Generic;
using StardewValley;

namespace StardewModdingAPI.Framework
{
    /// <summary>Provides translations stored in the mod's <c>i18n</c> folder, with one file per locale (like <c>en.json</c>) containing a flat key => value structure. Translations are fetched with locale fallback, so missing translations are filled in from broader locales (like <c>pt-BR.json</c> &lt; <c>pt.json</c> &lt; <c>default.json</c>).</summary>
    internal class TranslationHelper : ITranslationHelper
    {
        /*********
        ** Properties
        *********/
        /// <summary>The name of the relevant mod for error messages.</summary>
        private readonly string ModName;

        /// <summary>The translations for each locale.</summary>
        private readonly IDictionary<string, IDictionary<string, string>> All = new Dictionary<string, IDictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>The translations for the current locale, with locale fallback taken into account.</summary>
        private IDictionary<string, string> ForLocale;


        /*********
        ** Accessors
        *********/
        /// <summary>The current locale.</summary>
        public string Locale { get; private set; }

        /// <summary>The game's current language code.</summary>
        public LocalizedContentManager.LanguageCode LocaleEnum { get; private set; }

        /// <summary>Get a translation for the current locale. This is a convenience shortcut for <see cref="ITranslationHelper.Translate"/>.</summary>
        /// <param name="key">The translation key.</param>
        public Translation this[string key] => this.Translate(key);


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="modName">The name of the relevant mod for error messages.</param>
        /// <param name="locale">The initial locale.</param>
        /// <param name="languageCode">The game's current language code.</param>
        /// <param name="translations">The translations for each locale.</param>
        public TranslationHelper(string modName, string locale, LocalizedContentManager.LanguageCode languageCode, IDictionary<string, IDictionary<string, string>> translations)
        {
            // save data
            this.ModName = modName;
            foreach (var pair in translations)
                this.All[pair.Key] = new Dictionary<string, string>(pair.Value, StringComparer.InvariantCultureIgnoreCase);

            // set locale
            this.SetLocale(locale, languageCode);
        }

        /// <summary>Get all translations for the current locale.</summary>
        public IDictionary<string, string> GetTranslations()
        {
            return new Dictionary<string, string>(this.ForLocale, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>Get a translation for the current locale.</summary>
        /// <param name="key">The translation key.</param>
        public Translation Translate(string key)
        {
            this.ForLocale.TryGetValue(key, out string text);
            return new Translation(this.ModName, this.Locale, key, text);
        }

        /// <summary>Set the current locale and precache translations.</summary>
        /// <param name="locale">The current locale.</param>
        /// <param name="localeEnum">The game's current language code.</param>
        internal void SetLocale(string locale, LocalizedContentManager.LanguageCode localeEnum)
        {
            this.Locale = locale.ToLower().Trim();
            this.LocaleEnum = localeEnum;

            this.ForLocale = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string next in this.GetRelevantLocales(this.Locale))
            {
                // skip if locale not defined
                if (!this.All.TryGetValue(next, out IDictionary<string, string> translations))
                    continue;

                // add missing translations
                foreach (var pair in translations)
                {
                    if (!this.ForLocale.ContainsKey(pair.Key))
                        this.ForLocale.Add(pair);
                }
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the locales which can provide translations for the given locale, in precedence order.</summary>
        /// <param name="locale">The locale for which to find valid locales.</param>
        private IEnumerable<string> GetRelevantLocales(string locale)
        {
            // given locale
            yield return locale;

            // broader locales (like pt-BR => pt)
            while (true)
            {
                int dashIndex = locale.LastIndexOf('-');
                if (dashIndex <= 0)
                    break;

                locale = locale.Substring(0, dashIndex);
                yield return locale;
            }

            // default
            if (locale != "default")
                yield return "default";
        }
    }
}
