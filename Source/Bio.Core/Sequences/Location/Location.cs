using Bio.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Bio.Core.Sequences.Location;

public enum LocationStrand
{
    Forward = 1,
    Reverse = -1,
    Undefined = 0
}

/// <summary>
/// Abstract base class representing location.
/// </summary>
public class Location
{
    public static string _reference = @"(?:[a-zA-Z][a-zA-Z0-9_\.\|]*[a-zA-Z0-9]?\:)";
    public static string _oneof_position = @"one\-of\(\d+[,\d+]+\)";
    public static string _oneof_location = $@"[<>]?(?:\d+|{_oneof_position})\.\.[<>]?(?:\d+|{_oneof_position})";
    public static string _any_location = $@"({_reference}?{_oneof_location}|complement\({_oneof_location}\)|[^,]+|complement\([^,]+\))";
    public static Func<string, string[]> _split = (input) => new Regex(_any_location).Split(input);

    public static string _pair_location = @"[<>]?-?\d+\.\.[<>]?-?\d+";
    public static string _between_location = @"\d+\^\d+";
    public static string _within_position = @"\(\d+\.\d+\)";
    public static string _within_location = $@"([<>]?\d+|{_within_position})\.\.([<>]?\d+|{_within_position})";
    public static string _within_position2 = @"\((\d+)\.(\d+)\)";
    public static Regex _re_within_position = new Regex(_within_position);

    public static string _oneof_location2 = $@"([<>]?\d+|{_oneof_position})\.\.([<>]?\d+|{_oneof_position})";
    public static string _oneof_position2 = @"one\-of\((\d+[,\d+]+)\)";
    public static Regex _re_oneof_position = new Regex(_oneof_position2);

    public static string _solo_location = @"[<>]?\d+";
    public static string _solo_bond = $@"bond\({_solo_location})";
    public static Regex _re_location_category = new Regex(string.Format(@"^(?<{0}>{1})|(?<{2}>{3})|(?<{4}>{5})|(?<{6}>{7})|(?<{8}>{9})|(?<{10}>{11})$",
                                "pair", _pair_location,
                                "between", _between_location,
                                "within", _within_location,
                                "oneof", _oneof_location,
                                "bond", _solo_bond,
                                "solo", _solo_location
                            ));



    public static Location FromString(string text, long length = 0, bool circular = false, bool stranded = true)
    {
        int? strand;
        if (text.StartsWith("complement("))
        {
            if (text[^1] != ')')
            {
                throw new ArgumentException($"closing bracket missing in '{text}'");
            }
            text = text.Substring(11, text.Length - 12);
            strand = -1;
        }
        else if (stranded)
        {
            strand = 1;
        }
        else
        {
            strand = null;
        }

        List<string> parts;
        string operation = string.Empty;
        // Determine if we have a simple location or a compound location
        if (text.StartsWith("join("))
        {
            operation = "join";
            parts = _split(text.Substring(5, text.Length - 6)).Where((_, index) => index % 2 != 0).ToList();
        }
        else if (text.StartsWith("order("))
        {
            operation = "order";
            parts = _split(text.Substring(6, text.Length - 7)).Where((_, index) => index % 2 != 0).ToList();
        }
        else if (text.StartsWith("bond("))
        {
            operation = "bond";
            parts = _split(text.Substring(5, text.Length - 6)).Where((_, index) => index % 2 != 0).ToList();
        }
        else
        {
            var loc = SimpleLocation.FromString(text, length, circular);
            loc.Strand = strand;
            if (strand == -1)
            {
                loc.Parts.Reverse();
            }
            return loc;
        }

        var locs = new List<SimpleLocation>();
        foreach (var part in parts)
        {
            var loc = SimpleLocation.FromString(part, length, circular);
            if (loc == null)
            {
                break;
            }
            if (loc.Strand == -1)
            {
                if (strand == -1)
                {
                    throw new LocationParserError($"double complement in '{text}'?");
                }
            }
            else
            {
                loc.Strand = strand;
            }
            locs.AddRange(loc.Parts);
        }

        if (locs.Count == 1)
        {
            return locs[0];
        }

        if (strand == -1)
        {
            foreach (var loc in locs)
            {
                if (loc.Strand != -1)
                {
                    throw new LocationParseException("double complement in '{text}'?");
                }
            }
            locs.Reverse();
            return new CompoundLocation(locs, operation);
        }



        // Not recognized
        if (text.Contains("order") && text.Contains("join"))
        {
            throw new LocationParseException($"failed to parse feature location '{text}' containing a combination of 'join' and 'order' (nested operators) are illegal");
        }

        // See issue #937. Note that NCBI has already fixed this record.
        if (text.Contains(",)"))
        {
            Console.WriteLine("Dropping trailing comma in malformed feature location");
            text = text.Replace(",)", ")");
            return FromString(text);
        }

        throw new LocationParseException($"failed to parse feature location '{text}'");
    }
}
