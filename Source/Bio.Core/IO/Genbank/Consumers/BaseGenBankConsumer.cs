using Bio.Core.Sequences;
using Bio.Util;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.IO.Genbank.Consumers;

/// <summary>
/// <para>Abstract GenBank consumer providing useful general functions (PRIVATE).</para>
/// <para>This just helps to eliminate some duplication in things that most
/// GenBank consumers want to do.</para>
/// </summary>
public abstract class BaseGenBankConsumer
{
    protected ILogger logger { get; init; }

    protected Reference curReference; // Adjust the type as necessary
    public BaseGenBankConsumer()
    {
        this.logger = new BioLogger("BaseGenBankConsumer");
    }
    public Sequence Data { get; set; } = new Sequence();

    public static HashSet<string> RemoveSpaceKeys { get; protected set; } = new HashSet<string> { "translation" };


    /// <summary>
    /// Split a string of keywords into a nice clean list.
    /// </summary>
    /// <param name="keywordString">The string containing the keywords.</param>
    /// <returns>A list of cleaned keywords.</returns>
    protected static List<string> SplitKeywords(string keywordString)
    {
        // Process the keywords into a C# list
        string keywords = keywordString;
        if (string.IsNullOrEmpty(keywordString) || keywordString == ".")
        {
            keywords = "";
        }
        else if (keywordString.EndsWith("."))
        {
            keywords = keywordString.Substring(0, keywordString.Length - 1);
        }

        List<string> keywordList = keywords.Split(';').Select(x => x.Trim()).ToList();
        return keywordList;
    }

    /// <summary>
    /// Split a string of accession numbers into a list.
    /// </summary>
    /// <param name="accessionString">The string containing the accession numbers.</param>
    /// <returns>A list of cleaned accession numbers.</returns>
    protected static List<string> SplitAccessions(string accessionString)
    {
        // Replace all line feeds with spaces and semicolons with spaces
        string accessions = accessionString.Replace("\n", " ").Replace(";", " ");

        // Split the string into a list, removing any empty entries
        return accessions.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => x.Trim())
                         .Where(x => !string.IsNullOrEmpty(x))
                         .ToList();
    }

    /// <summary>
    /// Split a string with taxonomy info into a list.
    /// </summary>
    /// <param name="taxonomyString">The string containing the taxonomy information.</param>
    /// <returns>A list of cleaned taxonomy information.</returns>
    protected static List<string> SplitTaxonomy(string taxonomyString)
    {
        if (string.IsNullOrEmpty(taxonomyString) || taxonomyString == ".")
        {
            // Missing data, no taxonomy
            return new List<string>();
        }

        string taxInfo = taxonomyString.EndsWith(".") ? taxonomyString[..^1] : taxonomyString;
        List<string> taxList = taxInfo.Split(';').ToList();
        List<string> newTaxList = new List<string>();

        foreach (var taxItem in taxList)
        {
            newTaxList.AddRange(taxItem.Split('\n'));
        }

        newTaxList.RemoveAll(item => item == "");
        return newTaxList.Select(x => x.Trim()).ToList();
    }

    /// <summary>
    /// Clean whitespace out of a location string.
    /// The location parser isn't a fan of whitespace, so we clean it out
    /// before feeding it into the parser.
    /// This method splits the string on whitespace and rejoins it,
    /// effectively removing all whitespace.
    /// </summary>
    /// <param name="locationString"></param>
    /// <returns></returns>
    protected static string CleanLocation(string locationString)
    {
        // Clean whitespace out of a location string.
        // The location parser isn't a fan of whitespace, so we clean it out
        // before feeding it into the parser.
        // This method splits the string on whitespace and rejoins it,
        // effectively removing all whitespace.
        return string.Join("", locationString.Split());
    }

    /// <summary>
    /// Remove any newlines in the passed text, returning the new string.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    protected static string RemoveNewlines(string text)
    {
        // Remove any newlines in the passed text, returning the new string.
        string[] newlines = { "\n", "\r" };
        foreach (string newLineCharacter in newlines)
        {
            text = text.Replace(newLineCharacter, "");
        }

        return text;
    }

    /// <summary>
    /// Replace multiple spaces in the passed text with single spaces (PRIVATE).
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    protected static string NormalizeSpaces(string text)
    {
        // Replace multiple spaces in the passed text with single spaces.
        return string.Join(" ", text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>
    /// Remove all spaces from the passed text.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    protected static string RemoveSpaces(string text)
    {
        // Remove all spaces from the passed text.
        return text.Replace(" ", "");
    }

    /// <summary>
    /// Convert a start and end range to python notation(PRIVATE).
    ///
    /// In GenBank, starts and ends are defined in "biological" coordinates,
    /// where 1 is the first base and[i, j] means to include both i and j.
    /// In python, 0 is the first base and[i, j] means to include i, but
    /// not j.
    /// So, to convert "biological" to python coordinates, we need to
    /// subtract 1 from the start, and leave the end as it is to
    /// convert correctly.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    protected static (int newStart, int newEnd) ConvertToPythonNumbers(int start, int end)
    {

        int newStart = start - 1;
        int newEnd = end;

        return (newStart, newEnd);
    }

    /// <summary>
    /// Set the locus name is set as the name of the Sequence.
    /// </summary>
    /// <param name="locusName"></param>
    public abstract void Locus(string locusName);

    /// <summary>
    /// Record the sequence length.
    /// </summary>
    /// <param name="length"></param>
    public abstract void Size(string length);

    public abstract void ResidueType(string residueType);

    public abstract void Topology(string topology);

    public abstract void MoleculeType(string moleculeType);

    public abstract void DataFileDivision(string dataFileDivision);

    public abstract void Date(string date);

    public abstract void Definition(string definition);

    public abstract void Accession(string accession);

    public abstract void Tls(string content);

    public abstract void Tsa(string content);

    public abstract void Wgs(string content);

    public abstract void AddWgsScafld(string content);

    public abstract void Nid(string content);

    public abstract void Pid(string content);

    public abstract void Version(string versionId);

    public abstract void VersionSuffix(string version);

    public abstract void Project(string content);

    public abstract void DbLink(string content);

    public abstract void DbSource(string content);

    public abstract void Gi(string content);

    public abstract void Keywords(string content);

    public abstract void Segment(string content);

    public abstract void Source(string content);

    public abstract void Organism(string content);

    public abstract void Taxonomy(string content);

    public abstract void ReferenceNum(string content);

    public abstract void ReferenceBases(string content);
}
