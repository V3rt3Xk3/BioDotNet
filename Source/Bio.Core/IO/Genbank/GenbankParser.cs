using Bio.Core.IO.Genbank.Consumers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace Bio.Core.IO.Genbank;
public class GenbankParser : InternationalNucleotideSequenceDatabaseCollaborationParser
{
    // Override properties with specific values for GenBank files
    public override string RecordStart { get; } = "LOCUS       ";
    public override int HeaderWidth { get; } = 12;
    public override List<string> FeatureStartMarkers { get; } = new List<string> { "FEATURES             Location/Qualifiers", "FEATURES" };
    public override List<string> FeatureEndMarkers { get; } = new List<string>();  // Empty list
    public override int FeatureQualifierIndent { get; } = 21;
    public override string FeatureQualifierSpacer { get; } = new string(' ', 21);
    public override List<string> SequenceHeaders { get; } = new List<string> { "CONTIG", "ORIGIN", "BASE COUNT", "WGS", "TSA", "TLS" };
    public int GenBankIndent { get; } = 12;  // Use HeaderWidth, 12 is the magic number
    public string GenBankSpacer { get; } = new string(' ', 12); // Use HeaderWidth, 12 is the magic number
    public string StructuredCommentStart { get; } = "-START##";
    public string StructuredCommentEnd { get; } = "-END##";
    public string StructuredCommentDelim { get; } = " :: ";

    public GenbankParser(string filePath, bool debug) : base(filePath, debug) { }

    public GenbankParser(Stream inputStream, bool debug) : base(inputStream, debug) { }

    public (List<string>, string) ParseFooter()
    {
        if (!SequenceHeaders.Contains(Line.Substring(0, HeaderWidth).Trim()) && this.line.Substring(0, HEADER_WIDTH) != new string(' ', HEADER_WIDTH))
        {
            throw new ArgumentException($"Footer format unexpected: '{this.line}'");
        }

        var miscLines = new List<string>();
        while (SequenceHeaders.Contains(Line.Substring(0, HeaderWidth).Trim()) ||
               Line.Substring(0, HeaderWidth) == new string(' ', HeaderWidth) ||
               Line.Substring(0, 3) == "WGS")
        {
            miscLines.Add(Line.TrimEnd());
            Line = StreamReader.ReadLine();
            if (Line == null)
            {
                throw new ArgumentException("Premature end of file");
            }
        }

        if (SEQUENCE_HEADERS.Contains(this.line.Substring(0, HEADER_WIDTH).Trim()))
        {
            throw new ArgumentException($"Eh? '{this.line}'");
        }

        var seqLines = new List<string>();
        while (true)
        {
            if (string.IsNullOrEmpty(this.line))
            {
                Console.WriteLine("Premature end of file in sequence data");
                this.line = "//";
                break;
            }

            this.line = this.line.TrimEnd();
            if (string.IsNullOrEmpty(this.line))
            {
                Console.WriteLine("Blank line in sequence data");
                this.line = this.handle.ReadLine();
                continue;
            }

            if (this.line == "//" || this.line.StartsWith("CONTIG"))
            {
                break;
            }

            if (this.line.Length > 9 && this.line[9] != ' ')
            {
                Console.WriteLine("Invalid indentation for sequence line");
                this.line = this.line.Substring(1);
                if (this.line.Length > 9 && this.line[9] != ' ')
                {
                    throw new ArgumentException($"Sequence line mal-formed, '{this.line}'");
                }
            }

            seqLines.Add(this.line.Substring(10)); // Assuming sequence data starts at index 10
            this.line = this.handle.ReadLine();
        }

        return (miscLines, string.Concat(seqLines).Replace(" ", ""));
    }

