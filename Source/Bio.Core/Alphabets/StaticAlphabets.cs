using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.Alphabets;
/// <summary>
/// The currently supported and built-in alphabets for sequence items.
/// </summary>
public static class StaticAlphabets
{
    /// <summary>
    /// The DNA alphabet.
    /// </summary>
    public static readonly DnaAlphabet DNA = DnaAlphabet.Instance;
    public static readonly ProteinAlphabet Protein = ProteinAlphabet.Instance;
    public static readonly RnaAlphabet Rna = RnaAlphabet.Instance;

}
