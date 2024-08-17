using Bio.Core.Resources;
using Bio.Core.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.Alphabets;
/// <summary>
/// The basic alphabet that describes symbols used in sequences of amino
/// acids that come from codon encodings of RNA. This alphabet allows for
/// the twenty amino acids as well as a termination and gap symbol.
/// <para>
/// The character representations come from the NCBIstdaa standard and
/// are used in many sequence file formats. The NCBIstdaa standard has all
/// the same characters as NCBIeaa and IUPACaa, but adds Selenocysteine,
/// termination, and gap symbols to the latter.
/// </para>
/// <para>
/// The entries in this dictionary are:
/// Symbol - Extended Symbol - Name
/// A - Ala - Alanine
/// C - Cys - Cysteine
/// D - Asp - Aspartic Acid
/// E - Glu - Glutamic Acid
/// F - Phe - Phenylalanine
/// G - Gly - Glycine
/// H - His - Histidine
/// I - Ile - Isoleucine
/// K - Lys - Lysine
/// L - Leu - Leucine
/// M - Met - Methionine
/// N - Asn - Asparagine
/// O - Pyl - Pyrrolysine
/// P - Pro - Proline
/// Q - Gln - Glutamine
/// R - Arg - Arginine
/// S - Ser - Serine
/// T - Thr - Threoine
/// U - Sel - Selenocysteine
/// V - Val - Valine
/// W - Trp - Tryptophan
/// Y - Tyr - Tyrosine
/// * - Ter - Termination
/// - - --- - Gap.
/// </para>
/// </summary>
public class ProteinAlphabet : IAlphabet
{
    #region Private members

    /// <summary>
    /// Contains only basic symbols including Gap
    /// </summary>
    private readonly HashSet<char> basicSymbols = new HashSet<char>();

    /// <summary>
    /// Amino acids map  -  Maps A to A  and a to A
    /// that is key will contain unique values.
    /// This will be used in the IsValidSymbol method to address Scenarios like a == A, G == g etc.
    /// </summary>
    private readonly Dictionary<char, char> aminoAcidValueMap = new Dictionary<char, char>();

    /// <summary>
    /// Symbol to three-letter amino acid abbreviation.
    /// </summary>
    private readonly Dictionary<char, string> abbreviationMap1to3 = new Dictionary<char, string>();

    /// <summary>
    /// Three-letter amino acid abbreviation to symbol.
    /// </summary>
    private readonly Dictionary<string, char> abbreviationMap3to1 = new Dictionary<string, char>();

    /// <summary>
    /// Symbol to Friendly name mapping.
    /// </summary>
    private readonly Dictionary<char, string> friendlyNameMap = new Dictionary<char, string>();

    /// <summary>
    /// Holds the amino acids present in this RnaAlphabet.
    /// </summary>
    private readonly List<char> aminoAcids = new List<char>();

    /// <summary>
    /// Mapping from set of symbols to corresponding ambiguous symbol.
    /// </summary>
    private readonly Dictionary<HashSet<char>, char> basicSymbolsToAmbiguousSymbolMap = new Dictionary<HashSet<char>, char>(new HashSetComparer<char>());

    /// <summary>
    /// Mapping from ambiguous symbol to set of basic symbols they represent.
    /// </summary>
    private readonly Dictionary<char, HashSet<char>> ambiguousSyToBasicSymbolsMap = new Dictionary<char, HashSet<char>>();

    #endregion Private members

    /// <summary>
    /// Initializes static members of the ProteinAlphabet class
    /// Set up the static instance.
    /// </summary>
    static ProteinAlphabet()
    {
        Instance = new ProteinAlphabet();
    }