    /// <summary>
    /// Scan over and parse GenBank LOCUS line (PRIVATE).
    /// 
    /// This must cope with several variants, primarily the old and new column
    /// based standards from GenBank.Additionally EnsEMBL produces GenBank
    /// files where the LOCUS line is space separated rather that following
    /// the column based layout.
    ///
    /// We also try to cope with GenBank like files with partial LOCUS lines.
    /// 
    /// As of release 229.0, the columns are no longer strictly in a given
    /// position.See GenBank format release notes:
    ///
    /// "Historically, the LOCUS line has had a fixed length and its
    /// elements have been presented at specific column positions...
    /// But with the anticipated increases in the lengths of accession
    /// numbers, and the advent of sequences that are gigabases long,
    /// maintaining the column positions will not always be possible and
    /// the overall length of the LOCUS line could exceed 79 characters."
    /// </summary>
    /// <param name="consumer"></param>
    /// <param name="line"></param>
    protected override void FeedFirstLine(BaseGenBankConsumer consumer, string? line)
    {
        List<string> residueTypes = new List<string>() { " bp ", " aa ", " rc " };
        List<string> topologyOptions = new List<string>() { "", "linear", "circular" };

        bool lineStartsWithLocus = line?.StartsWith("LOCUS") ?? false;
        // Check if the line starts with "LOCUS"
        if (!lineStartsWithLocus)
        {
            throw new ArgumentException("LOCUS line does not start correctly:\n" + line);
        }

        // Old format LOCUS line
        bool residueTypeIsDeclaredAtChar29 = residueTypes.Contains(line.Substring(29, 4));
        bool emptyStringAtChar55to62 = string.IsNullOrWhiteSpace(line.Substring(55, 7));

        // New format LOCUS line
        bool residueTypeIsDeclaredAtChar40 = residueTypes.Contains(line.Substring(40, 4));
        bool topologyIsDeclaredAtChar54 = topologyOptions.Contains(line.Substring(54, 10).Trim());

        // Truncated LOCUS line
        bool truncatedLocusLine = line.Substring(this.GenBankIndent).Trim().Count(c => c == ' ') == 0;

        // Malformed GenBank file handling:
        string[] splitLine = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Invalid spacing LOCUS line
        bool splitLineLengthIs8 = splitLine.Length == 8;
        bool splitLine4thElementIsAAorBP = new string[] {"aa", "bp" }.Contains(splitLine[3].ToLower());
        bool splitLine6thElementIsTopology = new string[] { "linear", "circular" }.Contains(splitLine[5].ToLower());

        // EnsEMBL LOCUS lines
        bool splitLineLengthIs7 = splitLine.Length == 7;
        // bool splitLine4thElementIsAAorBP

        // EMBOSS seqret output
        bool splitLineLengthIsAtLeast4 = 4 <= splitLine.Length;
        // bool splitLine4thElementIsAAorBP

        // Cope with pseudo-GenBank files
        // bool splitLineLengthIsAtLeast4
        bool lastElementInSplitLineIsResidueType = new string[] { "aa", "bp" }.Contains(splitLine[^1].ToLower());

        if (residueTypeIsDeclaredAtChar29 && emptyStringAtChar55to62)
            ParseOldFormat(consumer, line);
        else if (residueTypeIsDeclaredAtChar40 && topologyIsDeclaredAtChar54)
            ParseNewFormat(consumer, line);
        else if (truncatedLocusLine)
            ParseTruncatedFormat(consumer, line);
        else if (splitLineLengthIs8 && splitLine4thElementIsAAorBP && splitLine6thElementIsTopology)
            ParseInvalidlySpacedFormat(consumer, line, splitLine);
        else if (splitLineLengthIs7 && splitLine4thElementIsAAorBP)
            ParseEnsEMBLFormat(consumer, line, splitLine);
        else if (splitLineLengthIsAtLeast4 && splitLine4thElementIsAAorBP)
            ParseEMBOSSSeqRetOutputFormat(consumer, line, splitLine);
        else if (splitLineLengthIsAtLeast4 && lastElementInSplitLineIsResidueType)
            ParsePseudoGenbankFormat(consumer, line, splitLine);
        else
            throw new InvalidDataException("Did not recognise the LOCUS line layout:\n" + line);

    }

