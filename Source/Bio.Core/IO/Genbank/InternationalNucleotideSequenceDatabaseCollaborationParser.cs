using Bio.Core.IO.Genbank.Consumers;
using Bio.Core.Sequences;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Bio.Core.IO.Genbank;
public abstract class InternationalNucleotideSequenceDatabaseCollaborationParser
{
    public bool Debug { get; init; } = false;
    protected ILogger logger { get; init; }


    // Base class does not define these as constants because they need to be overridden in subclasses.
    public abstract string RecordStart { get; }
    public abstract int HeaderWidth { get; }
    public abstract List<string> FeatureStartMarkers { get; }
    public abstract List<string> FeatureEndMarkers { get; }
    public abstract int FeatureQualifierIndent { get; }
    public abstract string FeatureQualifierSpacer { get; }
    public abstract List<string> SequenceHeaders { get; }


    public StreamReader StreamReader { get; protected set; }
    public string? Line { get; protected set; } = string.Empty;
    public InternationalNucleotideSequenceDatabaseCollaborationParser(string filePath, bool debug = false)
    {
        Debug = debug;
        logger = new BioLogger("Genbank or EMBL Parser");

        // Assertions in C# are typically replaced with exceptions for invalid state
        if (RecordStart.Length != HeaderWidth)
            throw new InvalidOperationException("RecordStart length must be equal to HeaderWidth.");

        foreach (var marker in SequenceHeaders)
        {
            if (marker != marker.TrimEnd())
                throw new InvalidOperationException("All SequenceHeaders must not have trailing whitespace.");
        }

        if (FeatureQualifierSpacer.Length != FeatureQualifierIndent)
            throw new InvalidOperationException("FeatureQualifierSpacer length must be equal to FeatureQualifierIndent.");

        string fullPath = Path.GetFullPath(filePath);
        StreamReader = new StreamReader(fullPath);
        StreamReader.BaseStream.Position = 0;
    }

    public InternationalNucleotideSequenceDatabaseCollaborationParser(Stream inputStream, bool debug = false)
    {
        Debug = debug;
        logger = new BioLogger("Genbank or EMBL Parser");

        // Assertions in C# are typically replaced with exceptions for invalid state
        if (RecordStart.Length != HeaderWidth)
            throw new InvalidOperationException("RecordStart length must be equal to HeaderWidth.");

        foreach (var marker in SequenceHeaders)
        {
            if (marker != marker.TrimEnd())
                throw new InvalidOperationException("All SequenceHeaders must not have trailing whitespace.");
        }

        if (FeatureQualifierSpacer.Length != FeatureQualifierIndent)
            throw new InvalidOperationException("FeatureQualifierSpacer length must be equal to FeatureQualifierIndent.");

        StreamReader = new StreamReader(inputStream);
        StreamReader.BaseStream.Position = 0;
    }


    /// <summary>
    /// Read in lines until find the ID/LOCUS line, which is returned.
    /// Any preamble(such as the header used by the NCBI on ``*.seq.gz`` archives)
    /// will we ignored.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public string? FindStart()
    {
        while (true)
        {
            string? line;
            if (!string.IsNullOrEmpty(Line))
            {
                line = Line;
                Line = "";
            }
            else
            {
                line = StreamReader.ReadLine();
            }

            if (line == null)
            {
                if (Debug)
                {
                    logger.LogWarning("End of file");
                }
                return null;
            }

            if (char.IsDigit(line[0]))
            {
                throw new ArgumentException("Is this handle in binary mode not text mode?");
            }

            if (line.StartsWith(RecordStart))
            {
                if (Debug)
                {
                    logger.LogWarning($"Found the start of a record:\n{line}");
                }
                Line = line;
                return line;
            }

            line = line.TrimEnd();

            if (line == "//")
            {
                if (Debug)
                {
                    logger.LogWarning("Skipping // marking end of last record");
                }
            }
            else if (line == "")
            {
                if (Debug)
                {
                    logger.LogWarning("Skipping blank line before record");
                }
            }
            else
            {
                if (Debug)
                {
                    logger.LogWarning($"Skipping header line before record:\n{line}");
                }
            }
        }
    }

