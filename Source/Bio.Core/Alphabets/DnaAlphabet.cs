using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bio.Core.Resources;
using Bio.Core.Util;

namespace Bio.Core.Alphabets;
/// <summary>
///     The basic alphabet that describes symbols used in DNA sequences.
///     This alphabet allows not only for the four base nucleotide symbols,
///     but also for various ambiguities, termination, and gap symbols.
///     <para>
///         The character representations come from the NCBI4na standard and
///         are used in many sequence file formats. The NCBI4na standard is the
///         same as the IUPACna standard with only the addition of the gap
///         character.
///     </para>
///     <para>
///         The entries in this dictionary are:
///         Symbol - Name
///         A - Adenine
///         C - Cytosine
///         M - A or C
///         G - Guanine
///         R - G or A
///         S - G or C
///         V - G or V or A
///         T - Thymine
///         W - A or T
///         Y - T or C
///         H - A or C or T
///         K - G or T
///         D - G or A or T
///         B - G or T or C
///         - - Gap
///         N - A or G or T or C.
///     </para>
/// </summary>
public class DnaAlphabet : IAlphabet
{
    #region Private members

    /// <summary>
    ///     Contains only basic symbols including Gap
    /// </summary>
    private readonly HashSet<char> basicSymbols = new HashSet<char>();

    /// <summary>
    ///     Nucleotide map  -  Maps A to A  and a to A
    ///     that is key will contain unique values.
    ///     This will be used in the IsValidSymbol method to address Scenarios like a == A, G == g etc.
    /// </summary>
    private readonly Dictionary<char, char> nucleotideValueMap = new Dictionary<char, char>();

    /// <summary>
    ///     Symbol to Friendly name mapping.
    /// </summary>
    private readonly Dictionary<char, string> friendlyNameMap = new Dictionary<char, string>();

    /// <summary>
    ///     Holds the nucleotides present in this DnaAlphabet.
    /// </summary>
    private readonly List<char> nucleotides = new List<char>();

    /// <summary>
    /// Mapping from set of symbols to corresponding ambiguous symbol.
    /// </summary>
    private Dictionary<HashSet<char>, char> basicSymbolsToAmbiguousSymbolMap = new Dictionary<HashSet<char>, char>(new HashSetComparer<char>());

    /// <summary>
    ///     Mapping from ambiguous symbol to set of basic symbols they represent.
    /// </summary>
    private readonly Dictionary<char, HashSet<char>> ambiguousSyToBasicSymbolsMap =
        new Dictionary<char, HashSet<char>>();

    /// <summary>
    ///     Holds complements.
    /// </summary>
    private readonly Dictionary<char, char> symbolToComplementSymbolMap = new Dictionary<char, char>();

    #endregion Private members

    /// <summary>
    ///     Initializes static members of the DnaAlphabet class.
    /// </summary>
    static DnaAlphabet()
    {
        Instance = new DnaAlphabet();
    }

    /// <summary>
    ///     Initializes a new instance of the DnaAlphabet class.
    /// </summary>
    protected DnaAlphabet()
    {
        this.Name = Resource.DnaAlphabetName;
        this.HasGaps = true;
        this.HasAmbiguity = false;
        this.HasTerminations = false;
        this.IsComplementSupported = true;
        this.A = 'A';
        this.C = 'C';
        this.G = 'G';
        this.T = 'T';
        this.Gap = '-';

        // Add to basic symbols
        this.basicSymbols.Add(this.A);
        this.basicSymbols.Add(char.ToLower(this.A));
        this.basicSymbols.Add(this.C);
        this.basicSymbols.Add(char.ToLower(this.C));
        this.basicSymbols.Add(this.G);
        this.basicSymbols.Add(char.ToLower(this.G));
        this.basicSymbols.Add(this.T);
        this.basicSymbols.Add(char.ToLower(this.T));
        this.basicSymbols.Add(this.Gap);

        // Add nucleotides
        this.AddNucleotide(this.A, "Adenine", 'a');
        this.AddNucleotide(this.C, "Cytosine", 'c');
        this.AddNucleotide(this.G, "Guanine", 'g');
        this.AddNucleotide(this.T, "Thymine", 't');
        this.AddNucleotide(this.Gap, "Gap");

        // Populate compliment data
        this.MapComplementNucleotide(this.A, this.T);
        this.MapComplementNucleotide(this.T, this.A);
        this.MapComplementNucleotide(this.C, this.G);
        this.MapComplementNucleotide(this.G, this.C);
        this.MapComplementNucleotide(this.Gap, this.Gap);
    }

    /// <summary>
    ///     Gets A - Adenine.
    /// </summary>
    public char A { get; private set; }