    private void ParseOldFormat(BaseGenBankConsumer consumer, string line)
    {
        // Old... note we insist on the 55:62 being empty to avoid trying
        // to parse space separated LOCUS lines from Ensembl etc, see below.
        //
        // Positions  Contents
        //    ---------  --------
        //    00:06      LOCUS
        //    06:12      spaces
        //    12:??      Locus name
        //    ??:??      space
        //    ??:29      Length of sequence, right-justified
        //    29:33      space, bp, space
        //    33:41      strand type / molecule type, e.g. DNA
        //    41:42      space
        //    42:51      Blank (implies linear), linear or circular
        //    51:52      space
        //    52:55      The division code (e.g. BCT, VRL, INV)
        //    55:62      space
        //    62:73      Date, in the form dd-MMM-yyyy (e.g., 15-MAR-1991)
        //
        // assert line[29:33] in [' bp ', ' aa ',' rc '] , \
        //       'LOCUS line does not contain size units at expected position:\n' + line

        // Extract and process various parts of the LOCUS line in the old format
        // Similar to the Python code, but adapted for C#
        // This is a simplified example, focusing on the structure rather than full implementation

        if (line[41] != ' ')
            throw new ArgumentException("LOCUS line does not contain space at position 42:\n" + line);


        List<string> topologyOptions = new List<string>() { "", "linear", "circular" };
        if (!topologyOptions.Contains(line.Substring(42, 9).Trim()))
            throw new ArgumentException("LOCUS line does not contain valid entry (linear, circular, ...):\n" + line);

        if (line[51] != ' ')
            throw new ArgumentException("LOCUS line does not contain space at position 52:\n" + line);

        if (!string.IsNullOrWhiteSpace(line.Substring(62, 11)))
        {
            if (line[64] != '-')
            {
                throw new ArgumentException("LOCUS line does not contain '-' at position 65 in date:\n" + line);
            }
            if (line[68] != '-')
            {
                throw new ArgumentException("LOCUS line does not contain '-' at position 69 in date:\n" + line);
            }
        }

        string nameAndLengthStr = line.Substring(GenBankIndent, 29 - GenBankIndent).Replace("  ", " ");
        string[] nameAndLength = nameAndLengthStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (nameAndLength.Length > 2)
        {
            throw new ArgumentException("Cannot parse the name and length in the LOCUS line:\n" + line);
        }
        if (nameAndLength.Length == 1)
        {
            throw new ArgumentException("Name and length collide in the LOCUS line:\n" + line);
        }

        string name = nameAndLength[0];
        string length = nameAndLength[1];
        if (name.Length > 16)
        {
            // Warning about name length, not throwing an exception
            Console.WriteLine("Warning: GenBank LOCUS line identifier over 16 characters");
        }
        consumer.Locus(name);
        consumer.Size(length);

        if (line.Substring(33, 18).Trim() == "" && line.Substring(29, 4) == " aa ")
        {
            consumer.ResidueType("PROTEIN");
        }
        else
        {
            consumer.ResidueType(line.Substring(33, 18).Trim());
        }

        consumer.MoleculeType(line.Substring(33, 8).Trim());
        consumer.Topology(line.Substring(42, 9).Trim());
        consumer.DataFileDivision(line.Substring(52, 3));
        if (!string.IsNullOrWhiteSpace(line.Substring(62, 11)))
        {
            consumer.Date(line.Substring(62, 11));
        }
    }
    private void ParseNewFormat(BaseGenBankConsumer consumer, string line)
    {
        // New... linear/circular/big blank test should avoid EnsEMBL style
        // LOCUS line being treated like a proper column based LOCUS line.
        //
        // Positions  Contents
        //    ---------  --------
        //    00:06      LOCUS
        //    06:12      spaces
        //    12:??      Locus name
        //    ??:??      space
        //    ??:40      Length of sequence, right-justified
        //    40:44      space, bp, space
        //    44:47      Blank, ss-, ds-, ms-
        //    47:54      Blank, DNA, RNA, tRNA, mRNA, uRNA, snRNA, cDNA
        //    54:55      space
        //    55:63      Blank (implies linear), linear or circular
        //    63:64      space
        //    64:67      The division code (e.g. BCT, VRL, INV)
        //    67:68      space
        //    68:79      Date, in the form dd-MMM-yyyy (e.g., 15-MAR-1991)
        //
        if (line.Length < 79)
        {
            // Assuming BiopythonParserWarning is a custom exception or warning type in your C# project
            if (Debug)
                Console.WriteLine($"Truncated LOCUS line found - is this correct?\n:{line}");
            int paddingLen = 79 - line.Length;
            line += new string(' ', paddingLen);
        }

        if (!new[] { " bp ", " aa ", " rc " }.Contains(line.Substring(40, 4)))
        {
            throw new ArgumentException("LOCUS line does not contain sequence type at expected position:\n" + line);
        }

        if (!new[] { "   ", "ss-", "ds-", "ms-" }.Contains(line.Substring(44, 3)))
        {
            throw new ArgumentException("LOCUS line does not have valid strand type (Single stranded, ...):\n" + line);
        }

        string sequenceType = line.Substring(47, 7).Trim().ToUpper();
        if (sequenceType != "" && !sequenceType.Contains("DNA") && !sequenceType.Contains("RNA"))
        {
            throw new ArgumentException("LOCUS line does not contain valid sequence type (DNA, RNA, ...):\n" + line);
        }

        if (line[54] != ' ')
        {
            throw new ArgumentException("LOCUS line does not contain space at position 55:\n" + line);
        }

        if (!new[] { "", "linear", "circular" }.Contains(line.Substring(55, 8).Trim()))
        {
            throw new ArgumentException("LOCUS line does not contain valid entry (linear, circular, ...):\n" + line);
        }

        if (line[63] != ' ')
        {
            throw new ArgumentException("LOCUS line does not contain space at position 64:\n" + line);
        }

        if (line[67] != ' ')
        {
            throw new ArgumentException("LOCUS line does not contain space at position 68:\n" + line);
        }

        if (!string.IsNullOrWhiteSpace(line.Substring(68, 11)))
        {
            if (line[70] != '-')
            {
                throw new ArgumentException("LOCUS line does not contain '-' at position 71 in date:\n" + line);
            }
            if (line[74] != '-')
            {
                throw new ArgumentException("LOCUS line does not contain '-' at position 75 in date:\n" + line);
            }
        }

        string nameAndLengthStr = line.Substring(GenBankIndent, 40-GenBankIndent).Replace("  ", " ");
        string[] nameAndLength = nameAndLengthStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (nameAndLength.Length > 2)
        {
            throw new ArgumentException("Cannot parse the name and length in the LOCUS line:\n" + line);
        }
        if (nameAndLength.Length == 1)
        {
            throw new ArgumentException("Name and length collide in the LOCUS line:\n" + line);
        }
        consumer.Locus(nameAndLength[0]);
        consumer.Size(nameAndLength[1]);

        if (line.Substring(44, 10).Trim() == "" && line.Substring(40, 4) == " aa ")
        {
            consumer.ResidueType(("PROTEIN " + line.Substring(54, 9)).Trim());
        }
        else
        {
            consumer.ResidueType(line.Substring(44, 19).Trim());
        }

        consumer.MoleculeType(line.Substring(44, 10).Trim());
        consumer.Topology(line.Substring(55, 8).Trim());
        if (!string.IsNullOrWhiteSpace(line.Substring(64, 12)))
        {
            consumer.DataFileDivision(line.Substring(64, 3));
        }
        if (!string.IsNullOrWhiteSpace(line.Substring(68, 11)))
        {
            consumer.Date(line.Substring(68, 11));
        }
    }
    private void ParseTruncatedFormat(BaseGenBankConsumer consumer, string line)
    {
        // Truncated LOCUS line, as produced by some EMBOSS tools - see bug 1762
        //
        // e.g.
        //
        //    "LOCUS       U00096"
        //
        // rather than:
        //
        //    "LOCUS       U00096               4639675 bp    DNA     circular BCT"
        //
        // Positions  Contents
        //    ---------  --------
        //    00:06      LOCUS
        //    06:12      spaces
        //    12:??      Locus name
        if (!string.IsNullOrWhiteSpace(line.Substring(this.GenBankIndent)))
        {
            consumer.Locus(line.Substring(this.GenBankIndent).Trim());
        }
        else
        {
            // Must just have just "LOCUS       ", is this even legitimate?
            // We should be able to continue parsing... we need real world testcases!
            if (Debug)
                Console.WriteLine($"Minimal LOCUS line found - is this correct?\n:{line}");
            // Assuming BiopythonParserWarning is a custom exception or warning type in your C# project
            // You might need to implement a similar warning mechanism or use exceptions/logging as appropriate.
        }
    }
    private void ParseInvalidlySpacedFormat(BaseGenBankConsumer consumer, string line, string[] splitLine)
    {
        // Cope with invalidly spaced GenBank LOCUS lines like
        // LOCUS       AB070938          6497 bp    DNA     linear   BCT 11-OCT-2001
        // This will also cope with extra long accession numbers and
        // sequence lengths
        consumer.Locus(splitLine[1]);

        // Provide descriptive error message if the sequence is too long
        // for C# to handle
        long sequenceLength = long.Parse(splitLine[2]);
        if (sequenceLength > Int64.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sequenceLength),
                $"Tried to load a sequence with a length {sequenceLength}, " +
                $"your installation of C# can only load sequences of length {Int64.MaxValue}"
            );
        }
        else
        {
            consumer.Size(splitLine[2]);
        }

        consumer.ResidueType(splitLine[4]);
        consumer.Topology(splitLine[5]);
        consumer.DataFileDivision(splitLine[6]);
        consumer.Date(splitLine[7]);

        if (line.Length < 80)
        {
            // Assuming BiopythonParserWarning is a custom exception or warning type in your C# project
            // You might need to implement a similar warning mechanism or use exceptions/logging as appropriate.
            if (Debug)
                Console.WriteLine(
                    $"Attempting to parse malformed locus line:\n{line}\n" +
                    $"Found locus {splitLine[1]} size {splitLine[2]} residue_type {splitLine[4]}\n" +
                    "Some fields may be wrong."
                );
        }
    }
    private void ParseEnsEMBLFormat(BaseGenBankConsumer consumer, string line, string[] splitLine)
    {
        // Cope with EnsEMBL genbank files which use space separation rather
        // than the expected column based layout. e.g.
        // LOCUS       HG531_PATCH 1000000 bp DNA HTG 18-JUN-2011
        // LOCUS       HG531_PATCH 759984 bp DNA HTG 18-JUN-2011
        // LOCUS       HG506_HG1000_1_PATCH 814959 bp DNA HTG 18-JUN-2011
        // LOCUS       HG506_HG1000_1_PATCH 1219964 bp DNA HTG 18-JUN-2011
        // Notice that the 'bp' can occur in the position expected by either
        // the old or the new fixed column standards (parsed above).
        consumer.Locus(splitLine[1]);
        consumer.Size(splitLine[2]);
        consumer.ResidueType(splitLine[4]);
        consumer.DataFileDivision(splitLine[5]);
        consumer.Date(splitLine[6]);
    }

    private void ParseEMBOSSSeqRetOutputFormat(BaseGenBankConsumer consumer, string line, string[] splitLine)
    {
        // Cope with EMBOSS seqret output where it seems the locus id can cause
        // the other fields to overflow. We just IGNORE the other fields!
        if (Debug)
            Console.WriteLine($"Malformed LOCUS line found - is this correct?\n:{line}");
        // Assuming BiopythonParserWarning is a custom exception or warning type in your C# project
        // You might need to implement a similar warning mechanism or use exceptions/logging as appropriate.

        consumer.Locus(splitLine[1]);
        consumer.Size(splitLine[2]);
    }

    private void ParsePseudoGenbankFormat(BaseGenBankConsumer consumer, string line, string[] splitLine)
    {
        // Cope with pseudo-GenBank files like this:
        //   "LOCUS       RNA5 complete       1718 bp"
        // Treat everything between LOCUS and the size as the identifier.
        if (Debug)
            Console.WriteLine($"Malformed LOCUS line found - is this correct?\n:{line}");
        // Assuming BiopythonParserWarning is a custom exception or warning type in your C# project
        // You might need to implement a similar warning mechanism or use exceptions/logging as appropriate.

        // In the above example:
        // "LOCUS       RNA5 complete       1718 bp"
        // We retain the RNA5 complete for the locus.
        consumer.Locus(line.Substring(5).Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Reverse().Skip(2).Reverse().Aggregate((current, next) => current + " " + next));
        consumer.Size(splitLine[^2]);
    }
}