    /// <summary>
    /// Return list of strings making up the header.
    /// New line characters are removed.
    /// Assumes you have just read in the ID/LOCUS line.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public List<string> ParseHeader()
    {
        if (Line?.Substring(0, HeaderWidth) != RecordStart)
        {
            throw new ArgumentException("Not at start of record");
        }

        var headerLines = new List<string>();
        string? line = null;
        while (true)
        {
            line = StreamReader.ReadLine();
            if (line is null)
            {
                throw new ArgumentException("Premature end of line during sequence data");
            }
            line = line.TrimEnd();

            if (FeatureStartMarkers.Contains(line))
            {
                if (Debug)
                {
                    logger.LogWarning("Found feature table");
                }
                break;
            }

            if (SequenceHeaders.Contains(line.Substring(0, Math.Min(line.Length, HeaderWidth)).TrimEnd()))
            {
                if (Debug)
                {
                    logger.LogWarning("Found start of sequence");
                }
                break;
            }

            if (line == "//")
            {
                throw new ArgumentException("Premature end of sequence data marker '//' found");
            }

            headerLines.Add(line);
        }

        Line = line;
        return headerLines;
    }


    
    public List<Feature> ParseFeatures(bool skip = false)
    {
        // Assuming FeatureStartMarkers, FEATURE_END_MARKERS, SEQUENCE_HEADERS, HEADER_WIDTH, FEATURE_QUALIFIER_INDENT,
        // FEATURE_QUALIFIER_SPACER, and BiopythonParserWarning are defined elsewhere in the class,
        // as well as a method parse_feature(string featureKey, List<string> featureLines) that returns a tuple.

        if (!FeatureStartMarkers.Contains(Line?.TrimEnd() ?? string.Empty))
        {
            if (Debug)
            {
                logger.LogWarning("Didn't find any feature table");
            }
            return new List<Feature>();
        }

        while (FeatureStartMarkers.Contains(Line?.TrimEnd() ?? string.Empty))
        {
            Line = StreamReader.ReadLine();
        }

        var features = new List<Feature>();
        string? line = Line;

        while (true)
        {
            if (string.IsNullOrEmpty(line))
            {
                throw new Exception("Premature end of line during features table");
            }
            if (SequenceHeaders.Contains(line.Substring(0, HeaderWidth).TrimEnd()))
            {
                if (Debug)
                {
                    logger.LogWarning("Found start of sequence");
                }
                break;
            }

            line = line.TrimEnd();
            if (line == "//")
            {
                throw new Exception("Premature end of features table, marker '//' found");
            }
            if (FeatureEndMarkers.Contains(line))
            {
                if (Debug)
                {
                    logger.LogWarning("Found end of features");
                }
                line = StreamReader.ReadLine();
                break;
            }
            if (line.Substring(2, FeatureQualifierIndent - 2).Trim() == "")
            {
                line = StreamReader.ReadLine();
                continue;
            }
            if (line.Length < FeatureQualifierIndent)
            {
                // Assuming BiopythonParserWarning is a method to log warnings
                logger.LogWarning($"line too short to contain a feature: {line}");
                line = StreamReader.ReadLine();
                continue;
            }

            if (skip)
            {
                line = StreamReader.ReadLine();
                while (line?.Substring(0, FeatureQualifierIndent) == FeatureQualifierSpacer)
                {
                    line = StreamReader.ReadLine();
                }
            }
            else
            {
                string featureKey;
                List<string> featureLines;
                if (line[FeatureQualifierIndent] != ' ' && line.Substring(FeatureQualifierIndent).Contains(" "))
                {
                    var parts = line.Substring(2).Trim().Split(new char[] { ' ' }, 2);
                    featureKey = parts[0];
                    featureLines = new List<string> { parts[1] };
                    logger.LogWarning($"Over indented {featureKey} feature?");
                }
                else
                {
                    featureKey = line.Substring(2, FeatureQualifierIndent - 2).Trim();
                    featureLines = new List<string> { line.Substring(FeatureQualifierIndent) };
                }
                line = StreamReader.ReadLine();
                while (line.Substring(0, FeatureQualifierIndent) == FeatureQualifierSpacer || (line != "" && line.TrimEnd() == ""))
                {
                    featureLines.Add(line.Substring(FeatureQualifierIndent).Trim());
                    line = StreamReader.ReadLine();
                }
                features.Add(ParseFeature(featureKey, featureLines));
            }
        }
        Line = line;
        return features;
    }


