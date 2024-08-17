using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.Sequences.Location;
public class CompoundLocation : Location
{
    private string operation;
    private IEnumerable<SimpleLocation> parts;
    public CompoundLocation(string operation, IEnumerable<SimpleLocation> parts)
    {
        this.operation = operation;
        this.parts = new List<SimpleLocation>(parts);

        foreach (var loc in this.parts)
        {
            if (!(loc is SimpleLocation))
            {
                throw new ArgumentException(
                    $"CompoundLocation should be given a list of SimpleLocation objects, not {loc.GetType()}"
                );
            }
        }

        if (this.parts.Count() < 2)
        {
            throw new ArgumentException(
                $"CompoundLocation should have at least 2 parts, not {string.Join(", ", parts)}"
            );
        }
    }
}
