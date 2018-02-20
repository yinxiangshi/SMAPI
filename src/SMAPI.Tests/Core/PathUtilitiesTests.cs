using NUnit.Framework;
using StardewModdingAPI.Framework.Utilities;

namespace StardewModdingAPI.Tests.Core
{
    /// <summary>Unit tests for <see cref="PathUtilities"/>.</summary>
    [TestFixture]
    public class PathUtilitiesTests
    {
        /*********
        ** Unit tests
        *********/
        [Test(Description = "Assert that GetSegments returns the expected values.")]
        [TestCase("", ExpectedResult = "")]
        [TestCase("/", ExpectedResult = "")]
        [TestCase("///", ExpectedResult = "")]
        [TestCase("/usr/bin", ExpectedResult = "usr|bin")]
        [TestCase("/usr//bin//", ExpectedResult = "usr|bin")]
        [TestCase("/usr//bin//.././boop.exe", ExpectedResult = "usr|bin|..|.|boop.exe")]
        [TestCase(@"C:", ExpectedResult = "C:")]
        [TestCase(@"C:/boop", ExpectedResult = "C:|boop")]
        [TestCase(@"C:\boop\/usr//bin//.././boop.exe", ExpectedResult = "C:|boop|usr|bin|..|.|boop.exe")]
        public string GetSegments(string path)
        {
            return string.Join("|", PathUtilities.GetSegments(path));
        }

        [Test(Description = "Assert that NormalisePathSeparators returns the expected values.")]
#if SMAPI_FOR_WINDOWS
        [TestCase("", ExpectedResult = "")]
        [TestCase("/", ExpectedResult = "")]
        [TestCase("///", ExpectedResult = "")]
        [TestCase("/usr/bin", ExpectedResult = @"usr\bin")]
        [TestCase("/usr//bin//", ExpectedResult = @"usr\bin")]
        [TestCase("/usr//bin//.././boop.exe", ExpectedResult = @"usr\bin\..\.\boop.exe")]
        [TestCase("C:", ExpectedResult = "C:")]
        [TestCase("C:/boop", ExpectedResult = @"C:\boop")]
        [TestCase(@"C:\usr\bin//.././boop.exe", ExpectedResult = @"C:\usr\bin\..\.\boop.exe")]
#else
        [TestCase("", ExpectedResult = "")]
        [TestCase("/", ExpectedResult = "/")]
        [TestCase("///", ExpectedResult = "/")]
        [TestCase("/usr/bin", ExpectedResult = "/usr/bin")]
        [TestCase("/usr//bin//", ExpectedResult = "/usr/bin")]
        [TestCase("/usr//bin//.././boop.exe", ExpectedResult = "/usr/bin/.././boop.exe")]
        [TestCase("C:", ExpectedResult = "C:")]
        [TestCase("C:/boop", ExpectedResult = "C:/boop")]
        [TestCase(@"C:\usr\bin//.././boop.exe", ExpectedResult = "C:/usr/bin/.././boop.exe")]
#endif
        public string NormalisePathSeparators(string path)
        {
            return PathUtilities.NormalisePathSeparators(path);
        }

        [Test(Description = "Assert that GetRelativePath returns the expected values.")]
#if SMAPI_FOR_WINDOWS
        [TestCase(@"C:\", @"C:\", ExpectedResult = "./")]
        [TestCase(@"C:\grandparent\parent\child", @"C:\grandparent\parent\sibling", ExpectedResult = @"..\sibling")]
        [TestCase(@"C:\grandparent\parent\child", @"C:\cousin\file.exe", ExpectedResult = @"..\..\..\cousin\file.exe")]
#else
        [TestCase("/", "/", ExpectedResult = "./")]
        [TestCase("/grandparent/parent/child", "/grandparent/parent/sibling", ExpectedResult = "../sibling")]
        [TestCase("/grandparent/parent/child", "/cousin/file.exe", ExpectedResult = "../../../cousin/file.exe")]
#endif
        public string GetRelativePath(string sourceDir, string targetPath)
        {
            return PathUtilities.GetRelativePath(sourceDir, targetPath);
        }
    }
}
