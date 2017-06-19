using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace StardewModdingAPI.Tests.Utilities
{
    /// <summary>Unit tests for <see cref="SemanticVersion"/>.</summary>
    [TestFixture]
    internal class SemanticVersionTests
    {
        /*********
        ** Unit tests
        *********/
        /****
        ** Constructor
        ****/
        [Test(Description = "Assert that the constructor sets the expected values for all valid versions.")]
        [TestCase("1.0", ExpectedResult = "1.0")]
        [TestCase("1.0.0", ExpectedResult = "1.0")]
        [TestCase("3000.4000.5000", ExpectedResult = "3000.4000.5000")]
        [TestCase("1.2-some-tag.4", ExpectedResult = "1.2-some-tag.4")]
        [TestCase("1.2.3-some-tag.4", ExpectedResult = "1.2.3-some-tag.4")]
        [TestCase("1.2.3-some-tag.4      ", ExpectedResult = "1.2.3-some-tag.4")]
        public string Constructor_FromString(string input)
        {
            return new SemanticVersion(input).ToString();
        }

        [Test(Description = "Assert that the constructor sets the expected values for all valid versions.")]
        [TestCase(1, 0, 0, null, ExpectedResult = "1.0")]
        [TestCase(3000, 4000, 5000, null, ExpectedResult = "3000.4000.5000")]
        [TestCase(1, 2, 3, "", ExpectedResult = "1.2.3")]
        [TestCase(1, 2, 3, "    ", ExpectedResult = "1.2.3")]
        [TestCase(1, 2, 3, "some-tag.4", ExpectedResult = "1.2.3-some-tag.4")]
        [TestCase(1, 2, 3, "some-tag.4   ", ExpectedResult = "1.2.3-some-tag.4")]
        public string Constructor_FromParts(int major, int minor, int patch, string tag)
        {
            // act
            ISemanticVersion version = new SemanticVersion(major, minor, patch, tag);

            // assert
            Assert.AreEqual(major, version.MajorVersion, "The major version doesn't match the given value.");
            Assert.AreEqual(minor, version.MinorVersion, "The minor version doesn't match the given value.");
            Assert.AreEqual(patch, version.PatchVersion, "The patch version doesn't match the given value.");
            Assert.AreEqual(string.IsNullOrWhiteSpace(tag) ? null : tag.Trim(), version.Build, "The tag doesn't match the given value.");
            return version.ToString();
        }

        [Test(Description = "Assert that the constructor throws the expected exception for invalid versions.")]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("1")]
        [TestCase("01.0")]
        [TestCase("1.05")]
        [TestCase("1.5.06")] // leading zeros specifically prohibited by spec
        [TestCase("1.2.3.4")]
        [TestCase("1.apple")]
        [TestCase("1.2.apple")]
        [TestCase("1.2.3.apple")]
        [TestCase("1..2..3")]
        [TestCase("1.2.3-")]
        [TestCase("1.2.3-some-tag...")]
        [TestCase("1.2.3-some-tag...4")]
        [TestCase("apple")]
        [TestCase("-apple")]
        [TestCase("-5")]
        public void Constructor_FromString_WithInvalidValues(string input)
        {
            if (input == null)
                this.AssertAndLogException<ArgumentNullException>(() => new SemanticVersion(input));
            else
                this.AssertAndLogException<FormatException>(() => new SemanticVersion(input));
        }

        //[Test(Description = "Assert that the constructor throws an exception if the values are invalid.")]
        //[TestCase(01, "Spring", 1)] // seasons are case-sensitive
        //[TestCase(01, "springs", 1)] // invalid season name
        //[TestCase(-1, "spring", 1)] // day < 0
        //[TestCase(29, "spring", 1)] // day > 28
        //[TestCase(01, "spring", -1)] // year < 1
        //[TestCase(01, "spring", 0)] // year < 1
        //[SuppressMessage("ReSharper", "AssignmentIsFullyDiscarded", Justification = "Deliberate for unit test.")]
        //public void Constructor_RejectsInvalidValues(int day, string season, int year)
        //{
        //    // act & assert
        //    Assert.Throws<ArgumentException>(() => _ = new SDate(day, season, year), "Constructing the invalid date didn't throw the expected exception.");
        //}


        /*********
        ** Private methods
        *********/
        /// <summary>Assert that the expected exception type is thrown, and log the action output and thrown exception.</summary>
        /// <typeparam name="T">The expected exception type.</typeparam>
        /// <param name="action">The action which may throw the exception.</param>
        /// <param name="message">The message to log if the expected exception isn't thrown.</param>
        [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "The message argument is deliberately only used in precondition checks since this is an assertion method.")]
        private void AssertAndLogException<T>(Func<object> action, string message = null)
            where T : Exception
        {
            this.AssertAndLogException<T>(() =>
            {
                object result = action();
                TestContext.WriteLine($"Func result: {result}");
            });
        }

        /// <summary>Assert that the expected exception type is thrown, and log the thrown exception.</summary>
        /// <typeparam name="T">The expected exception type.</typeparam>
        /// <param name="action">The action which may throw the exception.</param>
        /// <param name="message">The message to log if the expected exception isn't thrown.</param>
        [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "The message argument is deliberately only used in precondition checks since this is an assertion method.")]
        private void AssertAndLogException<T>(Action action, string message = null)
            where T : Exception
        {
            try
            {
                action();
            }
            catch (T ex)
            {
                TestContext.WriteLine($"Exception thrown:\n{ex}");
                return;
            }
            catch (Exception ex) when (!(ex is AssertionException))
            {
                TestContext.WriteLine($"Exception thrown:\n{ex}");
                Assert.Fail(message ?? $"Didn't throw the expected exception; expected {typeof(T).FullName}, got {ex.GetType().FullName}.");
            }

            // no exception thrown
            Assert.Fail(message ?? "Didn't throw an exception.");
        }
    }
}
