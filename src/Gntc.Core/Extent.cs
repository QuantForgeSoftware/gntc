using System.Globalization;

namespace Gntc.Core;

public static class Extent
{
    public static List<int> Parse(string text)
    {
        var result = new List<int>();
        if (string.IsNullOrEmpty(text)) throw new FormatException("Empty extent.");
        foreach (var part in text.Split(','))
        {
            var ends = part.Split('-');
            if (ends.Length is < 1 or > 2 || !int.TryParse(ends[0], NumberStyles.None, CultureInfo.InvariantCulture, out var from) || from < 1)
                throw new FormatException($"Invalid extent: {text}");
            var to = from;
            if (ends.Length == 2 && (!int.TryParse(ends[1], NumberStyles.None, CultureInfo.InvariantCulture, out to) || to < from))
                throw new FormatException($"Invalid extent: {text}");
            if ((long)to - from + result.Count > 1_000_000) throw new FormatException("Extent exceeds the one-million-word safety limit.");
            if (result.Count > 0 && from <= result[^1]) throw new FormatException("Extent segments must be ordered and nonoverlapping.");
            for (long i = from; i <= to; i++) result.Add((int)i);
        }
        return result;
    }

    public static string Format(IEnumerable<int> ids)
    {
        var sorted = ids.Distinct().Order().ToArray();
        var parts = new List<string>();
        for (var i = 0; i < sorted.Length; i++)
        {
            var start = sorted[i];
            while (i + 1 < sorted.Length && sorted[i + 1] == sorted[i] + 1) i++;
            parts.Add(start == sorted[i] ? start.ToString(CultureInfo.InvariantCulture) : $"{start}-{sorted[i]}");
        }
        return string.Join(',', parts);
    }
}
