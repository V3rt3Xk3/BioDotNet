using Bio.Core.IO.Genbank;
using Bio.Core.Sequences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.IO.Iterators;
public class GenBankIterator : SequenceIterator
{
    /// <summary>
    /// Breaks up a GenBank file into SeqRecord objects.
    /// 
    /// Argument source is a file-like object opened in text mode or a path to a file.
    /// Every section from the LOCUS line to the terminating // becomes
    /// a single SeqRecord with associated annotation and features.
    /// 
    /// Note that for genomes or chromosomes, there is typically only
    /// one record.
    /// 
    /// This gets called internally by Bio.SeqIO for the GenBank file format:
    /// 
    /// Example usage:
    /// 
    /// foreach (var record in SeqIO.Parse("GenBank/cor6_6.gb", "gb"))
    /// {
    ///     Console.WriteLine(record.Id);
    /// }
    /// 
    /// Equivalently,
    /// 
    /// using (var handle = File.OpenRead("GenBank/cor6_6.gb"))
    /// {
    ///     foreach (var record in new GenBankIterator(handle))
    ///     {
    ///         Console.WriteLine(record.Id);
    ///     }
    /// }
    /// 
    /// </summary>
    /// <param name="source">The source file path or stream.</param>
    public GenBankIterator(object source) : base(source, "t", "GenBank") { }

    public override IEnumerable<Sequence> Parse(Stream handle)
    {
#if DEBUG
        bool debug = true;
#else
        bool debug = false;
#endif
        var records = new GenbankParser(stream, debug).ParseRecords(handle);
        return records;
    }
}
