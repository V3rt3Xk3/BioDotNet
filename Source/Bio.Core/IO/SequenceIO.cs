using Bio.Core.Alphabets;
using Bio.Core.Sequences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bio.Core.IO.Genbank;
using Bio.Core.IO.Iterators;

namespace Bio.Core.IO;
public static class SequenceIO
{
    static readonly Dictionary<string, Type> _FormatToIterator = new Dictionary<string, Type>
        {
            //{"abi", AbiIO.AbiIterator},
            //{"abi-trim", AbiIO.AbiTrimIterator},
            //{"ace", AceIO.AceIterator},
            //{"fasta", FastaIO.FastaIterator},
            //{"fasta-2line", FastaIO.FastaTwoLineIterator},
            //{"ig", IgIO.IgIterator},
            //{"embl", InsdcIO.EmblIterator},
            //{"embl-cds", InsdcIO.EmblCdsFeatureIterator},
            {"gb", typeof(GenBankIterator)},
            //{"gck", GckIO.GckIterator},
            {"genbank", typeof(GenBankIterator)}
            //{"genbank-cds", InsdcIO.GenBankCdsFeatureIterator},
            //{"gfa1", GfaIO.Gfa1Iterator},
            //{"gfa2", GfaIO.Gfa2Iterator},
            //{"imgt", InsdcIO.ImgtIterator},
            //{"nib", NibIO.NibIterator},
            //{"cif-seqres", PdbIO.CifSeqresIterator},
            //{"cif-atom", PdbIO.CifAtomIterator},
            //{"pdb-atom", PdbIO.PdbAtomIterator},
            //{"pdb-seqres", PdbIO.PdbSeqresIterator},
            //{"phd", PhdIO.PhdIterator},
            //{"pir", PirIO.PirIterator},
            //{"fastq", QualityIO.FastqPhredIterator},
            //{"fastq-sanger", QualityIO.FastqPhredIterator},
            //{"fastq-solexa", QualityIO.FastqSolexaIterator},
            //{"fastq-illumina", QualityIO.FastqIlluminaIterator},
            //{"qual", QualityIO.QualPhredIterator},
            //{"seqxml", SeqXmlIO.SeqXmlIterator},
            //{"sff", SffIO.SffIterator},
            //{"snapgene", SnapGeneIO.SnapGeneIterator},
            //{"sff-trim", SffIO.SffTrimIterator}, // Not sure about this in the long run
            //{"swiss", SwissIO.SwissIterator},
            //{"tab", TabIO.TabIterator},
            //{"twobit", TwoBitIO.TwoBitIterator},
            //{"uniprot-xml", UniprotIO.UniprotIterator},
            //{"xdna", XdnaIO.XdnaIterator},
        };

    public static IEnumerable<Sequence> Parse(string format, Stream handle)
    {
        if (!(format is string))
        {
            throw new ArgumentException("Need a string for the file format (lower case)", nameof(format));
        }
        if (string.IsNullOrEmpty(format))
        {
            throw new ArgumentException("Format required (lower case string)", nameof(format));
        }
        if (!format.Equals(format.ToLower()))
        {
            throw new ArgumentException($"Format string '{format}' should be lower case", nameof(format));
        }

        // The alphabet argument check is omitted as it's not relevant in C# context

        // Assuming _FormatToIterator is a Dictionary or similar mapping in C#
        // and it's available within this context
        if (_FormatToIterator.TryGetValue(format, out var iteratorGenerator))
        {
            var iterator = Activator.CreateInstance(iteratorGenerator, handle) as SequenceIterator;
            if (iterator == null)
            {
                throw new InvalidOperationException($"Could not create an instance of {iteratorGenerator.FullName}.");
            }
            return iterator;
        }
        else
        {
            throw new ArgumentException($"Unknown format '{format}'", nameof(format));
        }
    }
}
