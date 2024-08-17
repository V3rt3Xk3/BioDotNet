using Bio.Core.Alphabets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.Sequences;
public class DnaSequence : NucelicAcidSequence
{
    /// <summary>
    /// Initializes a new instance of the Sequence class with specified alphabet and string sequence.
    /// Symbols in the sequence are validated with the specified alphabet.
    /// </summary>
    /// <param name="alphabet">Alphabet to which this class should conform.</param>
    /// <param name="sequence">The sequence in string form.</param>
    public DnaSequence(IAlphabet alphabet, string sequence)
        : base(alphabet, sequence)
    {
    }

    /// <summary>
    /// Initializes a new instance of the Sequence class with specified alphabet and chars.
    /// chars representing Symbols in the values are validated with the specified alphabet.
    /// </summary>
    /// <param name="alphabet">Alphabet to which this instance should conform.</param>
    /// <param name="values">An array of chars representing the symbols.</param>
    public DnaSequence(IAlphabet alphabet, char[] values)
        : base(alphabet, values)
    {
    }

    /// <summary>
    /// Initializes a new instance of the Sequence class with passed new Sequence. Creates a copy of the sequence.
    /// </summary>
    /// <param name="newSequence">The New sequence for which the copy has to be made.</param>
    public DnaSequence(DnaSequence newSequence) : base(newSequence) { }
}
