using Bio.Core.Sequences;
using Bio.Core.Sequences.Location;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Bio.Core.IO.Genbank.Consumers;

/// <summary>
/// Create a SeqRecord object with Features to return (PRIVATE).
/// Attributes:
/// <list type="bullet">
///    <item>
///        <term>useFuzziness</term>
///        <description>specify whether or not to parse with fuzziness in feature locations.</description>
///    </item>
///    <item>
///        <term>featureCleaner</term>
///        <description>a class that will be used to provide specialized cleaning-up of feature values.</description>
///    </item>
///</list>
/// </summary>
public class FeatureConsumer : BaseGenBankConsumer
{ 
    private bool useFuzziness;
    private FeatureValueCleaner? featureCleaner; // Assuming the type of featureCleaner, adjust as necessary

    private string sequenceType;
    private List<string> sequenceData;
    private object curFeature; // Adjust the type as necessary
    private long? expectedSize;

    public FeatureConsumer(bool useFuzziness, FeatureValueCleaner? featureCleaner = null) : base()
    {
        // Assuming SeqRecord constructor in C# takes similar parameters
        this.Data = new Sequence();

        this.useFuzziness = useFuzziness;
        this.featureCleaner = featureCleaner;

        this.sequenceType = "";
        this.sequenceData = new List<string>();
        this.curReference = null;
        this.curFeature = null;
        this.expectedSize = null;
    }

    /// <summary>
    /// Set the locus name is set as the name of the Sequence.
    /// </summary>
    /// <param name="locusName"></param>
    public override void Locus(string locusName)
    {
        this.Data.Name = locusName;
    }

    /// <summary>
    /// Record the sequence length.
    /// </summary>
    /// <param name="length"></param>
    public override void Size(string length)
    {
        this.expectedSize = long.Parse(length);
    }

    /// <summary>
    /// Record the sequence type (SEMI-OBSOLETE).
    /// 
    /// This reflects the fact that the topology (linear/circular) and
    /// molecule type (e.g. DNA vs RNA) were a single field in early
    /// files. Current GenBank/EMBL files have two fields.
    /// </summary>
    /// <param name="type">The sequence type.</param>
    public override void ResidueType(string type)
    {
        sequenceType = type.Trim();
    }


    /// <summary>
    /// Validate and record sequence topology.
    /// The topology argument should be "linear" or "circular" (string).
    /// </summary>
    /// <param name="topology"></param>
    /// <exception cref="ParserFailureError"></exception>
    public override void Topology(string topology)
    {
        
        if (!string.IsNullOrEmpty(topology))
        {
            if (topology != "linear" && topology != "circular")
            {
                throw new ArgumentException($"Unexpected topology {topology} should be linear or circular");
            }
            Data.Annotations["topology"] = topology;
        }
    }

    /// <summary>
    /// Validate and record the molecule type (for round-trip etc).
    /// </summary>
    /// <param name="molType"></param>
    /// <exception cref="Exception"></exception>
    public override void MoleculeType(string molType)
    {
        // Validate and record the molecule type (for round-trip etc).
        if (!string.IsNullOrEmpty(molType))
        {
            if (molType.Contains("circular") || molType.Contains("linear"))
            {
                throw new ArgumentException($"Molecule type {molType} should not include topology");
            }

            // Writing out records will fail if we have a lower case DNA
            // or RNA string in here, so upper case it.
            // This is a bit ugly, but we don't want to upper case e.g.
            // the m in mRNA, but thanks to the strip we lost the spaces
            // so we need to index from the back
            if ((molType.EndsWith("DNA", StringComparison.OrdinalIgnoreCase) || molType.EndsWith("RNA", StringComparison.OrdinalIgnoreCase))
                && !molType.EndsWith("DNA") && !molType.EndsWith("RNA"))
            {
                logger.LogWarning($"Non-upper case molecule type in LOCUS line: {molType}");
            }

            Data.Annotations["molecule_type"] = molType;
        }
    }

    public override void DataFileDivision(string division)
    {
        Data.Annotations["data_file_division"] = division;
    }

    public void Date(DateTime submitDate)
    {
        Data.Annotations["date"] = submitDate;
    }

    public override void Definition(string definition)
    {
        // Set the definition as the description of the sequence.
        if (!string.IsNullOrEmpty(Data.Description))
        {
            // Append to any existing description
            // e.g. EMBL files with two DE lines.
            Data.Description += " " + definition;
        }
        else
        {
            Data.Description = definition;
        }
    }

    /// <summary>
    /// Set the accession number as the id of the sequence.
    /// If we have multiple accession numbers, the first one passed is used.
    /// </summary>
    /// <param name="accNum"></param>
    public override void Accession(string accNum)
    {
        List<string> newAccNums = SplitAccessions(accNum);

        // Also record them ALL in the annotations
        try
        {
            // On the off chance there was more than one accession line:
            foreach (string acc in newAccNums)
            {
                // Prevent repeat entries
                if (!((List<string>)Data.Annotations["accessions"]).Contains(acc))
                {
                    ((List<string>)Data.Annotations["accessions"]).Add(acc);
                }
            }
        }
        catch (KeyNotFoundException)
        {
            Data.Annotations["accessions"] = newAccNums;
        }

        // if we haven't set the id information yet, add the first acc num
        if (string.IsNullOrEmpty(Data.ID))
        {
            if (newAccNums.Count > 0)
            {
                // Use the FIRST accession as the ID, not the first on this line!
                Data.ID = ((List<string>)Data.Annotations["accessions"])[0];
            }
        }
    }