    /// <summary>
    /// Initializes a new instance of the ProteinAlphabet class.
    /// </summary>
    protected ProteinAlphabet()
    {
        this.Name = Resource.ProteinAlphabetName;
        this.HasGaps = true;
        this.HasAmbiguity = false;
        this.HasTerminations = true;
        this.IsComplementSupported = false;

        this.A = (char)'A';
        this.C = (char)'C';
        this.D = (char)'D';
        this.E = (char)'E';
        this.F = (char)'F';
        this.G = (char)'G';
        this.H = (char)'H';
        this.I = (char)'I';
        this.K = (char)'K';
        this.L = (char)'L';
        this.M = (char)'M';
        this.N = (char)'N';
        this.O = (char)'O';
        this.P = (char)'P';
        this.Q = (char)'Q';
        this.R = (char)'R';
        this.S = (char)'S';
        this.T = (char)'T';
        this.U = (char)'U';
        this.V = (char)'V';
        this.W = (char)'W';
        this.Y = (char)'Y';

        this.Gap = (char)'-';
        this.Ter = (char)'*';

        // Add to basic symbols
        basicSymbols.Add(A); basicSymbols.Add((char)char.ToLower((char)A));
        basicSymbols.Add(C); basicSymbols.Add((char)char.ToLower((char)C));
        basicSymbols.Add(D); basicSymbols.Add((char)char.ToLower((char)D));
        basicSymbols.Add(E); basicSymbols.Add((char)char.ToLower((char)E));
        basicSymbols.Add(F); basicSymbols.Add((char)char.ToLower((char)F));
        basicSymbols.Add(G); basicSymbols.Add((char)char.ToLower((char)G));
        basicSymbols.Add(H); basicSymbols.Add((char)char.ToLower((char)H));
        basicSymbols.Add(I); basicSymbols.Add((char)char.ToLower((char)I));
        basicSymbols.Add(K); basicSymbols.Add((char)char.ToLower((char)K));
        basicSymbols.Add(L); basicSymbols.Add((char)char.ToLower((char)L));
        basicSymbols.Add(M); basicSymbols.Add((char)char.ToLower((char)M));
        basicSymbols.Add(N); basicSymbols.Add((char)char.ToLower((char)N));
        basicSymbols.Add(O); basicSymbols.Add((char)char.ToLower((char)O));
        basicSymbols.Add(P); basicSymbols.Add((char)char.ToLower((char)P));
        basicSymbols.Add(Q); basicSymbols.Add((char)char.ToLower((char)Q));
        basicSymbols.Add(R); basicSymbols.Add((char)char.ToLower((char)R));
        basicSymbols.Add(S); basicSymbols.Add((char)char.ToLower((char)S));
        basicSymbols.Add(T); basicSymbols.Add((char)char.ToLower((char)T));
        basicSymbols.Add(U); basicSymbols.Add((char)char.ToLower((char)U));
        basicSymbols.Add(V); basicSymbols.Add((char)char.ToLower((char)V));
        basicSymbols.Add(W); basicSymbols.Add((char)char.ToLower((char)W));
        basicSymbols.Add(Y); basicSymbols.Add((char)char.ToLower((char)Y));
        basicSymbols.Add(this.Gap);

        this.AddAminoAcid(this.A, "Ala", "Alanine", (char)'a');
        this.AddAminoAcid(this.C, "Cys", "Cysteine", (char)'c');
        this.AddAminoAcid(this.D, "Asp", "Aspartic", (char)'d');
        this.AddAminoAcid(this.E, "Glu", "Glutamic", (char)'e');
        this.AddAminoAcid(this.F, "Phe", "Phenylalanine", (char)'f');
        this.AddAminoAcid(this.G, "Gly", "Glycine", (char)'g');
        this.AddAminoAcid(this.H, "His", "Histidine", (char)'h');
        this.AddAminoAcid(this.I, "Ile", "Isoleucine", (char)'i');
        this.AddAminoAcid(this.K, "Lys", "Lysine", (char)'k');
        this.AddAminoAcid(this.L, "Leu", "Leucine", (char)'l');
        this.AddAminoAcid(this.M, "Met", "Methionine", (char)'m');
        this.AddAminoAcid(this.N, "Asn", "Asparagine", (char)'n');
        this.AddAminoAcid(this.O, "Pyl", "Pyrrolysine", (char)'o');
        this.AddAminoAcid(this.P, "Pro", "Proline", (char)'p');
        this.AddAminoAcid(this.Q, "Gln", "Glutamine", (char)'q');
        this.AddAminoAcid(this.R, "Arg", "Arginine", (char)'r');
        this.AddAminoAcid(this.S, "Ser", "Serine", (char)'s');
        this.AddAminoAcid(this.T, "Thr", "Threoine", (char)'t');
        this.AddAminoAcid(this.U, "Sec", "Selenocysteine", (char)'u');
        this.AddAminoAcid(this.V, "Val", "Valine", (char)'v');
        this.AddAminoAcid(this.W, "Trp", "Tryptophan", (char)'w');
        this.AddAminoAcid(this.Y, "Tyr", "Tyrosine", (char)'y');

        this.AddAminoAcid(this.Gap, "---", "Gap");
        this.AddAminoAcid(this.Ter, "***", "Termination");
    }

