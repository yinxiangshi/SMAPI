using System;
using System.Collections.Generic;
using NUnit.Framework;
using StardewModdingAPI.Framework;
using StardewValley;

namespace StardewModdingAPI.Tests
{
    /// <summary>Unit tests for <see cref="TranslationHelper"/> and <see cref="Translation"/>.</summary>
    [TestFixture]
    public class TranslationTests
    {
        /*********
        ** Data
        *********/
        /// <summary>Sample translation text for unit tests.</summary>
        public static string[] Samples = { null, "", "  ", "boop", "  boop  " };

        /// <summary>A token structure type.</summary>
        public enum TokenType
        {
            /// <summary>The tokens are passed in a dictionary.</summary>
            Dictionary,

            /// <summary>The tokens are passed in an anonymous object.</summary>
            AnonymousObject
        }


        /*********
        ** Unit tests
        *********/
        /****
        ** Translation helper
        ****/
        [Test(Description = "Assert that the translation helper correctly handles no translations.")]
        public void Helper_HandlesNoTranslations()
        {
            // arrange
            var data = new Dictionary<string, IDictionary<string, string>>();

            // act
            ITranslationHelper helper = new TranslationHelper("ModName", "en", LocalizedContentManager.LanguageCode.en).SetTranslations(data);
            Translation translation = helper.Get("key");

            // assert
            Assert.AreEqual("en", helper.Locale, "The locale doesn't match the input value.");
            Assert.AreEqual(LocalizedContentManager.LanguageCode.en, helper.LocaleEnum, "The locale enum doesn't match the input value.");
            Assert.IsNotNull(helper.GetTranslations(), "The full list of translations is unexpectedly null.");
            Assert.AreEqual(0, helper.GetTranslations().Count, "The full list of translations is unexpectedly not empty.");

            Assert.IsNotNull(translation, "The translation helper unexpectedly returned a null translation.");
            Assert.AreEqual(this.GetPlaceholderText("key"), translation.ToString(), "The translation returned an unexpected value.");
        }

        [Test(Description = "Assert that the translation helper returns the expected translations correctly.")]
        public void Helper_GetTranslations_ReturnsExpectedText()
        {
            // arrange
            var data = this.GetSampleData();
            var expected = this.GetExpectedTranslations();

            // act
            var actual = new Dictionary<string, IDictionary<string, string>>();
            TranslationHelper helper = new TranslationHelper("ModName", "en", LocalizedContentManager.LanguageCode.en).SetTranslations(data);
            foreach (string locale in expected.Keys)
            {
                this.AssertSetLocale(helper, locale, LocalizedContentManager.LanguageCode.en);
                actual[locale] = helper.GetTranslations();
            }

            // assert
            foreach (string locale in expected.Keys)
            {
                Assert.IsNotNull(actual[locale], $"The translations for {locale} is unexpectedly null.");
                Assert.That(actual[locale], Is.EquivalentTo(expected[locale]), $"The translations for {locale} don't match the expected values.");
            }
        }

        [Test(Description = "Assert that the translations returned by the helper has the expected text.")]
        public void Helper_Get_ReturnsExpectedText()
        {
            // arrange
            var data = this.GetSampleData();
            var expected = this.GetExpectedTranslations();

            // act
            var actual = new Dictionary<string, IDictionary<string, string>>();
            TranslationHelper helper = new TranslationHelper("ModName", "en", LocalizedContentManager.LanguageCode.en).SetTranslations(data);
            foreach (string locale in expected.Keys)
            {
                this.AssertSetLocale(helper, locale, LocalizedContentManager.LanguageCode.en);
                actual[locale] = new Dictionary<string, string>();
                foreach (string key in expected[locale].Keys)
                    actual[locale][key] = helper.Get(key);
            }

            // assert
            foreach (string locale in expected.Keys)
            {
                Assert.IsNotNull(actual[locale], $"The translations for {locale} is unexpectedly null.");
                Assert.That(actual[locale], Is.EquivalentTo(expected[locale]), $"The translations for {locale} don't match the expected values.");
            }
        }