    public override void Tls(string content)
    {
        Data.Annotations["tls"] = content.Split('-');
    }

    public override void Tsa(string content)
    {
        Data.Annotations["tsa"] = content.Split('-');
    }

    public override void Wgs(string content)
    {
        Data.Annotations["wgs"] = content.Split('-');
    }

    public override void AddWgsScafld(string content)
    {
        if (!Data.Annotations.ContainsKey("wgs_scafld"))
        {
            Data.Annotations["wgs_scafld"] = new List<string>();
        }
        ((List<string>)Data.Annotations["wgs_scafld"]).AddRange(content.Split('-'));
    }

    public override void Nid(string content)
    {
        Data.Annotations["nid"] = content;
    }

    public override void Pid(string content)
    {
        Data.Annotations["pid"] = content;
    }

    /// <summary>
    /// Want to use the versioned accession as the record.id
    /// This comes from the VERSION line in GenBank files, or the
    /// obsolete SV line in EMBL. For the new EMBL files we need
    /// both the version suffix from the ID line and the accession
    /// from the AC line.
    /// </summary>
    /// <param name="versionId"></param>
    public override void Version(string versionId)
    {
        if (versionId.Count(c => c == '.') == 1 && int.TryParse(versionId.Split('.')[1], out _))
        {
            this.Accession(versionId.Split('.')[0]);
            this.VersionSuffix(versionId.Split('.')[1]);
        }
        else if (!string.IsNullOrEmpty(versionId))
        {
            // For backwards compatibility...
            Data.ID = versionId;
        }
    }

