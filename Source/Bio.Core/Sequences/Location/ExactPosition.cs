using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.Sequences.Location;
internal class ExactPosition : Position
{

    public ExactPosition(long position)
    {
        this.position = position;
    }

    public static ExactPosition Create(long position, int extension = 0)
    {
        // Create an ExactPosition object.
        if (extension != 0)
        {
            throw new ArgumentException($"Non-zero extension {extension} for exact position.");
        }
        return new ExactPosition(position);
    }
}
