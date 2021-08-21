using GamesBucket.Shared.Helpers;
using NUnit.Framework;

namespace GamesBucket.Tests
{
    public class ExtensionTest
    {
        [Test]
        public void Remove_Substring_From_String()
        {
            var source = "Batman: Arkham Asylum Game of the Year Edition";
            var substring = "Game of the Year Edition\\s*";
            var expected = "Batman: Arkham Asylum";

            Assert.AreEqual(expected, source.RemoveSubstringPattern(substring).RemoveSubstringPattern(@"\(([^\)]+)\)"));
        }
    }
}