    /// <summary>
    /// Gets the Alanine Amino acid. 
    /// </summary>
    public char A { get; private set; }

    /// <summary>
    /// Gets the Cysteine Amino acid.
    /// </summary>
    public char C { get; private set; }

    /// <summary>
    /// Gets the Aspartic Acid.
    /// </summary>
    public char D { get; private set; }

    /// <summary>
    /// Gets the Glutamic Acid.
    /// </summary>
    public char E { get; private set; }

    /// <summary>
    /// Gets the Phenylalanine Amino acid. 
    /// </summary>
    public char F { get; private set; }

    /// <summary>
    /// Gets the Glycine Amino acid.
    /// </summary>
    public char G { get; private set; }

    /// <summary>
    /// Gets the Histidine Amino acid.
    /// </summary>
    public char H { get; private set; }

    /// <summary>
    /// Gets the Isoleucine Amino acid.
    /// </summary>
    public char I { get; private set; }

    /// <summary>
    /// Gets the Lysine Amino acid.
    /// </summary>
    public char K { get; private set; }

    /// <summary>
    /// Gets the Leucine Amino acid.
    /// </summary>
    public char L { get; private set; }

    /// <summary>
    /// Gets the Methionine Amino acid.
    /// </summary>
    public char M { get; private set; }

    /// <summary>
    /// Gets the Asparagine Amino acid.
    /// </summary>
    public char N { get; private set; }

    /// <summary>
    /// Gets the Pyrrolysine Amino acid.
    /// </summary>
    public char O { get; private set; }

    /// <summary>
    /// Gets the Proline Amino acid.
    /// </summary>
    public char P { get; private set; }

    /// <summary>
    /// Gets the Glutamine Amino acid.
    /// </summary>
    public char Q { get; private set; }

    /// <summary>
    /// Gets the Arginine Amino acid.
    /// </summary>
    public char R { get; private set; }

    /// <summary>
    /// Gets the Serine Amino acid.
    /// </summary>
    public char S { get; private set; }

    /// <summary>
    /// Gets the Threoine Amino acid.
    /// </summary>
    public char T { get; private set; }

    /// <summary>
    /// Gets the Selenocysteine Amino acid.
    /// </summary>
    public char U { get; private set; }

    /// <summary>
    /// Gets the Valine Amino acid.
    /// </summary>
    public char V { get; private set; }

    /// <summary>
    /// Gets the Tryptophan Amino acid.
    /// </summary>
    public char W { get; private set; }

    /// <summary>
    /// Gets the Tyrosine Amino acid.
    /// </summary>
    public char Y { get; private set; }

    /// <summary>
    /// Gets the Gap character.
    /// </summary>
    public char Gap { get; private set; }

    /// <summary>
    /// Gets the Termination character.
    /// </summary>
    public char Ter { get; private set; }

    /// <summary>
    /// Gets or sets the name of this alphabet - this is always 'Protein'.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    /// Gets or sets a value indicating whether this alphabet has a gap character.
    /// This alphabet does have a gap character.
    /// </summary>
    public bool HasGaps { get; protected set; }

    /// <summary>
    /// Gets or sets a value indicating whether this alphabet has ambiguous characters.
    /// This alphabet does have ambiguous characters.
    /// </summary>
    public bool HasAmbiguity { get; protected set; }

    /// <summary>
    /// Gets or sets a value indicating whether this alphabet has termination characters.
    /// This alphabet does have termination characters.
    /// </summary>
    public bool HasTerminations { get; protected set; }

    /// <summary>
    /// Gets or sets a value indicating whether complement is supported or not.
    /// </summary>
    public bool IsComplementSupported { get; protected set; }

    /// <summary>
    /// Instance of this class.
    /// </summary>
    public static readonly ProteinAlphabet Instance;

