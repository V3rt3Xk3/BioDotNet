using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.IO;

/// <summary>
/// Hold information about a Feature in the Feature Table of 
/// GenBank record.
/// </summary>
public class Feature
{
    /// <summary>
    /// The key name of the feature (ie. source)
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The string specifying the location of the feature.
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// A list of Qualifier objects in the feature.
    /// </summary>
    public List<Qualifier>? Qualifiers { get; set; }

    /// <summary>
    /// Initialize the class.
    /// </summary>
    /// <param name="key">The key of the feature.</param>
    /// <param name="location">The location of the feature.</param>
    public Feature(string key = "", string location = "", List<Qualifier>? qualifiers = null)
    {
        Key = key;
        Location = location;
        Qualifiers = qualifiers;
    }

    /// <summary>
    /// Representation of the object for debugging or logging.
    /// </summary>
    /// <returns>A string representation of the Feature.</returns>
    public override string ToString()
    {
        var output = string.Format(GenbankRecord.INTERNAL_FEATURE_FORMAT, Key);
        output += GenbankRecord.WrappedGenbank(Location, GenbankRecord.GB_FEATURE_INDENT, true, ',');
        
        // If we don't have qualifiers.
        if (Qualifiers is null) return output;
        
        foreach (var qualifier in Qualifiers)
        {
            output += qualifier.ToString();
        }
        return output;
    }
}