using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bio.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace Bio.Core.Sequences.Location;
public class SimpleLocation : Location
{
    protected ILogger logger { get; init; }

    protected Position start;
    protected Position end;
    protected LocationStrand strand;
    protected string? reference;
    protected string? ref_db;

    public SimpleLocation(long start, long end, LocationStrand strand = LocationStrand.Undefined, string? reference = null, string? ref_db = null)
    {
        logger = new BioLogger("SimpleLocation Logger");
        // TODO - Check 0 <= start <= end (<= length of reference)
        this.start = new ExactPosition(start);
        this.end = new ExactPosition(end);

        if (this.start.MonomerPosition > this.end.MonomerPosition)
        {
            throw new ArgumentException(
                $"End location ({this.end}) must be greater than or equal to start location ({this.start})"
            );
        }

        this.strand = strand;
        this.reference = reference;
        this.ref_db = ref_db;
    }
    public SimpleLocation(Position start, long end, LocationStrand strand = LocationStrand.Undefined, string? reference = null, string? ref_db = null)
    {
        logger = new BioLogger("SimpleLocation Logger");
        // TODO - Check 0 <= start <= end (<= length of reference)
        this.start = start;
        this.end = new ExactPosition(end);

        if (this.start.MonomerPosition > this.end.MonomerPosition)
        {
            throw new ArgumentException(
                $"End location ({this.end}) must be greater than or equal to start location ({this.start})"
            );
        }

        this.strand = strand;
        this.reference = reference;
        this.ref_db = ref_db;
    }
    public SimpleLocation(long start, Position end, LocationStrand strand = LocationStrand.Undefined, string? reference = null, string? ref_db = null)
    {
        logger = new BioLogger("SimpleLocation Logger");
        // TODO - Check 0 <= start <= end (<= length of reference)
        this.start = new ExactPosition(start);
        this.end = end;

        if (this.start.MonomerPosition > this.end.MonomerPosition)
        {
            throw new ArgumentException(
                $"End location ({this.end}) must be greater than or equal to start location ({this.start})"
            );
        }

        this.strand = strand;
        this.reference = reference;
        this.ref_db = ref_db;
    }
    public SimpleLocation(Position start, Position end, LocationStrand strand = LocationStrand.Undefined, string? reference = null, string? ref_db = null)
    {
        logger = new BioLogger("SimpleLocation Logger");
        // TODO - Check 0 <= start <= end (<= length of reference)
        this.start = start;
        this.end = end;

        if (this.start.MonomerPosition > this.end.MonomerPosition)
        {
            throw new ArgumentException(
                $"End location ({this.end}) must be greater than or equal to start location ({this.start})"
            );
        }

        this.strand = strand;
        this.reference = reference;
        this.ref_db = ref_db;
    }

    public static CompoundLocation operator +(SimpleLocation self, SimpleLocation other)
    {
        return new CompoundLocation(new List<SimpleLocation> { self, other });
    }
    public static SimpleLocation operator +(SimpleLocation self, long other)
    {
        return self.Shift(other);
    }

    public SimpleLocation Shift(long offset)
    {
        // Return a copy of the SimpleLocation shifted by an offset (PRIVATE).
        // Returns self when location is relative to an external reference.

        // TODO - What if offset is a fuzzy position?
        if (reference != null || ref_db != null)
        {
            return this;
        }
        return new SimpleLocation(
            start: start.MonomerPosition + offset,
            end: end.MonomerPosition + offset,
            strand: strand
        );
    }

