using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.Sequences;
public interface INucleicAcidSequence : ISequence
{
    /// <summary>
    /// Return a sequence representing the complement of this sequence.
    /// </summary>
    INucleicAcidSequence GetComplementedSequence();

    /// <summary>
    /// Return a sequence representing the reverse complement of this sequence.
    /// </summary>
    INucleicAcidSequence GetReverseComplementedSequence();
}
