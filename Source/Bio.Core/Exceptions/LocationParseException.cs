using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.Exceptions;
internal class LocationParseException : Exception
{
    public LocationParseException(string message) : base(message)
    {
    }
}