        /****
        ** Translation
        ****/
        [Test(Description = "Assert that HasValue returns the expected result for various inputs.")]
        [TestCase(null, ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase("  ", ExpectedResult = true)]
        [TestCase("boop", ExpectedResult = true)]
        [TestCase("  boop  ", ExpectedResult = true)]
        public bool Translation_HasValue(string text)
        {
            return new Translation("ModName", "pt-BR", "key", text).HasValue();
        }

        [Test(Description = "Assert that the translation's ToString method returns the expected text for various inputs.")]
        public void Translation_ToString([ValueSource(nameof(TranslationTests.Samples))] string text)
        {
            // act
            Translation translation = new Translation("ModName", "pt-BR", "key", text);

            // assert
            if (translation.HasValue())
                Assert.AreEqual(text, translation.ToString(), "The translation returned an unexpected value given a valid input.");
            else
                Assert.AreEqual(this.GetPlaceholderText("key"), translation.ToString(), "The translation returned an unexpected value given a null or empty input.");
        }

        [Test(Description = "Assert that the translation's implicit string conversion returns the expected text for various inputs.")]
        public void Translation_ImplicitStringConversion([ValueSource(nameof(TranslationTests.Samples))] string text)
        {
            // act
            Translation translation = new Translation("ModName", "pt-BR", "key", text);

            // assert
            if (translation.HasValue())
                Assert.AreEqual(text, (string)translation, "The translation returned an unexpected value given a valid input.");
            else
                Assert.AreEqual(this.GetPlaceholderText("key"), (string)translation, "The translation returned an unexpected value given a null or empty input.");
        }

        [Test(Description = "Assert that the translation returns the expected text given a use-placeholder setting.")]
        public void Translation_UsePlaceholder([Values(true, false)] bool value, [ValueSource(nameof(TranslationTests.Samples))] string text)
        {
            // act
            Translation translation = new Translation("ModName", "pt-BR", "key", text).UsePlaceholder(value);

            // assert
            if (translation.HasValue())
                Assert.AreEqual(text, translation.ToString(), "The translation returned an unexpected value given a valid input.");
            else if (!value)
                Assert.AreEqual(text, translation.ToString(), "The translation returned an unexpected value given a null or empty input with the placeholder disabled.");
            else
                Assert.AreEqual(this.GetPlaceholderText("key"), translation.ToString(), "The translation returned an unexpected value given a null or empty input with the placeholder enabled.");
        }

        [Test(Description = "Assert that the translation's Assert method throws the expected exception.")]
        public void Translation_Assert([ValueSource(nameof(TranslationTests.Samples))] string text)
        {
            // act
            Translation translation = new Translation("ModName", "pt-BR", "key", text);

            // assert
            if (translation.HasValue())
                Assert.That(() => translation.Assert(), Throws.Nothing, "The assert unexpected threw an exception for a valid input.");
            else
                Assert.That(() => translation.Assert(), Throws.Exception.TypeOf<KeyNotFoundException>(), "The assert didn't throw an exception for invalid input.");
        }

        [Test(Description = "Assert that the translation returns the expected text after setting the default.")]
        public void Translation_Default([ValueSource(nameof(TranslationTests.Samples))] string text, [ValueSource(nameof(TranslationTests.Samples))] string @default)
        {
            // act
            Translation translation = new Translation("ModName", "pt-BR", "key", text).Default(@default);

            // assert
            if (!string.IsNullOrEmpty(text))
                Assert.AreEqual(text, translation.ToString(), "The translation returned an unexpected value given a valid base text.");
            else if (!string.IsNullOrEmpty(@default))
                Assert.AreEqual(@default, translation.ToString(), "The translation returned an unexpected value given a null or empty base text, but valid default.");
            else
                Assert.AreEqual(this.GetPlaceholderText("key"), translation.ToString(), "The translation returned an unexpected value given a null or empty base and default text.");
        }

        /****
        ** Translation tokens
        ****/
        [Test(Description = "Assert that multiple translation tokens are replaced correctly regardless of the token structure.")]
        public void Translation_Tokens([Values(TokenType.AnonymousObject, TokenType.Dictionary)] TokenType tokenType)
        {
            // arrange
            string start = Guid.NewGuid().ToString("N");
            string middle = Guid.NewGuid().ToString("N");
            string end = Guid.NewGuid().ToString("N");
            const string input = "{{start}} tokens are properly replaced (including {{middle}} {{  MIDdlE}}) {{end}}";
            string expected = $"{start} tokens are properly replaced (including {middle} {middle}) {end}";

            // act
            Translation translation = new Translation("ModName", "pt-BR", "key", input);
            switch (tokenType)
            {
                case TokenType.AnonymousObject:
                    translation = translation.Tokens(new { start, middle, end });
                    break;

                case TokenType.Dictionary:
                    translation = translation.Tokens(new Dictionary<string, object> { ["start"] = start, ["middle"] = middle, ["end"] = end });
                    break;

                default:
                    throw new NotSupportedException($"Unknown token type {tokenType}.");
            }

            // assert
            Assert.AreEqual(expected, translation.ToString(), "The translation returned an unexpected text.");
        }

        [Test(Description = "Assert that the translation can replace tokens in all valid formats.")]
        [TestCase("{{value}}", "value")]
        [TestCase("{{ value }}", "value")]
        [TestCase("{{value       }}", "value")]
        [TestCase("{{ the_value }}", "the_value")]
        [TestCase("{{ the.value_here }}", "the.value_here")]
        [TestCase("{{ the_value-here.... }}", "the_value-here....")]
        [TestCase("{{ tHe_vALuE-HEre.... }}", "tHe_vALuE-HEre....")]
        public void Translation_Tokens_ValidFormats(string text, string key)
        {
            // arrange
            string value = Guid.NewGuid().ToString("N");

            // act
            Translation translation = new Translation("ModName", "pt-BR", "key", text).Tokens(new Dictionary<string, object> { [key] = value });

            // assert
            Assert.AreEqual(value, translation.ToString(), "The translation returned an unexpected value given a valid base text.");
        }

        [Test(Description = "Assert that translation tokens are case-insensitive and surrounding-whitespace-insensitive.")]
        [TestCase("{{value}}", "value")]
        [TestCase("{{VaLuE}}", "vAlUe")]
        [TestCase("{{VaLuE   }}", "   vAlUe")]
        public void Translation_Tokens_KeysAreNormalised(string text, string key)
        {
            // arrange
            string value = Guid.NewGuid().ToString("N");

            // act
            Translation translation = new Translation("ModName", "pt-BR", "key", text).Tokens(new Dictionary<string, object> { [key] = value });

            // assert
            Assert.AreEqual(value, translation.ToString(), "The translation returned an unexpected value given a valid base text.");
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Set a translation helper's locale and assert that it was set correctly.</summary>
        /// <param name="helper">The translation helper to change.</param>
        /// <param name="locale">The expected locale.</param>
        /// <param name="localeEnum">The expected game language code.</param>
        private void AssertSetLocale(TranslationHelper helper, string locale, LocalizedContentManager.LanguageCode localeEnum)
        {
            helper.SetLocale(locale, localeEnum);
            Assert.AreEqual(locale, helper.Locale, "The locale doesn't match the input value.");
            Assert.AreEqual(localeEnum, helper.LocaleEnum, "The locale enum doesn't match the input value.");
        }

        /// <summary>Get sample raw translations to input.</summary>
        private IDictionary<string, IDictionary<string, string>> GetSampleData()
        {
            return new Dictionary<string, IDictionary<string, string>>
            {
                ["default"] = new Dictionary<string, string>
                {
                    ["key A"] = "default A",
                    ["key C"] = "default C"
                },
                ["en"] = new Dictionary<string, string>
                {
                    ["key A"] = "en A",
                    ["key B"] = "en B"
                },
                ["en-US"] = new Dictionary<string, string>(),
                ["zzz"] = new Dictionary<string, string>
                {
                    ["key A"] = "zzz A"
                }
            };
        }

        /// <summary>Get the expected translation output given <see cref="TranslationTests.GetSampleData"/>, based on the expected locale fallback.</summary>
        private IDictionary<string, IDictionary<string, string>> GetExpectedTranslations()
        {
            return new Dictionary<string, IDictionary<string, string>>
            {
                ["default"] = new Dictionary<string, string>
                {
                    ["key A"] = "default A",
                    ["key C"] = "default C"
                },
                ["en"] = new Dictionary<string, string>
                {
                    ["key A"] = "en A",
                    ["key B"] = "en B",
                    ["key C"] = "default C"
                },
                ["en-us"] = new Dictionary<string, string>
                {
                    ["key A"] = "en A",
                    ["key B"] = "en B",
                    ["key C"] = "default C"
                },
                ["zzz"] = new Dictionary<string, string>
                {
                    ["key A"] = "zzz A",
                    ["key C"] = "default C"
                }
            };
        }

        /// <summary>Get the default placeholder text when a translation is missing.</summary>
        /// <param name="key">The translation key.</param>
        private string GetPlaceholderText(string key)
        {
            return string.Format(Translation.PlaceholderText, key);
        }
    }
}
