using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

using Bio.Core.Alphabet;
using Bio.Core.Sequences;
using Bio.Core.Resources;


namespace Bio.Util
{
    /// <summary>
    /// Generally useful static methods.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// The .gz extension to indicate gzipped files
        /// </summary>
        public const string ZippedFileExtension = ".gz";

        /// <summary>
        /// Stores the number of alphabets to show in ToString function of a class.
        /// </summary>
        public const int AlphabetsToShowInToString = 64;

        /// <summary>
        /// Key to get GenBankMetadata object from Metadata of a sequence which is parsed from GenBankParser.
        /// </summary>
        public const string GenBankMetadataKey = "GenBank";

        /// <summary>
        /// Delimitar "!" used to seperate the PairedRead information with other info in the sequence id.
        /// </summary>
        public static char PairedReadDelimiter = '!';

        private const string Space = " ";
        private const string Colon = ":";
        private const string Comma = ",";
        private const string ProjectDBLink = "Project";
        private const string BioProjectDBLink = "BioProject";
        private const string TraceAssemblyArchiveDBLink = "Trace Assembly Archive";
        private const string SegmentDelim = " of ";
        private const string SingleStrand = "ss-";
        private const string DoubleStrand = "ds-";
        private const string MixedStrand = "ms-";
        private const string LinearStrandTopology = "linear";
        private const string CircularStrandTopology = "circular";

        /// <summary>
        /// Key to get SAMAlignmentHeader object from Metadata of a sequence alignment which is parsed from SAMParser.
        /// </summary>
        public const string SAMAlignmentHeaderKey = "SAMAlignmentHeader";

        /// <summary>
        /// Key to get SAMAlignedSequenceHeader object from Metadata of a aligned sequence which is parsed from SAMParser.
        /// </summary>
        public const string SAMAlignedSequenceHeaderKey = "SAMAlignedSequenceHeader";

        private static readonly Random random = new Random();


        /// <summary>
        /// Helper method for large multiplication - this function is missing
        /// from the portable profile.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static long BigMul(int a, int b)
        {
            return ((long)a * (long)b);
        }

        /// <summary>
        /// Get a range of sequence.
        /// </summary>
        /// <param name="sequence">Original sequence.</param>
        /// <param name="start">Start position.</param>
        /// <param name="length">Length of sequence.</param>
        /// <returns>New sequence with range specified.</returns>
        public static ISequence GetSequenceRange(ISequence sequence, long start, long length)
        {
            if (sequence == null)
            {
                throw new ArgumentNullException("sequence");
            }

            if (start < 0 || start >= sequence.Count)
            {
                throw new ArgumentOutOfRangeException(
                    Resource.ParameterNameStart,
                    Resource.ParameterMustLessThanCount);
            }

            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(
                    "length",
                    Resource.ParameterMustNonNegative);
            }

            if ((sequence.Count - start) < length)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            char[] newSeqData = new char[length];

            long j = 0;
            for (long i = start; i < start + length; i++, j++)
            {
                newSeqData[j] = sequence[i];
            }

            ISequence newSequence = sequence.GetSubSequence(start, length);
            return newSequence;
        }


        

        

        

        /// <summary>
        /// Create a useful error message when a sequence fails validation.
        /// </summary>
        /// <param name="alphabet"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static ArgumentOutOfRangeException GenerateAlphabetCheckFailureException(IAlphabet alphabet, char[] data)
        {
            return new ArgumentOutOfRangeException("Sequence contains an illegal character not allowed in alphabet: "
                + alphabet.Name + ".  Sequence was:\r\n" + data.ToString());
        }


        

        

        /// <summary>
        /// Copies source array to destination array.
        /// </summary>
        /// <param name="sourceArray">Source array</param>
        /// <param name="destinationArray">Destination array</param>
        /// <param name="length">No of elements to copy</param>
        public static void Copy(Array sourceArray, Array destinationArray, long length)
        {
            Copy(sourceArray, 0, destinationArray, 0, length);
        }


        /// <summary>
        ///  Copies source array to destination array.
        /// </summary>
        /// <param name="sourceArray">Source array</param>
        /// <param name="sourceIndex">Source stating index.</param>
        /// <param name="destinationArray">Destination array</param>
        /// <param name="destinationIndex">Destination stating index.</param>
        /// <param name="length">No of elements to copy</param>
        public static void Copy(Array sourceArray, long sourceIndex, Array destinationArray, long destinationIndex, long length)
        {
            Array.Copy(sourceArray, sourceIndex, destinationArray, destinationIndex, length);
        }
    }
}