    /// <summary>
    /// Gets count of nucleotides.
    /// </summary>
    public int Count
    {
        get
        {
            return this.aminoAcids.Count;
        }
    }

    /// <summary>
    /// Gets the char value of item at the given index.
    /// </summary>
    /// <param name="index">Index of the item to retrieve.</param>
    /// <returns>char value at the given index.</returns>
    public char this[int index]
    {
        get
        {
            return this.aminoAcids[index];
        }
    }

    /// <summary>
    /// Gets the friendly name of a given symbol.
    /// </summary>
    /// <param name="item">Symbol to find friendly name.</param>
    /// <returns>Friendly name of the given symbol.</returns>
    public string GetFriendlyName(char item)
    {
        string fName;
        friendlyNameMap.TryGetValue(aminoAcidValueMap[item], out fName);
        return fName;
    }

    /// <summary>
    /// Gets the three-letter abbreviation of a given symbol.
    /// </summary>
    /// <param name="item">Symbol to find three-letter abbreviation.</param>
    /// <returns>Three-letter abbreviation of the given symbol.</returns>
    public string GetThreeLetterAbbreviation(char item)
    {
        string threeLetterAbbreviation;
        abbreviationMap1to3.TryGetValue(aminoAcidValueMap[item], out threeLetterAbbreviation);
        return threeLetterAbbreviation;
    }

    /// <summary>
    /// Gets the symbol from a three-letter abbreviation.
    /// </summary>
    /// <param name="item">Three letter abbreviation to find symbol.</param>
    /// <returns>Symbol corresponding to three-letter abbreviation.</returns>
    public char GetSymbolFromThreeLetterAbbrev(string item)
    {
        char symbol;
        abbreviationMap3to1.TryGetValue(item, out symbol);
        return symbol;
    }

    /// <summary>
    /// Gets the complement of the symbol.
    /// </summary>
    /// <param name="symbol">The protein symbol.</param>
    /// <param name="complementSymbol">The complement symbol.</param>
    /// <returns>Returns true if it gets the complements symbol.</returns>
    public bool TryGetComplementSymbol(char symbol, out char complementSymbol)
    {
        // Complement is not possible.
        complementSymbol = default(char);
        return false;
    }

    /// <summary>
    /// This method tries to get the complements for specified symbols.
    /// </summary>
    /// <param name="symbols">Symbols to look up.</param>
    /// <param name="complementSymbols">Complement  symbols (output).</param>
    /// <returns>Returns true if found else false.</returns>
    public bool TryGetComplementSymbol(char[] symbols, out char[] complementSymbols)
    {
        complementSymbols = null;
        return false;
    }
    /// <summary>
    /// Gets the default Gap symbol.
    /// </summary>
    /// <param name="defaultGapSymbol">The default symbol.</param>
    /// <returns>Returns true if it gets the Default Gap Symbol.</returns>
    public virtual bool TryGetDefaultGapSymbol(out char defaultGapSymbol)
    {
        defaultGapSymbol = this.Gap;
        return true;
    }

    /// <summary>
    /// Gets the default Termination symbol.
    /// </summary>
    /// <param name="defaultTerminationSymbol">The default Termination symbol.</param>
    /// <returns>Returns true if it gets the  default Termination symbol.</returns>
    public virtual bool TryGetDefaultTerminationSymbol(out char defaultTerminationSymbol)
    {
        defaultTerminationSymbol = this.Ter;
        return true;
    }

    /// <summary>
    /// Gets the Gap symbol.
    /// </summary>
    /// <param name="gapSymbols">The Gap Symbol.</param>
    /// <returns>Returns true if it gets the  Gap symbol.</returns>
    public virtual bool TryGetGapSymbols(out HashSet<char> gapSymbols)
    {
        gapSymbols = new HashSet<char>();
        gapSymbols.Add(this.Gap);
        return true;
    }

    /// <summary>
    /// Gets the Termination symbol.
    /// </summary>
    /// <param name="terminationSymbols">The Termination symbol.</param>
    /// <returns>Returns true if it gets the Termination symbol.</returns>
    public virtual bool TryGetTerminationSymbols(out HashSet<char> terminationSymbols)
    {
        terminationSymbols = new HashSet<char>();
        terminationSymbols.Add(this.Ter);
        return true;
    }

