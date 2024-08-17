using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.IO;
public class Reference
{
    /// <summary>
    /// The number of the reference in the listing of references.
    /// </summary>
    public string Number { get; set; } = "";

    /// <summary>
    /// The bases in the sequence the reference refers to.
    /// </summary>
    public string Bases { get; set; } = "";

    /// <summary>
    /// String with all of the authors.
    /// </summary>
    public string Authors { get; set; } = "";

    /// <summary>
    /// Consortium the authors belong to.
    /// </summary>
    public string Consrtm { get; set; } = "";

    /// <summary>
    /// The title of the reference.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Information about the journal where the reference appeared.
    /// </summary>
    public string Journal { get; set; } = "";

    /// <summary>
    /// The medline id for the reference.
    /// </summary>
    public string MedlineId { get; set; } = "";

    /// <summary>
    /// The pubmed_id for the reference.
    /// </summary>
    public string PubmedId { get; set; } = "";

    /// <summary>
    /// Free-form remarks about the reference.
    /// </summary>
    public string Remark { get; set; } = "";

    /// <summary>
    /// The location of the reference in the sequence.
    /// </summary>
    public List<string> Location { get; set; } = new List<string>();
}