    ///<summary>
    ///Parses a feature given as a list of strings into a tuple.
    ///</summary>
    ///<remarks>
    ///Expects a feature as a list of strings and returns a tuple 
    ///consisting of a key, location, and qualifiers.
    ///
    ///For example, given a GenBank feature like the following:
    ///
    ///<code>
    ///CDS complement(join(490883..490885,1..879))
    ///             /locus_tag="NEQ001"
    ///             /note="conserved hypothetical [Methanococcus jannaschii];
    ///             COG1583:Uncharacterized ACR; IPR001472:Bipartite nuclear
    ///             localization signal; IPR002743: Protein of unknown
    ///             function DUF57"
    ///             /codon_start=1
    ///             /transl_table=11
    ///             /product="hypothetical protein"
    ///             /protein_id="NP_963295.1"
    ///             /db_xref="GI:41614797"
    ///             /db_xref="GeneID:2732620"
    ///             /translation="MRLLLELKALNSIDKKQLSNYLIQGFIYNILKNTEYSWLHNWKK
    ///             EKYFNFTLIPKKDIIENKRYYLIISSPDKRFIEVLHNKIKDLDIITIGLAQFQLRKTK
    ///             KFDPKLRFPWVTITPIVLREGKIVILKGDKYYKVFVKRLEELKKYNLIKKKEPILEEP
    ///             IEISLNQIKDGWKIIDVKDRYYDFRNKSFSAFSNWLRDLKEQSLRKYNNFCGKNFYFE
    ///             EAIFEGFTFYKTVSIRIRINRGEAVYIGTLWKELNVYRKLDKEEREFYKFLYDCGLGS
    ///             LNSMGFGFVNTKKNSAR"
    ///</code>
    ///
    ///This should be parsed into a tuple with the key as "CDS", the location 
    ///as a string, and the qualifiers as a list of string tuples.
    ///
    ///The returned tuple contains:
    ///- A key as a string.
    ///- A location string.
    ///- A list of qualifiers as list of string tuples.
    ///
    ///Qualifiers example:
    ///<code>
    ///     [('locus_tag', '"NEQ001"'),
    ///     ('note', '"conserved hypothetical [Methanococcus jannaschii];\nCOG1583:..."'),
    ///     ('codon_start', '1'),
    ///     ('transl_table', '11'),
    ///     ('product', '"hypothetical protein"'),
    ///     ('protein_id', '"NP_963295.1"'),
    ///     ('db_xref', '"GI:41614797"'),
    ///     ('db_xref', '"GeneID:2732620"'),
    ///     ('translation', '"MRLLLELKALNSIDKKQLSNYLIQGFIYNILKNTEYSWLHNWKK\nEKYFNFT..."')]
    ///</code>
    ///
    ///Note: In the example, the "note" and "translation" qualifiers were 
    ///edited for compactness and would contain multiple newline characters 
    ///as shown.
    ///
    ///If a qualifier is quoted, then the quotes are NOT removed.No 
    ///whitespace is removed from the beginning or end of the strings.
    ///</remarks>
    ///<returns>
    ///A tuple containing the key as a string, the location as a string, 
    ///and the qualifiers as a list of string tuples.
    ///</returns>
    public Feature ParseFeature(string featureKey, List<string> featureLines)
    {
        IEnumerable<string> iterator = featureLines.Where(x => !string.IsNullOrEmpty(x));
        try
        {
            string? line = iterator.FirstOrDefault();
            if (line == null) throw new InvalidOperationException("Sequence contains no elements");

            string featureLocation = line.Trim();
            while (featureLocation.EndsWith(","))
            {
                // Multiline location, still more to come!
                line = iterator.SkipWhile(x => x != line).Skip(1).FirstOrDefault();
                if (line == null) throw new InvalidOperationException("Sequence contains no elements");
                featureLocation += line.Trim();
            }
            if (featureLocation.Count(x => x == '(') > featureLocation.Count(x => x == ')'))
            {
                // Including the prev line in warning would be more explicit,
                // but this way get one-and-only-one warning shown by default:
                logger.LogWarning("Non-standard feature line wrapping (didn't break on comma)?");
                while (featureLocation.EndsWith(",") || featureLocation.Count(x => x == '(') > featureLocation.Count(x => x == ')'))
                {
                    line = iterator.SkipWhile(x => x != line).Skip(1).FirstOrDefault();
                    if (line == null) throw new InvalidOperationException("Sequence contains no elements");
                    featureLocation += line.Trim();
                }
            }

            List<Qualifier> qualifiers = new List<Qualifier>();

            foreach (var (foreachLine, index) in iterator.Select((value, index) => (value, index)))
            {
                // check for extra wrapping of the location closing parentheses
                if (index == 0 && foreachLine.StartsWith(")"))
                {
                    featureLocation += foreachLine.Trim();
                }
                else if (foreachLine.StartsWith("/"))
                {
                    // New qualifier
                    int i = foreachLine.IndexOf("=");
                    string key = foreachLine.Substring(1, i - 1); // does not work if i==-1
                    string? value = i != -1 ? foreachLine.Substring(i + 1) : null; // we ignore 'value' if i==-1
                    if (i != -1 && (value?.StartsWith(" ") ?? false) && value.TrimStart().StartsWith("\""))
                    {
                        logger.LogWarning("White space after equals in qualifier");
                        value = value.TrimStart();
                    }
                    if (i == -1)
                    {
                        // Qualifier with no key, e.g. /pseudo
                        key = foreachLine.Substring(1);
                        qualifiers.Add(new Qualifier(key));
                    }
                    else if (string.IsNullOrEmpty(value))
                    {
                        // ApE can output /note=
                        qualifiers.Add(new Qualifier(key));
                    }
                    else if (value == "\"")
                    {
                        // One single quote
                        logger.LogWarning($"Single quote {key}:{value}");
                        // DO NOT remove the quote...
                        qualifiers.Add(new Qualifier(key, value));
                    }
                    else if (value.StartsWith("\""))
                    {
                        // Quoted...
                        List<string> valueList = new List<string> { value };
                        while (!valueList.Last().EndsWith("\""))
                        {
                            line = iterator.SkipWhile(x => x != foreachLine).Skip(1).FirstOrDefault();
                            if (foreachLine == null) throw new InvalidOperationException("Sequence contains no elements");
                            valueList.Add(foreachLine);
                        }
                        value = string.Join("\n", valueList);
                        // DO NOT remove the quotes...
                        qualifiers.Add(new Qualifier(key, value));
                    }
                    else
                    {
                        // Unquoted
                        qualifiers.Add(new Qualifier(key, value));
                    }
                }
                else
                {
                    // Unquoted continuation
                    if (!qualifiers.Any()) throw new InvalidOperationException("No qualifiers to continue");
                    var lastQualifier = qualifiers.Last();
                    if (lastQualifier.Value == null) throw new InvalidOperationException("Last qualifier value is null");
                    qualifiers[qualifiers.Count - 1] = new Qualifier(lastQualifier.Key, lastQualifier.Value + "\n" + foreachLine);
                }
            }
            return (new Feature(featureKey, featureLocation, qualifiers));
        }
        catch (InvalidOperationException)
        {
            // Bummer
            throw new Exception($"Problem with '{featureKey}' feature:\n{string.Join("\n", featureLines)}");
        }
    }

