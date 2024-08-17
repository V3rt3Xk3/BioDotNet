using Bio.Core.Alphabets;
using Bio.Core.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.Sequences;
public class NucelicAcidSequence : Sequence, INucleicAcidSequence
{
    protected NucelicAcidSequence() { }
    /// <summary>
    /// Initializes a new instance of the Sequence class with specified alphabet and string sequence.
    /// Symbols in the sequence are validated with the specified alphabet.
    /// </summary>
    /// <param name="alphabet">Alphabet to which this class should conform.</param>
    /// <param name="sequence">The sequence in string form.</param>
    public NucelicAcidSequence(IAlphabet alphabet, string sequence)
        : base(alphabet, sequence)
    {
    }

    /// <summary>
    /// Initializes a new instance of the Sequence class with specified alphabet and chars.
    /// chars representing Symbols in the values are validated with the specified alphabet.
    /// </summary>
    /// <param name="alphabet">Alphabet to which this instance should conform.</param>
    /// <param name="values">An array of chars representing the symbols.</param>
    public NucelicAcidSequence(IAlphabet alphabet, char[] values)
        : base(alphabet, values)
    {
    }

    /// <summary>
    /// Initializes a new instance of the Sequence class with passed new Sequence. Creates a copy of the sequence.
    /// </summary>
    /// <param name="newSequence">The New sequence for which the copy has to be made.</param>
    public NucelicAcidSequence(NucelicAcidSequence newSequence) : base(newSequence) { }

    /// <summary>
    /// Return a new sequence representing the complement of this sequence.
    /// </summary>
    public INucleicAcidSequence GetComplementedSequence()
    {
        if (!this.Alphabet.IsComplementSupported)
        {
            throw new InvalidOperationException(Resource.ComplementNotFound);
        }

        char[] complemented = new char[this.Count];
        this.Alphabet.TryGetComplementSymbol(this.sequence, out complemented);

        NucelicAcidSequence seq = new NucelicAcidSequence { sequence = complemented, Alphabet = this.Alphabet, ID = this.ID, Count = this.Count };
        if (this._metadata != null)
            seq._metadata = new Dictionary<string, object>(this._metadata);

        return seq;
    }

    /// <summary>
    /// Return a new sequence representing the reverse complement of this sequence.
    /// </summary>
    public INucleicAcidSequence GetReverseComplementedSequence()
    {
        if (!this.Alphabet.IsComplementSupported)
        {
            throw new InvalidOperationException(Resource.ComplementNotFound);
        }

        char[] reverseComplemented = new char[this.Count];
        this.Alphabet.TryGetComplementSymbol(this.sequence, out reverseComplemented);
        Array.Reverse(reverseComplemented);
        NucelicAcidSequence seq = new NucelicAcidSequence { sequence = reverseComplemented, Alphabet = this.Alphabet, ID = this.ID, Count = this.Count };
        if (this._metadata != null)
            seq._metadata = new Dictionary<string, object>(this._metadata);

        return seq;
    }
}
