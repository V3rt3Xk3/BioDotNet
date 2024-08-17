using Bio.Core.Alphabets;
using Bio.Core.Sequences;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.IO.Iterators;

/// <summary>
/// Base class for building SeqRecord iterators.
///
/// You should write a parse method that returns a SeqRecord generator.You
/// may wish to redefine the __init__ method as well.
/// </summary>
public abstract class SequenceIterator : IEnumerable<Sequence>
{
    protected Stream stream;
    protected bool shouldCloseStream;
    protected IEnumerable<Sequence> records;
    protected IEnumerator<Sequence> recordsEnumerator;

    /// <summary>
    /// Create a SequenceIterator object.
    ///
    /// <list type="bullet">
    /// <item>
    /// <description>source - input file stream, or path to input file</description>
    /// </item>
    /// <item>
    /// <description>alphabet - no longer used, should be None</description>
    /// </item>
    /// </list>
    /// <para>This method MAY be overridden by any subclass.</para>
    /// <para>Note when subclassing:</para>
    /// <list type="bullet">
    /// <item>
    /// <description>There should be a single non-optional argument, the source.</description>
    /// </item>
    /// <item>
    /// <description>You do not have to require an alphabet.</description>
    /// </item>
    /// <item>
    /// <description>You can add additional optional arguments.</description>
    /// </item>
    /// </list>
    /// </summary>
    /// <param name="source"></param>
    /// <param name="mode"></param>
    /// <param name="fmt"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public SequenceIterator(object source, string mode = "t", string fmt = null)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source), "Source cannot be null.");
        }

        if (mode != "t" && mode != "b")
        {
            throw new ArgumentException($"Unknown mode '{mode}'", nameof(mode));
        }

        if (source is string path)
        {
            this.stream = new FileStream(path, FileMode.Open, mode == "t" ? FileAccess.Read : FileAccess.ReadWrite);
            this.shouldCloseStream = true;
        }
        else if (source is Stream streamSource)
        {
            if (mode == "t" && !streamSource.CanRead || mode == "b" && !streamSource.CanRead)
            {
                throw new ArgumentException($"Stream must be opened in {(mode == "t" ? "text" : "binary")} mode.", nameof(source));
            }
            this.stream = streamSource;
            this.shouldCloseStream = false;
        }
        else
        {
            throw new ArgumentException("Source must be a file path or a Stream.", nameof(source));
        }

        try
        {
            this.records = Parse(this.stream);
            this.recordsEnumerator = this.records.GetEnumerator();
        }
        catch
        {
            if (this.shouldCloseStream)
            {
                this.stream.Close();
            }
            throw;
        }
    }

    /// <summary>
    /// Start parsing the file, and return a SeqRecord iterator.
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public abstract IEnumerable<Sequence> Parse(Stream stream);

    public IEnumerator<Sequence> GetEnumerator()
    {
        return records.GetEnumerator();
    }

    public Sequence? MoveNext()
    {
        if (recordsEnumerator.MoveNext())
        {
            Sequence current = recordsEnumerator.Current;
            return current;
        }
        else
        {
            stream.Close();
            return null;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
