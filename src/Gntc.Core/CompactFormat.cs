using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Gntc.Core;

public static class CompactFormat
{
    public static string Write(Corpus corpus)
    {
        corpus.Canonicalize();
        var output = new StringBuilder("@GNTC/1\n");
        foreach (var (key, value) in corpus.Metadata) output.AppendLine($"@meta {key}={Quote(value)}");
        var runs = new List<List<Word>>();
        foreach (var w in corpus.Words)
        {
            if (runs.Count == 0 || runs[^1][^1].Verse != w.Verse || runs[^1][^1].Id + 1 != w.Id ||
                runs[^1][^1].Position + 1 != w.Position || runs[^1][^1].SourceBcv != w.SourceBcv) runs.Add([]);
            runs[^1].Add(w);
        }
        foreach (var run in runs)
        {
            var first = run[0];
            output.AppendLine($"R {first.Id} {Quote(first.Verse)} {first.Position} {Quote(first.SourceBcv)}");
            output.AppendLine($"T {first.Id} {string.Join('|', run.Select(w => Quote(w.Surface)))}");
        }
        foreach (var w in corpus.Words.Where(w => w.HasMorphology))
            output.AppendLine($"M {w.Id} {Quote(w.Lemma!)} {Quote(w.Pos!)} {Quote(w.Parsing!)} {Quote(w.Form!)} {Quote(w.Normalized!)}");
        foreach (var a in corpus.Annotations)
        {
            output.Append($"{a.Kind} {a.Id} {string.Join(' ', a.Args.Select(Quote))}");
            foreach (var (key, value) in a.Attributes) output.Append($" {key}={Quote(value)}");
            output.Append('\n');
        }
        return output.ToString().Replace("\r\n", "\n", StringComparison.Ordinal);
    }

    public static Corpus Read(string text)
    {
        var corpus = new Corpus();
        var words = new Dictionary<int, Word>();
        var mappings = new SortedDictionary<int, (string Verse, int Position, string Bcv)>();
        var morph = new Dictionary<int, string[]>();
        var header = false;
        var lineNumber = 0;
        foreach (var raw in text.TrimStart('\uFEFF').Split('\n'))
        {
            lineNumber++;
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#')) continue;
            try
            {
                if (!header)
                {
                    if (line != "@GNTC/1") throw new FormatException("First record must be @GNTC/1.");
                    header = true;
                    continue;
                }
                if (line.StartsWith("T ", StringComparison.Ordinal))
                {
                    var separator = line.IndexOf(' ', 2);
                    if (separator < 0) throw new FormatException("T requires a start ID and text.");
                    var start = Positive(line[2..separator]);
                    var cells = Split(line[(separator + 1)..], '|');
                    for (var offset = 0; offset < cells.Count; offset++)
                    {
                        var id = checked(start + offset);
                        var surface = Unquote(cells[offset]);
                        if (surface.Length == 0 || !words.TryAdd(id, new Word { Id = id, Surface = surface }))
                            throw new FormatException($"Empty or duplicate text token {id}.");
                    }
                    continue;
                }
                var fields = Split(line, ' ');
                var kind = fields[0];
                if (kind == "@meta")
                {
                    if (fields.Count != 2) throw new FormatException("@meta takes one key=value pair.");
                    AddAttribute(corpus.Metadata, fields[1]);
                }
                else if (kind == "R")
                {
                    if (fields.Count != 5) throw new FormatException("R takes start, verse, position, source BCV.");
                    if (!mappings.TryAdd(Positive(fields[1]), (Unquote(fields[2]), Positive(fields[3]), Unquote(fields[4]))))
                        throw new FormatException("Duplicate reference mapping.");
                }
                else if (kind == "M")
                {
                    if (fields.Count != 7) throw new FormatException("M takes word ID, lemma, POS, parsing, form, normalized form.");
                    if (!morph.TryAdd(Positive(fields[1]), fields.Skip(2).Select(Unquote).ToArray()))
                        throw new FormatException("Duplicate morphology record.");
                }
                else if (Records.Fields.TryGetValue(kind, out var names))
                {
                    if (fields.Count < names.Length + 2) throw new FormatException($"Insufficient fields in {kind} record.");
                    var a = new Annotation { Kind = kind, Id = fields[1], Args = fields.Skip(2).Take(names.Length).Select(Unquote).ToList() };
                    foreach (var field in fields.Skip(names.Length + 2)) AddAttribute(a.Attributes, field);
                    corpus.Annotations.Add(a);
                }
                else throw new FormatException($"Unknown record type: {kind}");
            }
            catch (Exception e) when (e is FormatException or JsonException or OverflowException)
            {
                throw new FormatException($"Line {lineNumber}: {e.Message}", e);
            }
        }
        if (!header) throw new FormatException("Missing @GNTC/1 header.");
        foreach (var (id, w) in words.OrderBy(x => x.Key))
        {
            var available = mappings.Where(x => x.Key <= id).ToArray();
            if (available.Length == 0) throw new FormatException($"Missing reference mapping for token {id}.");
            var mapping = available[^1];
            w.Verse = mapping.Value.Verse;
            w.Position = checked(mapping.Value.Position + id - mapping.Key);
            w.SourceBcv = mapping.Value.Bcv;
            if (morph.Remove(id, out var m))
            {
                w.Lemma = m[0]; w.Pos = m[1]; w.Parsing = m[2]; w.Form = m[3]; w.Normalized = m[4];
            }
            corpus.Words.Add(w);
        }
        if (morph.Count > 0) throw new FormatException("Morphology references a missing token.");
        if (mappings.Keys.Any(id => !words.ContainsKey(id))) throw new FormatException("Reference mapping starts at a missing token.");
        return corpus;
    }

    public static string Quote(string value) => value.Length > 0 && !value.Any(c => char.IsWhiteSpace(c) || c is '"' or '\\' or '|' or '=')
        ? value : JsonSerializer.Serialize(value, JsonFormat.Options);

    private static string Unquote(string value)
    {
        if (value.StartsWith('"')) return JsonSerializer.Deserialize<string>(value) ?? throw new FormatException("Null string.");
        if (value.Any(c => c is '"' or '\\' || char.IsControl(c))) throw new FormatException("Quote strings containing quotes, backslashes, or control characters.");
        return value;
    }

    private static int Positive(string text) => int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value) && value > 0
        ? value : throw new FormatException($"Expected a positive integer: {text}");

    private static void AddAttribute(IDictionary<string, string> target, string field)
    {
        var index = field.IndexOf('=');
        if (index < 1 || index == field.Length - 1) throw new FormatException("Expected key=value.");
        var key = field[..index];
        if (key.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '.' or '_' or '-'))) throw new FormatException("Invalid attribute name.");
        if (!target.TryAdd(key, Unquote(field[(index + 1)..]))) throw new FormatException($"Duplicate attribute: {key}");
    }

    private static List<string> Split(string text, char separator)
    {
        var result = new List<string>();
        var part = new StringBuilder();
        var quoted = false;
        var escaped = false;
        foreach (var c in text)
        {
            if (escaped) { part.Append(c); escaped = false; continue; }
            if (quoted && c == '\\') { part.Append(c); escaped = true; continue; }
            if (c == '"') quoted = !quoted;
            if (!quoted && (c == separator || separator == ' ' && char.IsWhiteSpace(c)))
            {
                if (part.Length > 0 || separator == '|') result.Add(part.ToString());
                part.Clear();
            }
            else part.Append(c);
        }
        if (quoted || escaped) throw new FormatException("Unterminated quoted string.");
        if (part.Length > 0 || separator == '|') result.Add(part.ToString());
        return result;
    }
}