    /// <summary>
    /// Gets the valid symbol.
    /// </summary>
    /// <returns>Returns HashSet of valid symbols.</returns>
    public HashSet<char> GetValidSymbols()
    {
        return new HashSet<char>(this.aminoAcidValueMap.Keys);
    }

    /// <summary>
    /// Gets the ambigious characters present in alphabet.
    /// </summary>
    public HashSet<char> GetAmbiguousSymbols()
    {
        return new HashSet<char>(this.ambiguousSyToBasicSymbolsMap.Keys);
    }

    /// <summary>
    /// Maps A to A  and a to A
    /// that is key will contain unique values.
    /// This will be used in the IsValidSymbol method to address Scenarios like a == A, G == g etc.
    /// </summary>
    public Dictionary<char, char> GetSymbolValueMap()
    {
        Dictionary<char, char> symbolMap = new Dictionary<char, char>();

        foreach (KeyValuePair<char, char> mapping in this.aminoAcidValueMap)
        {
            symbolMap[mapping.Key] = mapping.Value;
        }

        return symbolMap;
    }

    /// <summary>
    /// Gets the Ambiguous symbol.
    /// </summary>
    /// <param name="symbols">The symbol.</param>
    /// <param name="ambiguousSymbol">The Ambiguous symbol.</param>
    /// <returns>Returns true if it gets the Ambiguous symbol.</returns>
    public bool TryGetAmbiguousSymbol(HashSet<char> symbols, out char ambiguousSymbol)
    {
        return this.basicSymbolsToAmbiguousSymbolMap.TryGetValue(symbols, out ambiguousSymbol);
    }

    /// <summary>
    /// Gets the Basic symbol.
    /// </summary>
    /// <param name="ambiguousSymbol">The Ambiguous symbol.</param>
    /// <param name="basicSymbols">The Basic symbol.</param>
    /// <returns>Returns true if it gets the Basic symbol.</returns>
    public bool TryGetBasicSymbols(char ambiguousSymbol, out HashSet<char> basicSymbols)
    {
        return this.ambiguousSyToBasicSymbolsMap.TryGetValue(ambiguousSymbol, out basicSymbols);
    }

    /// <summary>
    /// Compares two symbols.
    /// </summary>
    /// <param name="x">The first symbol to compare.</param>
    /// <param name="y">The second symbol to compare.</param>
    /// <returns>Returns true if x equals y else false.</returns>
    public virtual bool CompareSymbols(char x, char y)
    {
        char nucleotideA, nucleotideB;

        if (this.aminoAcidValueMap.TryGetValue(x, out nucleotideA))
        {
            if (this.aminoAcidValueMap.TryGetValue(y, out nucleotideB))
            {
                if (this.ambiguousSyToBasicSymbolsMap.ContainsKey(nucleotideA) || this.ambiguousSyToBasicSymbolsMap.ContainsKey(nucleotideB))
                {
                    return false;
                }

                return nucleotideA == nucleotideB;
            }
            else
            {
                throw new ArgumentException(Resource.InvalidParameter, "y");
            }
        }
        else
        {
            throw new ArgumentException(Resource.InvalidParameter, "x");
        }
    }

