using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bio.Core.Sequences.Location;
public abstract class Position : IComparable<Position>
{
    public long MonomerPosition { get; protected set; }

    public static Position FromString(string text, long offset = 0)
    {
        if (offset != 0 && offset != -1)
        {
            throw new ArgumentException(
                "To convert one-based indices to zero-based indices, offset must be either 0 (for end positions) or -1 (for start positions)."
            );
        }
        if (text == "?")
        {
            return new UnknownPosition();
        }
        if (text.StartsWith("?"))
        {
            return new UncertainPosition(int.Parse(text.Substring(1)) + offset);
        }
        if (text.StartsWith("<"))
        {
            return new BeforePosition(int.Parse(text.Substring(1)) + offset);
        }
        if (text.StartsWith(">"))
        {
            return new AfterPosition(int.Parse(text.Substring(1)) + offset);
        }
        var m = Location._re_within_position.Match(text);
        if (m.Success)
        {
            var groups = m.Groups;
            long s = long.Parse(groups[1].Value) + offset;
            long e = long.Parse(groups[2].Value) + offset;
            long defaultPosition = offset == -1 ? s : e;
            return new WithinPosition(defaultPosition, s, e);
        }
        m = Location._re_oneof_position.Match(text);
        if (m.Success)
        {
            var positions = m.Groups[1].Value;
            var parts = positions.Split(',').Select(pos => new ExactPosition(int.Parse(pos) + offset)).ToList();
            long defaultPosition = offset == -1 ? parts.Min(pos => pos.MonomerPosition) : parts.Max(pos => pos.MonomerPosition);
            return new OneOfPosition(defaultPosition, parts);
        }
        return new ExactPosition(int.Parse(text) + offset);
    }

    public int CompareTo(Position? other)
    {
        if (other is null)
        {
            return 1;
        }
        if (this.MonomerPosition < other.MonomerPosition)
            return -1;
        else if (this.MonomerPosition == other.MonomerPosition)
            return 0;
        else
            return 1;
    }
}