    public Location FromString(string text, long? length = null, bool circular = false)
    {
        LocationStrand strand;
        if (text.StartsWith("complement("))
        {
            text = text.Substring(11, text.Length - 12);
            strand = LocationStrand.Reverse;
        }
        else
        {
            strand = LocationStrand.Undefined;
        }

        // Try simple cases first for speed
        try
        {
            var parts = text.Split("..");
            long s = long.Parse(parts[0]) - 1;
            long e = long.Parse(parts[1]);

            if (0 <= s && s < e)
            {
                return new SimpleLocation(s, e, strand);
            }
        }
        catch (FormatException) { }


        string refText = string.Empty;
        try
        {
            var parts = text.Split(':');
            refText = parts[0];
            text = parts[1];

            if (2 < parts.Length)
                throw new IndexOutOfRangeException();
        }
        catch (IndexOutOfRangeException)
        {
            refText = string.Empty;
        }

        var match = Location._re_location_category.Match(text);
        if (!match.Success)
        {
            throw new LocationParseException($"Could not parse feature location '{text}'");
        }

        string? key = null;
        string? value = null;
        foreach (var groupName in Location._re_location_category.GetGroupNames())
        {
            value = match.Groups[groupName].Value;
            if (!string.IsNullOrEmpty(value))
            {
                key = groupName;
                break;
            }
        }

        if (value != text)
        {
            throw new Exception("Assertion failed: value == text");
        }


        Position startPostion;
        Position endPosition;
        if (key == "bond")
        {
            // e.g. bond(196)
            logger.LogWarning("Dropping bond qualifier in feature location");
            text = text.Substring(5, text.Length - 6);
            startPostion = Position.FromString(text, -1);
            endPosition = Position.FromString(text);
        }
        else if (key == "solo")
        {
            // e.g. "123"
            startPostion = Position.FromString(text, -1);
            endPosition = Position.FromString(text);
        }
        else if (new[] { "pair", "within", "oneof" }.Contains(key))
        {
            var parts = text.Split(new[] { ".." }, StringSplitOptions.None);
            string s = parts[0];
            string e = parts[1];

            // Attempt to fix features that span the origin
            startPostion = Position.FromString(s, -1);
            endPosition = Position.FromString(e);

            if (startPostion.CompareTo(endPosition) < 0)
            {
                // There is likely a problem with origin wrapping.
                // Create a CompoundLocation of the wrapped feature,
                // consisting of two SimpleLocation objects to extend to
                // the list of feature locations.
                if (!circular)
                {
                    throw new LocationParseException(
                        $"It appears that '{text}' is a feature that spans the origin, but the sequence topology is undefined."
                    );
                }

                logger.LogWarning(
                    $"Attempting to fix invalid location {text} as " +
                    "it looks like incorrect origin wrapping. " +
                    "Please fix input file, this could have " +
                    "unintended behavior."
                );

                // TODO: this assumption might not be correct in bijection
                long lengthEvaluated = length ?? 0;

                SimpleLocation f1 = new SimpleLocation(startPostion, lengthEvaluated, strand);
                SimpleLocation f2 = new SimpleLocation(0, endPosition, strand);

                if (strand == LocationStrand.Reverse)
                {
                    // For complementary features spanning the origin
                    return f2 + f1;
                }
                else
                {
                    return f1 + f2;
                }
            }
        }
        else if (key == "between")
        {
            // A between location like "67^68" (one based counting) is a
            // special case (note it has zero length). In C# slice
            // notation this is 67:67, a zero length slice.  See Bug 2622
            // Further more, on a circular genome of length N you can have
            // a location N^1 meaning the junction at the origin. See Bug 3098.
            // NOTE - We can imagine between locations like "2^4", but this
            // is just "3".  Similarly, "2^5" is just "3..4"
            var parts = text.Split('^');
            long s = long.Parse(parts[0]);
            long e = long.Parse(parts[1]);
            if (s + 1 == e || (s == length && e == 1))
            {
                startPostion = new ExactPosition(s);
                endPosition = startPostion;
            }
            else
            {
                throw new LocationParseException($"Invalid feature location '{text}'");
            }
        }
        else 
            throw new NotImplementedException($"Feature location '{text}' parsing not implemented.");

        if (startPostion.MonomerPosition < 0)
            throw new LocationParseException($"Negative starting position in feature location '{text}'.");

        return new SimpleLocation(startPostion, endPosition, strand, refText);
    }
}