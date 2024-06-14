using Bio.Core.Alphabet;
using Bio.Core.Sequences;
using NUnit.Framework;

namespace Bio.Core.Tests;

public class DnaSequence_tests
{

    [Test]
    public void Test1()
    {
        Sequence sequence = new(StaticAlphabets.DNA, "AGTCTCGCTAGCATCGCAT");
        Assert.That("AGTCTCGCTAGCATCGCAT" == sequence.ToString(), $"The sequence ");
    }
}