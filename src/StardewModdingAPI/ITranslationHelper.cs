using System.Collections.Generic;
using StardewValley;

namespace StardewModdingAPI
{
    /// <summary>Provides translations stored in the mod's <c>i18n</c> folder, with one file per locale (like <c>en.json</c>) containing a flat key => value structure. Translations are fetched with locale fallback, so missing translations are filled in from broader locales (like <c>pt-BR.json</c> &lt; <c>pt.json</c> &lt; <c>default.json</c>).</summary>
    public interface ITranslationHelper
    {
        /*********
        ** Accessors
        *********/
        /// <summary>The current locale.</summary>
        string Locale { get; }

        /// <summary>The game's current language code.</summary>
        LocalizedContentManager.LanguageCode LocaleEnum { get; }


        /*********
        ** Public methods
        *********/
        /// <summary>Get all translations for the current locale.</summary>
        IDictionary<string, string> GetTranslations();

        /// <summary>Get a translation for the current locale.</summary>
        /// <param name="key">The translation key.</param>
        Translation Translate(string key);
    }
}