    /// <summary>
    /// Return an ISequence (with SeqFeatures if do_features=True).
    /// See also the method ParseRecords() for use on multi-record files.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="doFeatures"></param>
    /// <returns></returns>
    public ISequence Parse(StreamReader reader, bool doFeatures = true)
    {
        // Assuming _FeatureConsumer and FeatureValueCleaner are implemented in C#
        var consumer = new FeatureConsumer(useFuzziness: true, featureCleaner: new FeatureValueCleaner());

        if (Feed(reader, consumer, doFeatures))
        {
            return consumer.Data;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Parse records, return a SeqRecord object iterator.
    ///Each record(from the ID/LOCUS line to the // line) becomes a SeqRecord
    ///The SeqRecord objects include SeqFeatures if do_features= True
    ///This method is intended for use in Bio.SequenceIO
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="doFeatures"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public IEnumerable<ISequence> ParseRecords(Stream handle, bool doFeatures = true)
    {
        // Assuming there's a method to properly open and handle the stream in C#
        using (StreamReader reader = new StreamReader(handle))
        {
            while (!reader.EndOfStream)
            {
                ISequence record = Parse(reader, doFeatures);
                if (record == null)
                {
                    break;
                }
                if (record.ID == null)
                {
                    throw new ArgumentException("Failed to parse the record's ID. Invalid ID line?");
                }
                if (record.Name == "<unknown name>")
                {
                    throw new ArgumentException("Failed to parse the record's name. Invalid ID line?");
                }
                if (record.Description == "<unknown description>")
                {
                    throw new ArgumentException("Failed to parse the record's description");
                }
                yield return record;
            }
        }
    }

    /// <summary>
    /// Feed a set of data into the consumer.
    /// This method is intended for use with the "old" code in Bio.GenBank
    /// </summary>
    /// <param name="handle">A handle with the information to parse.</param>
    /// <param name="consumer">The consumer that should be informed of events.</param>
    /// <param name="doFeatures">Boolean, should the features be parsed? Skipping the features can be much faster.</param>
    /// <returns>true - Passed a record or false - Did not find a record</returns>
    public bool Feed(StreamReader handle, BaseGenBankConsumer consumer, bool doFeatures = true)
    {
        // Feed a set of data into the consumer.
        // This method is intended for use with the "old" code in Bio.GenBank

        // Arguments:
        // - handle - A handle with the information to parse.
        // - consumer - The consumer that should be informed of events.
        // - doFeatures - Boolean, should the features be parsed?
        //   Skipping the features can be much faster.

        // Return values:
        // - true  - Passed a record
        // - false - Did not find a record

        this.StreamReader = handle;
        if (FindStart() is null)
        {
            // Could not find (another) record
            consumer.Data = new Sequence();
            return false;
        }

        // We use the above class methods to parse the file into a simplified format.
        // The first line, header lines, and any misc lines after the features will be
        // dealt with by GenBank / EMBL specific derived classes.

        // First line and header:
        FeedFirstLine(consumer, Line);
        FeedHeaderLines(consumer, ParseHeader());

        // Features (common to both EMBL and GenBank):
        if (doFeatures)
        {
            FeedFeatureTable(consumer, ParseFeatures(skip: false));
        }
        else
        {
            ParseFeatures(skip: true); // ignore the data
        }

        // Footer and sequence
        var (miscLines, sequenceString) = ParseFooter();
        FeedMiscLines(consumer, miscLines);

        consumer.Sequence(sequenceString);
        // Calls to consumer.BaseNumber() do nothing anyway
        consumer.RecordEnd("//");

        Debug.Assert(this.line == "//");

        // And we are done
        return true;
    }

    /// <summary>
    /// Handle the LOCUS/ID line, passing data to the consumer (PRIVATE).
    /// This should be implemented by the EMBL / GenBank specific subclass
    /// Used by the parse_records() and parse() methods
    /// </summary>
    /// <param name="consumer"></param>
    /// <param name="line"></param>
    protected abstract void FeedFirstLine(BaseGenBankConsumer consumer, string? line);


    /// <summary>
    /// Handle the header lines (list of strings), 
    /// passing data to the consumer (PRIVATE).
    /// This should be implemented by the EMBL / GenBank specific subclass
    /// Used by the parse_records() and parse() methods.
    /// </summary>
    /// <param name="consumer"></param>
    /// <param name="headerLines"></param>
    protected abstract void FeedHeaderLines(BaseGenBankConsumer consumer, List<string> headerLines);

    /// <summary>
    /// Handle the feature table (list of tuples), passing 
    /// data to the consumer (PRIVATE).
    /// Used by the parse_records() and parse() methods.
    /// </summary>
    /// <param name="consumer"></param>
    /// <param name="featureTuples"></param>
    public static void FeedFeatureTable(BaseGenBankConsumer consumer, List<Tuple<string, string, List<Tuple<string, string>>>> featureTuples)
    {
        // Handle the feature table (list of tuples), passing data to the consumer (PRIVATE).
        // Used by the parse_records() and parse() methods.
        consumer.StartFeatureTable();
        foreach (var featureTuple in featureTuples)
        {
            var featureKey = featureTuple.Item1;
            var locationString = featureTuple.Item2;
            var qualifiers = featureTuple.Item3;

            consumer.FeatureKey(featureKey);
            consumer.Location(locationString);
            foreach (var qualifier in qualifiers)
            {
                var qKey = qualifier.Item1;
                var qValue = qualifier.Item2;

                if (qValue == null)
                {
                    consumer.FeatureQualifier(qKey, qValue);
                }
                else
                {
                    consumer.FeatureQualifier(qKey, qValue.Replace("\n", " "));
                }
            }
        }
    }
}
