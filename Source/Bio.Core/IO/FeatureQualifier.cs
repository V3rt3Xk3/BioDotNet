using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bio.Core.IO.Genbank.Consumers;

namespace Bio.Core.IO;
public class Qualifier
{
    // Attributes
    public string Key { get; set; } // The key name of the qualifier (e.g., /organism=)
    public string Value { get; set; } // The value of the qualifier ("Dictyostelium discoideum")

    // Constructor
    public Qualifier(string key = "", string value = "")
    {
        Key = key;
        Value = value;
    }

    // Representation of the object for debugging or logging
    public override string ToString()
    {
        // Assuming Record.GB_FEATURE_INDENT and a method _wrapped_genbank similar to the Python version exist
        string output = new string(' ', GenbankRecord.GB_FEATURE_INDENT);
        bool spaceWrap = true;
        foreach (var noSpaceKey in BaseGenBankConsumer.RemoveSpaceKeys)
        {
            if (Key.Contains(noSpaceKey))
            {
                spaceWrap = false;
                break;
            }
        }
        // Assuming _wrapped_genbank is a method that wraps the GenBank format string
        return output + GenbankRecord.WrappedGenbank(Key + Value, GenbankRecord.GB_FEATURE_INDENT, spaceWrap);
    }
}