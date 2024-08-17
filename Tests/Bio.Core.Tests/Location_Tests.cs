using Bio.Core.Alphabets;
using Bio.Core.Sequences.Location;
using NUnit.Framework;


namespace Bio.Core.Tests;
internal class Location_Tests
{
    [TestCase("123..145", new string[] { "123..145" })]
    public void _split_Regex_SplitsCorrectly(string input, string[] expectedResult)
    {
        // Arrange

        // Act
        string[] actualResult = Location._split(input);

        // Assert
        CollectionAssert.AreEqual(expectedResult, actualResult);
    }
}
