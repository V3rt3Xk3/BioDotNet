using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.IO;

/// <summary>
/// Hold GenBank information in a format similar to the original record.
/// The Record class is meant to make data easy to get to when you are
/// just interested in looking at GenBank data.
/// </summary>
/// <remarks>
/// Attributes:
/// <list type="bullet">
/// <item>
/// <description>locus - The name specified after the LOCUS keyword in the GenBank record. This may be the accession number, or a clone id or something else.</description>
/// </item>
/// <item>
/// <description>size - The size of the record.</description>
/// </item>
/// <item>
/// <description>residue_type - The type of residues making up the sequence in this record. Normally something like RNA, DNA or PROTEIN, but may be as esoteric as 'ss-RNA circular'.</description>
/// </item>
/// <item>
/// <description>data_file_division - The division this record is stored under in GenBank (ie. PLN -> plants; PRI -> humans, primates; BCT -> bacteria...)</description>
/// </item>
/// <item>
/// <description>date - The date of submission of the record, in a form like '28-JUL-1998'</description>
/// </item>
/// <item>
/// <description>accession - List of all accession numbers for the sequence.</description>
/// </item>
/// <item>
/// <description>nid - Nucleotide identifier number.</description>
/// </item>
/// <item>
/// <description>pid - Protein identifier number</description>
/// </item>
/// <item>
/// <description>version - The accession number + version (ie. AB01234.2)</description>
/// </item>
/// <item>
/// <description>db_source - Information about the database the record came from</description>
/// </item>
/// <item>
/// <description>gi - The NCBI gi identifier for the record.</description>
/// </item>
/// <item>
/// <description>keywords - A list of keywords related to the record.</description>
/// </item>
/// <item>
/// <description>segment - If the record is one of a series, this is info about which segment this record is (something like '1 of 6').</description>
/// </item>
/// <item>
/// <description>source - The source of material where the sequence came from.</description>
/// </item>
/// <item>
/// <description>organism - The genus and species of the organism (ie. 'Homo sapiens')</description>
/// </item>
/// <item>
/// <description>taxonomy - A listing of the taxonomic classification of the organism, starting general and getting more specific.</description>
/// </item>
/// <item>
/// <description>references - A list of Reference objects.</description>
/// </item>
/// <item>
/// <description>comment - Text with any kind of comment about the record.</description>
/// </item>
/// <item>
/// <description>features - A listing of Features making up the feature table.</description>
/// </item>
/// <item>
/// <description>base_counts - A string with the counts of bases for the sequence.</description>
/// </item>
/// <item>
/// <description>origin - A string specifying info about the origin of the sequence.</description>
/// </item>
/// <item>
/// <description>sequence - A string with the sequence itself.</description>
/// </item>
/// <item>
/// <description>contig - A string of location information for a CONTIG in a RefSeq file</description>
/// </item>
/// <item>
/// <description>project - The genome sequencing project numbers (will be replaced by the dblink cross-references in 2009).</description>
/// </item>
/// <item>
/// <description>dblinks - The genome sequencing project number(s) and other links. (will replace the project information in 2009).</description>
/// </item>
/// </list>
/// </remarks>
public class GenbankRecord
{
    public static readonly int GB_LINE_LENGTH = 79;
    public static readonly int GB_BASE_INDENT = 12;
    public static readonly int GB_FEATURE_INDENT = 21;
    public static readonly int GB_INTERNAL_INDENT = 2;
    public static readonly int GB_OTHER_INTERNAL_INDENT = 3;
    public static readonly int GB_FEATURE_INTERNAL_INDENT = 5;
    public static readonly int GB_SEQUENCE_INDENT = 9;

    public static readonly string BASE_FORMAT = $"%-{GB_BASE_INDENT}s";
    public static readonly string INTERNAL_FORMAT = $" {" ".PadLeft(GB_INTERNAL_INDENT)}%-{GB_BASE_INDENT - GB_INTERNAL_INDENT}s";
    public static readonly string OTHER_INTERNAL_FORMAT = $" {" ".PadLeft(GB_OTHER_INTERNAL_INDENT)}%-{GB_BASE_INDENT - GB_OTHER_INTERNAL_INDENT}s";
    public static readonly string BASE_FEATURE_FORMAT = $"%-{GB_FEATURE_INDENT}s";
    public static readonly string INTERNAL_FEATURE_FORMAT = $" {" ".PadLeft(GB_FEATURE_INTERNAL_INDENT)}%-{GB_FEATURE_INDENT - GB_FEATURE_INTERNAL_INDENT}s";
    public static readonly string SEQUENCE_FORMAT = $"%{GB_SEQUENCE_INDENT}s";


    public string Locus { get; set; }
    public int Size { get; set; }
    public string ResidueType { get; set; }
    public string DataFileDivision { get; set; }
    public string Date { get; set; }
    public List<string> Accession { get; set; }
    public string Nid { get; set; }
    public string Pid { get; set; }
    public string Version { get; set; }
    public string DbSource { get; set; }
    public string Gi { get; set; }
    public List<string> Keywords { get; set; }
    public string Segment { get; set; }
    public string Source { get; set; }
    public string Organism { get; set; }
    public List<string> Taxonomy { get; set; }
    public List<Reference> References { get; set; }
    public string Comment { get; set; }
    public List<Feature> Features { get; set; }
    public string BaseCounts { get; set; }
    public string Origin { get; set; }
    public string Sequence { get; set; }
    public string Contig { get; set; }
    public string Project { get; set; }
    public List<string> DbLinks { get; set; }

    public static string WrappedGenbank(string information, int indent, bool wrapSpace = true, char splitChar = ' ')
    {
        int infoLength = GB_LINE_LENGTH - indent;
        if (string.IsNullOrEmpty(information))
        {
            // GenBank files use "." for missing data
            return ".\n";
        }

        string[] infoParts;
        if (wrapSpace)
        {
            infoParts = information.Split(new[] { splitChar }, StringSplitOptions.None);
        }
        else
        {
            var parts = new List<string>();
            int curPos = 0;
            while (curPos < information.Length)
            {
                int length = Math.Min(infoLength, information.Length - curPos);
                parts.Add(information.Substring(curPos, length));
                curPos += length;
            }
            infoParts = parts.ToArray();
        }

        // First get the information string split up by line
        var outputParts = new List<string>();
        string curPart = "";
        foreach (var infoPart in infoParts)
        {
            if (curPart.Length + 1 + infoPart.Length > infoLength)
            {
                if (!string.IsNullOrEmpty(curPart))
                {
                    if (splitChar != ' ')
                    {
                        curPart += splitChar;
                    }
                    outputParts.Add(curPart);
                }
                curPart = infoPart;
            }
            else
            {
                if (string.IsNullOrEmpty(curPart))
                {
                    curPart = infoPart;
                }
                else
                {
                    curPart += splitChar + infoPart;
                }
            }
        }

        // Add the last bit of information to the output
        if (!string.IsNullOrEmpty(curPart))
        {
            outputParts.Add(curPart);
        }

        // Now format the information string for return
        var outputInfo = outputParts[0] + "\n";
        foreach (var outputPart in outputParts.Skip(1))
        {
            outputInfo += new string(' ', indent) + outputPart + "\n";
        }

        return outputInfo;
    }
}