    /// <summary>
    ///     Gets T - Thymine.
    /// </summary>
    public char T { get; private set; }

    /// <summary>
    ///     Gets G - Guanine.
    /// </summary>
    public char G { get; private set; }

    /// <summary>
    ///     Gets C - Cytosine.
    /// </summary>
    public char C { get; private set; }

    /// <summary>
    ///     Gets Default Gap symbol.
    /// </summary>
    public char Gap { get; private set; }

    /// <summary>
    ///     Gets or sets Friendly name for Alphabet type.
    /// </summary>
    public string Name { get; protected set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this alphabet has a gap character.
    ///     This alphabet does have a gap symbol.
    /// </summary>
    public bool HasGaps { get; protected set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this alphabet has ambiguous character.
    ///     This alphabet does have ambiguous symbols.
    /// </summary>
    public bool HasAmbiguity { get; protected set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this alphabet has termination character.
    ///     This alphabet does not have termination symbols.
    /// </summary>
    public bool HasTerminations { get; protected set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this Complement is supported on this Alphabet.
    ///     This alphabet has support for complement.
    /// </summary>
    public bool IsComplementSupported { get; protected set; }

    /// <summary>
    ///     Static instance of this class.
    /// </summary>
    public static readonly DnaAlphabet Instance;

    /// <summary>
    ///     Gets count of nucleotides.
    /// </summary>
    public int Count
    {
        get
        {
            return this.nucleotides.Count;
        }
    }

    /// <summary>
    ///     Gets the byte value of item at the given index.
    /// </summary>
    /// <param name="index">Index of the item to retrieve.</param>
    /// <returns>Byte value at the given index.</returns>
    public char this[int index]
    {
        get
        {
            return this.nucleotides[index];
        }
    }

    /// <summary>
    ///     Gets the friendly name of a given symbol.
    /// </summary>
    /// <param name="item">Symbol to find friendly name.</param>
    /// <returns>Friendly name of the given symbol.</returns>
    public string GetFriendlyName(char item)
    {
        string fName;
        this.friendlyNameMap.TryGetValue(this.nucleotideValueMap[item], out fName);
        return fName;
    }

    /// <summary>
    ///     This method tries to get the complement of this symbol.
    /// </summary>
    /// <param name="symbol">Symbol to look up.</param>
    /// <param name="complementSymbol">Complement  symbol (output).</param>
    /// <returns>Returns true if found else false.</returns>
    public bool TryGetComplementSymbol(char symbol, out char complementSymbol)
    {
        // verify whether the nucleotides exist or not.
        char nucleotide;
        if (this.nucleotideValueMap.TryGetValue(symbol, out nucleotide))
        {
            return this.symbolToComplementSymbolMap.TryGetValue(nucleotide, out complementSymbol);
        }
        complementSymbol = default(char);
        return false;
    }

    /// <summary>
    ///     This method tries to get the complements for specified symbols.
    /// </summary>
    /// <param name="symbols">Symbols to look up.</param>
    /// <param name="complementSymbols">Complement  symbols (output).</param>
    /// <returns>Returns true if found else false.</returns>
    public bool TryGetComplementSymbol(char[] symbols, out char[] complementSymbols)
    {
        if (symbols == null)
        {
            complementSymbols = null;
            return false;
        }

        long length = symbols.GetLongLength(0);
        complementSymbols = new char[length];
        for (long index = 0; index < length; index++)
        {
            char nucleotide;
            char complementSymbol;
            if (this.nucleotideValueMap.TryGetValue(symbols[index], out nucleotide)
                && this.symbolToComplementSymbolMap.TryGetValue(nucleotide, out complementSymbol))
            {
                complementSymbols[index] = complementSymbol;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Try to get the default gap symbol.
    /// </summary>
    /// <param name="defaultGapSymbol">Default gap symbol (output).</param>
    /// <returns>True if gets else false.</returns>
    public virtual bool TryGetDefaultGapSymbol(out char defaultGapSymbol)
    {
        defaultGapSymbol = this.Gap;
        return true;
    }

    /// <summary>
    ///     Get the termination symbols if present in the alphabet.
    /// </summary>
    /// <param name="defaultTerminationSymbol">The default Termination Symbol.</param>
    /// <returns>True if gets else false.</returns>
    public virtual bool TryGetDefaultTerminationSymbol(out char defaultTerminationSymbol)
    {
        defaultTerminationSymbol = default(char);
        return false;
    }

    /// <summary>
    ///     Get the gap symbols if present in the alphabet.
    /// </summary>
    /// <param name="gapSymbols">Hash set of gap Symbols.</param>
    /// <returns>If Gaps found returns true. </returns>
    public virtual bool TryGetGapSymbols(out HashSet<char> gapSymbols)
    {
        gapSymbols = new HashSet<char>();
        gapSymbols.Add(this.Gap);
        return true;
    }

    /// <summary>
    ///     Get the termination symbols if present in the alphabet.
    /// </summary>
    /// <param name="terminationSymbols">Termination Symbols.</param>
    /// <returns>True if gets else false.</returns>
    public virtual bool TryGetTerminationSymbols(out HashSet<char> terminationSymbols)
    {
        terminationSymbols = null;
        return false;
    }

    /// <summary>
    ///     Get the valid symbols in the alphabet.
    /// </summary>
    /// <returns>True if gets else false.</returns>
    public HashSet<char> GetValidSymbols()
    {
        return new HashSet<char>(this.nucleotideValueMap.Keys);
    }

    /// <summary>
    ///     Get the ambiguous symbols if present in the alphabet.
    /// </summary>
    /// <param name="symbols">The symbols.</param>
    /// <param name="ambiguousSymbol">Ambiguous Symbol. </param>
    /// <returns>True if gets else false.</returns>
    public bool TryGetAmbiguousSymbol(HashSet<char> symbols, out char ambiguousSymbol)
    {
        return this.basicSymbolsToAmbiguousSymbolMap.TryGetValue(symbols, out ambiguousSymbol);
    }

    /// <summary>
    ///     Get the basic symbols if present in the alphabet.
    /// </summary>
    /// <param name="ambiguousSymbol">The ambiguousSymbol.</param>
    /// <param name="basicSymbols">The basicSymbols.</param>
    /// <returns>True if gets else false.</returns>
    public bool TryGetBasicSymbols(char ambiguousSymbol, out HashSet<char> basicSymbols)
    {
        return this.ambiguousSyToBasicSymbolsMap.TryGetValue(ambiguousSymbol, out basicSymbols);
    }

    /// <summary>
    ///     Compares two symbols.
    /// </summary>
    /// <param name="x">The first symbol to compare.</param>
    /// <param name="y">The second symbol to compare.</param>
    /// <returns>Returns true if x equals y else false.</returns>
    public virtual bool CompareSymbols(char x, char y)
    {
        char nucleotideA;

        if (this.nucleotideValueMap.TryGetValue(x, out nucleotideA))
        {
            char nucleotideB;
            if (this.nucleotideValueMap.TryGetValue(y, out nucleotideB))
            {
                if (this.ambiguousSyToBasicSymbolsMap.ContainsKey(nucleotideA)
                    || this.ambiguousSyToBasicSymbolsMap.ContainsKey(nucleotideB))
                {
                    return false;
                }

                return nucleotideA == nucleotideB;
            }
            throw new ArgumentException(Resource.InvalidParameter, "y");
        }
        throw new ArgumentException(Resource.InvalidParameter, "x");
    }

    /// <summary>
    ///     Find the consensus nucleotide for a set of nucleotides.
    /// </summary>
    /// <param name="symbols">Set of sequence items.</param>
    /// <returns>Consensus nucleotide.</returns>
    public virtual char GetConsensusSymbol(HashSet<char> symbols)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     Validates if all symbols provided are DNA symbols or not.
    /// </summary>
    /// <param name="symbols">Symbols to be validated.</param>
    /// <param name="offset">Offset from where validation should start.</param>
    /// <param name="length">Number of symbols to validate from the specified offset.</param>
    /// <returns>True if the validation succeeds, else false.</returns>
    public bool ValidateSequence(char[] symbols, long offset, long length)
    {
        if (symbols == null)
        {
            throw new ArgumentNullException(nameof(symbols));
        }

        if (symbols.Length < offset + length)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        for (long i = offset; i < (length + offset); i++)
        {
            if (!this.nucleotideValueMap.ContainsKey(symbols[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Checks if the provided item is a gap character or not
    /// </summary>
    /// <param name="item">Item to be checked</param>
    /// <returns>True if the specified item is a gap</returns>
    public virtual bool CheckIsGap(char item)
    {
        return item == this.Gap;
    }

    /// <summary>
    ///     Checks if the provided item is an ambiguous character or not
    /// </summary>
    /// <param name="item">Item to be checked</param>
    /// <returns>True if the specified item is a ambiguous</returns>
    public virtual bool CheckIsAmbiguous(char item)
    {
        return !this.basicSymbols.Contains(item);
    }

    /// <summary>
    ///     Gets the ambiguous symbols present in alphabet.
    /// </summary>
    public HashSet<char> GetAmbiguousSymbols()
    {
        return new HashSet<char>(this.ambiguousSyToBasicSymbolsMap.Keys);
    }

    /// <summary>
    ///     Maps A to A  and a to A
    ///     that is key will contain unique values.
    ///     This will be used in the IsValidSymbol method to address Scenarios like a == A, G == g etc.
    /// </summary>
    public Dictionary<char, char> GetSymbolValueMap()
    {
        var symbolMap = new Dictionary<char, char>();

        foreach (var mapping in this.nucleotideValueMap)
        {
            symbolMap[mapping.Key] = mapping.Value;
        }

        return symbolMap;
    }

    /// <summary>
    ///     Converts the DNA Alphabets to string.
    /// </summary>
    /// <returns>DNA alphabets.</returns>
    public override string ToString()
    {
        return new string(this.nucleotides.Select(x => (char)x).ToArray());
    }

    /// <summary>
    ///     Byte array of nucleotides.
    /// </summary>
    /// <returns>Returns the Enumerator for nucleotides list.</returns>
    public IEnumerator<char> GetEnumerator()
    {
        return this.nucleotides.GetEnumerator();
    }

    /// <summary>
    ///     Creates an IEnumerator of the nucleotides.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    /// <summary>
    ///     Add the given nucleotide symbols to this alphabet type.
    /// </summary>
    /// <param name="nucleotideValue">The nucleotide Value.</param>
    /// <param name="friendlyName">User friendly name of the symbol.</param>
    /// <param name="otherPossibleValues">The other Possible Values.</param>
    protected void AddNucleotide(char nucleotideValue, string friendlyName, params char[] otherPossibleValues)
    {
        // Verify whether the nucleotide value or other possible values already exist or not.
        if (this.nucleotideValueMap.ContainsKey(nucleotideValue)
            || otherPossibleValues.Any(x => this.nucleotideValueMap.Keys.Contains(x)))
        {
            throw new ArgumentException(Resource.SymbolExistsInAlphabet, "nucleotideValue");
        }
        if (string.IsNullOrEmpty(friendlyName))
        {
            throw new ArgumentNullException("friendlyName");
        }

        this.nucleotideValueMap.Add(nucleotideValue, nucleotideValue);
        foreach (char value in otherPossibleValues)
        {
            this.nucleotideValueMap.Add(value, nucleotideValue);
        }

        this.nucleotides.Add(nucleotideValue);
        this.friendlyNameMap.Add(nucleotideValue, friendlyName);
    }

    /// <summary>
    ///     Maps the ambiguous nucleotide to the nucleotides it represents.
    ///     For example ambiguous nucleotide M represents the basic nucleotides A or C.
    /// </summary>
    /// <param name="ambiguousNucleotide">Ambiguous nucleotide.</param>
    /// <param name="nucleotidesToMap">Nucleotide represented by ambiguous nucleotide.</param>
    protected void MapAmbiguousNucleotide(char ambiguousNucleotide, params char[] nucleotidesToMap)
    {
        char ambiguousSymbol;

        // Verify whether the nucleotides to map are valid nucleotides.
        if (!this.nucleotideValueMap.TryGetValue(ambiguousNucleotide, out ambiguousSymbol))
        {
            throw new ArgumentException(Resource.CouldNotRecognizeSymbol, "ambiguousNucleotide");
        }

        var mappingValues = new char[nucleotidesToMap.Length];
        int i = 0;
        char validatedValueToMap;

        foreach (char valueToMap in nucleotidesToMap)
        {
            if (!this.nucleotideValueMap.TryGetValue(valueToMap, out validatedValueToMap))
            {
                throw new ArgumentException(Resource.CouldNotRecognizeSymbol, "nucleotidesToMap");
            }

            mappingValues[i++] = validatedValueToMap;
        }

        var basicSymbols = new HashSet<char>(mappingValues);
        this.ambiguousSyToBasicSymbolsMap.Add(ambiguousSymbol, basicSymbols);
        this.basicSymbolsToAmbiguousSymbolMap.Add(basicSymbols, ambiguousSymbol);
    }

    /// <summary>
    ///     Verify whether the nucleotides exist or not.
    /// </summary>
    /// <param name="nucleotide">The Nucleotide.</param>
    /// <param name="complementNucleotide">Complement Nucleotide.</param>
    protected void MapComplementNucleotide(char nucleotide, char complementNucleotide)
    {
        // verify whether the nucleotides exist or not.
        char symbol; // validated nucleotides
        if (this.nucleotideValueMap.TryGetValue(nucleotide, out symbol))
        {
            char complementSymbol; // validated nucleotides
            if (this.nucleotideValueMap.TryGetValue(complementNucleotide, out complementSymbol))
            {
                this.symbolToComplementSymbolMap.Add(symbol, complementSymbol);
                return;
            }
        }

        throw new ArgumentException(Resource.CouldNotRecognizeSymbol, "nucleotide");
    }
}