    /// <summary>
    /// Find the consensus nucleotide for a set of nucleotides.
    /// </summary>
    /// <param name="symbols">Set of sequence items.</param>
    /// <returns>Consensus nucleotide.</returns>
    public virtual char GetConsensusSymbol(HashSet<char> symbols)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// Validates if all symbols provided are Protein symbols or not.
    /// </summary>
    /// <param name="symbols">Symbols to be validated.</param>
    /// <param name="offset">Offset from where validation should start.</param>
    /// <param name="length">Number of symbols to validate from the specified offset.</param>
    /// <returns>True if the validation succeeds, else false.</returns>
    public bool ValidateSequence(char[] symbols, long offset, long length)
    {
        if (symbols == null)
        {
            throw new ArgumentNullException("symbols");
        }

        for (long i = offset; i < length; i++)
        {
            if (!this.aminoAcidValueMap.ContainsKey(symbols[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if the provided item is a gap character or not
    /// </summary>
    /// <param name="item">Item to be checked</param>
    /// <returns>True if the specified item is a gap</returns>
    public virtual bool CheckIsGap(char item)
    {
        return item == this.Gap;
    }

    /// <summary>
    /// Checks if the provided item is an ambiguous character or not
    /// </summary>
    /// <param name="item">Item to be checked</param>
    /// <returns>True if the specified item is a ambiguous</returns>
    public virtual bool CheckIsAmbiguous(char item)
    {
        return !basicSymbols.Contains(item);
    }

    /// <summary>
    /// char array of nucleotides.
    /// </summary>
    /// <returns>Returns the Enumerator for nucleotides list.</returns>
    public IEnumerator<char> GetEnumerator()
    {
        return this.aminoAcids.GetEnumerator();
    }

    /// <summary>
    /// Converts the Protein Alphabets to string.
    /// </summary>
    /// <returns>Protein alphabets.</returns>
    public override string ToString()
    {
        return new string(this.aminoAcids.Select(x => (char)x).ToArray());
    }

    /// <summary>
    /// Creates an IEnumerator of the nucleotides.
    /// </summary>
    /// <returns>Returns Enumerator over alphabet values.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    /// <summary>
    /// Adds a Amino acid to the existing amino acids.
    /// </summary>
    /// <param name="aminoAcidValue">Amino acid to be added.</param>
    /// <param name="threeLetterAbbreviation">Three letter abbreviation for the symbol.</param>
    /// <param name="friendlyName">User friendly name of the symbol.</param>
    /// <param name="otherPossibleValues">Maps Capital and small Letters.</param>
    protected void AddAminoAcid(char aminoAcidValue, string threeLetterAbbreviation, string friendlyName, params char[] otherPossibleValues)
    {
        // Verify whether the aminoAcidValue or other possible values already exist or not.
        if (this.aminoAcidValueMap.ContainsKey(aminoAcidValue) || otherPossibleValues.Any(x => this.aminoAcidValueMap.Keys.Contains(x)))
        {
            throw new ArgumentException(Resource.SymbolExistsInAlphabet, "aminoAcidValue");
        }
        if (string.IsNullOrEmpty(friendlyName))
        {
            throw new ArgumentNullException("friendlyName");
        }

        this.aminoAcidValueMap.Add(aminoAcidValue, aminoAcidValue);
        foreach (char value in otherPossibleValues)
        {
            this.aminoAcidValueMap.Add(value, aminoAcidValue);
        }

        this.aminoAcids.Add(aminoAcidValue);
        this.abbreviationMap1to3.Add(aminoAcidValue, threeLetterAbbreviation);
        this.abbreviationMap3to1.Add(threeLetterAbbreviation, aminoAcidValue);
        this.friendlyNameMap.Add(aminoAcidValue, friendlyName);
    }

    /// <summary>
    /// Maps the ambiguous amino acids to the amino acids it represents. 
    /// For example ambiguous amino acids M represents the basic nucleotides A or C.
    /// </summary>
    /// <param name="ambiguousAminoAcid">Ambiguous amino acids.</param>
    /// <param name="aminoAcidsToMap">Nucleotide represented by ambiguous amino acids.</param>
    protected void MapAmbiguousAminoAcid(char ambiguousAminoAcid, params char[] aminoAcidsToMap)
    {
        char ambiguousSymbol;

        // Verify whether the nucleotides to map are valid nucleotides.
        if (!this.aminoAcidValueMap.TryGetValue(ambiguousAminoAcid, out ambiguousSymbol) || !aminoAcidsToMap.All(x => this.aminoAcidValueMap.Keys.Contains(x)))
        {
            throw new ArgumentException(Resource.CouldNotRecognizeSymbol, "ambiguousAminoAcid");
        }

        char[] mappingValues = new char[aminoAcidsToMap.Length];
        int i = 0;
        foreach (char valueToMap in aminoAcidsToMap)
        {
            mappingValues[i++] = this.aminoAcidValueMap[valueToMap];
        }

        HashSet<char> basicSymbols = new HashSet<char>(mappingValues);
        this.ambiguousSyToBasicSymbolsMap.Add(ambiguousSymbol, basicSymbols);
        this.basicSymbolsToAmbiguousSymbolMap.Add(basicSymbols, ambiguousSymbol);
    }
}
