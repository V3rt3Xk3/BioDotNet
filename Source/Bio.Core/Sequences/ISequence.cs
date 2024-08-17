using Bio.Core.Alphabets;
using Bio.Core.IO;
using System.Collections;

namespace Bio.Core.Sequences;


/// <summary>
/// Implementations of ISequence make up the one of the core sets
/// of data structures in Bio. It is these sequences that store
/// data relevant to DNA, RNA, and Amino Acid structures. Several
/// algorithms for alignment, assembly, and analysis take these items
/// as their basic data inputs and outputs.
/// </summary>
public interface ISequence : IEnumerable<char>
{
    /// <summary>
    /// Gets or sets an identification provided to distinguish the sequence to others
    /// being worked with.
    /// </summary>
    string ID { get; set; }

    /// <summary>
    /// Gets alphabet to which this sequence should conform.
    /// </summary>
    IAlphabet Alphabet { get; }

    /// <summary>
    /// Sequence name, optional (string)
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Sequence description, optional (string)
    /// </summary>
    public string Description { get; set; }


    /// <summary>
    /// Database cross references, optional (list of strings)
    /// </summary>
    public List<string> DatabaseCrossReferences { get; protected set; }

    /// <summary>
    /// Any (sub)features, optional (list of Feature objects)
    /// </summary>
    public List<Feature> Features { get; protected set; }

    /// <summary>
    /// Dictionary of annotations for the whole sequence
    /// </summary>
    public Dictionary<string, object> Annotations { get; protected set; }

    /// <summary>
    /// Per letter/symbol annotation (restricted dictionary). This 
    /// holds string sequences(lists, strings or tuples) whose length 
    /// matches that of the sequence. A typical use would be to hold 
    /// a list of integers representing sequencing quality scores, or 
    /// a string representing the secondary structure.
    /// </summary>
    public Dictionary<string, IEnumerable> LetterAnnotations { get; protected set; }

    /// <summary>
    /// Gets the number of sequence items contained in the Sequence.
    /// </summary>
    long Count { get; }

    /// <summary>
    /// Allows the sequence to function like an array, getting and setting
    /// the sequence item at the particular index specified. Note that the
    /// index value starts its count at 0.
    /// </summary>
    /// <param name="index">The index value.</param>
    /// <returns>A byte which represents the symbol.</returns>
    char this[long index]
    {
        get;
    }

    /// <summary>
    /// Many sequence representations when saved to file also contain
    /// information about that sequence. Unfortunately there is no standard
    /// around what that data may be from format to format. This property
    /// allows a place to put structured metadata that can be accessed by
    /// a particular key.
    /// 
    /// For example, if species information is stored in a particular Species
    /// class, you could add it to the dictionary by:
    /// 
    /// mySequence.Metadata["SpeciesInfo"] = mySpeciesInfo;
    /// 
    /// To fetch the data you would use:
    /// 
    /// Species mySpeciesInfo = mySequence.Metadata["SpeciesInfo"];
    /// 
    /// Particular formats may create their own data model class for information
    /// unique to their format as well. Such as:
    /// 
    /// GenBankMetadata genBankData = new GenBankMetadata();
    /// // ... add population code
    /// mySequence.MetaData["GenBank"] = genBankData;
    /// </summary>
    Dictionary<string, object> Metadata { get; }

    /// <summary>
    /// Return a sequence representing this sequence with the orientation reversed.
    /// </summary>
    ISequence GetReversedSequence();


    

    /// <summary>
    /// Return a sequence representing a range (subsequence) of this sequence.
    /// </summary>
    /// <param name="start">The index of the first symbol in the range.</param>
    /// <param name="length">The number of symbols in the range.</param>
    /// <returns>The virtual sequence.</returns>
    ISequence GetSubSequence(long start, long length);


    /// <summary>
    /// Gets the index of first non gap symbol.
    /// </summary>
    /// <returns>If found returns an zero based index of the first non gap symbol, otherwise returns -1.</returns>
    long IndexOfNonGap();

    /// <summary>
    /// Returns the position of the first item beyond startPos that does not 
    /// have a Gap symbol.
    /// </summary>
    /// <param name="startPos">Index value above which to search for non-Gap symbol.</param>
    /// <returns>If found returns an zero based index of the first non gap symbol, otherwise returns -1.</returns>
    long IndexOfNonGap(long startPos);

    /// <summary>
    /// Gets the index of last non gap symbol.
    /// </summary>
    /// <returns>If found returns an zero based index of the last non gap symbol, otherwise returns -1.</returns>
    long LastIndexOfNonGap();

    /// <summary>
    /// Gets the index of last non gap symbol before the specified end position.
    /// </summary>
    /// <param name="endPos">Index value below which to search for non-Gap symbol.</param>
    /// <returns>If found returns an zero based index of the last non gap symbol, otherwise returns -1.</returns>
    long LastIndexOfNonGap(long endPos);
}