    public override void Project(string content)
    {
        // Handle the information from the PROJECT line as a list of projects.
        // e.g.:
        // PROJECT     GenomeProject:28471
        // or:
        // PROJECT     GenomeProject:13543  GenomeProject:99999
        //
        // This is stored as dbxrefs in the SeqRecord to be consistent with the
        // projected switch of this line to DBLINK in future GenBank versions.
        // Note the NCBI plan to replace "GenomeProject:28471" with the shorter
        // "Project:28471" as part of this transition.

        content = content.Replace("GenomeProject:", "Project:");
        if (!Data.Annotations.ContainsKey("dbxrefs"))
        {
            Data.Annotations["dbxrefs"] = new List<string>();
        }
        ((List<string>)Data.Annotations["dbxrefs"]).AddRange(content.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
    }

    public override void DbLink(string content)
    {
        // Store DBLINK cross references as dbxrefs in our record object.
        // This line type is expected to replace the PROJECT line in 2009. e.g.
        //
        // During transition:
        //
        //     PROJECT     GenomeProject:28471
        //     DBLINK      Project:28471
        //                 Trace Assembly Archive:123456
        //
        // Once the project line is dropped:
        //
        //     DBLINK      Project:28471
        //                 Trace Assembly Archive:123456
        //
        // Note GenomeProject -> Project.
        //
        // We'll have to see some real examples to be sure, but based on the
        // above example we can expect one reference per line.
        //
        // Note that at some point the NCBI have included an extra space, e.g.:
        //
        //     DBLINK      Project: 28471

        // During the transition period with both PROJECT and DBLINK lines,
        // we don't want to add the same cross reference twice.
        while (content.Contains(": "))
        {
            content = content.Replace(": ", ":");
        }
        if (!Data.DbXrefs.Contains(content.Trim()))
        {
            Data.DbXrefs.Add(content.Trim());
        }
    }

    /// <summary>
    /// Set the version to overwrite the id.
    /// Since the version provides the same information as the accession
    /// number, plus some extra info, we set this as the id if we have
    /// a version.
    /// </summary>
    /// <param name="version">The version string.</param>
    /// <example>
    /// GenBank line:
    /// VERSION     U49845.1  GI:1293613
    /// or the obsolete EMBL line:
    /// SV   U49845.1
    /// Scanner calls consumer.version("U49845.1")
    /// which then calls consumer.version_suffix(1)
    /// 
    /// EMBL new line:
    /// ID   X56734; SV 1; linear; mRNA; STD; PLN; 1859 BP.
    /// Scanner calls consumer.version_suffix(1)
    /// </example>
    public override void VersionSuffix(string version)
    {
        
        if (int.TryParse(version, out int versionNumber))
        {
            Data.Annotations["sequence_version"] = versionNumber;
        }
        else
        {
            throw new ArgumentException("Version must be a digit.", nameof(version));
        }
    }

    public override void DbSource(string content)
    {
        Data.Annotations["db_source"] = content.TrimEnd();
    }

    public override void Gi(string content)
    {
        Data.Annotations["gi"] = content;
    }

    public override void Keywords(string content)
    {
        if (Data.Annotations.ContainsKey("keywords"))
        {
            // Multi-line keywords, append to list
            // Note EMBL states "A keyword is never split between lines."
            ((List<string>)Data.Annotations["keywords"]).AddRange(SplitKeywords(content));
        }
        else
        {
            Data.Annotations["keywords"] = SplitKeywords(content);
        }
    }

    public override void Segment(string content)
    {
        Data.Annotations["segment"] = content;
    }

    public override void Source(string content)
    {
        // Note that some software (e.g. VectorNTI) may produce an empty
        // source (rather than using a dot/period as might be expected).
        string sourceInfo;
        if (string.IsNullOrEmpty(content))
        {
            sourceInfo = "";
        }
        else if (content.EndsWith("."))
        {
            sourceInfo = content.Substring(0, content.Length - 1);
        }
        else
        {
            sourceInfo = content;
        }
       Data.Annotations["source"] = sourceInfo;
    }

    public override void Organism(string content)
    {
        Data.Annotations["organism"] = content;
    }

    public override void Taxonomy(string content)
    {
        // Record (another line of) the taxonomy lineage.
        var lineage = SplitTaxonomy(content);
        if (Data.Annotations.ContainsKey("taxonomy"))
        {
            ((List<string>)Data.Annotations["taxonomy"]).AddRange(lineage);
        }
        else
        {
            Data.Annotations["taxonomy"] = lineage;
        }
    }

    public override void ReferenceNum(string content)
    {
        // Signal the beginning of a new reference object.
        // If we have a current reference that hasn't been added to
        // the list of references, add it.
        if (this.curReference != null)
        {
            if (!Data.Annotations.ContainsKey("references"))
            {
                Data.Annotations["references"] = new List<Reference>();
            }
            ((List<Reference>)Data.Annotations["references"]).Add(this.curReference);
        }
        else
        {
            Data.Annotations["references"] = new List<Reference>();
        }

        this.curReference = new Reference();
    }


    /// <summary>
    /// Attempt to determine the sequence region the reference entails.
    /// 
    /// Possible types of information we may have to deal with:
    /// 
    /// (bases 1 to 86436)
    /// (sites)
    /// (bases 1 to 105654; 110423 to 111122)
    /// 1  (residues 1 to 182)
    /// </summary>
    /// <param name="content">The content string containing reference base information.</param>
    public override void ReferenceBases(string content)
    {
        // First remove the parentheses
        if (!content.EndsWith(")"))
        {
            throw new ArgumentException("Content must end with a closing parenthesis.", nameof(content));
        }
        string refBaseInfo = content.Substring(1, content.Length - 2);

        var allLocations = new List<string>();
        // Parse if we've got 'bases' and 'to'
        if (refBaseInfo.Contains("bases") && refBaseInfo.Contains("to"))
        {
            // Get rid of the beginning 'bases'
            refBaseInfo = refBaseInfo.Substring(5);
            var locations = SplitReferenceLocations(refBaseInfo);
            allLocations.AddRange(locations);
        }
        else if (refBaseInfo.Contains("residues") && refBaseInfo.Contains("to"))
        {
            int residuesStart = refBaseInfo.IndexOf("residues");
            // Get only the information after "residues"
            refBaseInfo = refBaseInfo.Substring(residuesStart + "residues ".Length);
            var locations = SplitReferenceLocations(refBaseInfo);
            allLocations.AddRange(locations);
        }
        // Make sure if we are not finding information then we have
        // the string 'sites' or the string 'bases'
        else if (refBaseInfo == "sites" || refBaseInfo.Trim() == "bases")
        {
            // Do nothing
        }
        // Otherwise raise an error
        else
        {
            throw new ArgumentException($"Could not parse base info {refBaseInfo} in record {this.data.Id}");
        }

        this.curReference.Location = allLocations;
    }

    protected List<SimpleLocation> SplitReferenceLocations(string locationString)
    {
        /// <summary>
        /// Get reference locations out of a string of reference information (PRIVATE).
        /// 
        /// The passed string should be of the form:
        /// 
        ///     1 to 20; 20 to 100
        /// 
        /// This splits the information out and returns a list of location objects
        /// based on the reference locations.
        /// </summary>
        /// <param name="locationString">The string containing reference locations.</param>
        /// <returns>A list of SimpleLocation objects.</returns>

        // Split possibly multiple locations using the ';'
        var allBaseInfo = locationString.Split(';');

        var newLocations = new List<SimpleLocation>();
        foreach (var baseInfo in allBaseInfo)
        {
            var parts = baseInfo.Split(new[] { "to" }, StringSplitOptions.None);
            int start = int.Parse(parts[0].Trim());
            int end = int.Parse(parts[1].Trim());
            var (newStart, newEnd) = ConvertToCSharpNumbers(start, end);
            var thisLocation = new SimpleLocation(newStart, newEnd);
            newLocations.Add(thisLocation);
        }
        return newLocations;
    }
}
