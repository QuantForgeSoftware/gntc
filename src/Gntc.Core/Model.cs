using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gntc.Core;

public sealed class Corpus
{
    public string Format { get; set; } = "GNTC/1";
    public SortedDictionary<string, string> Metadata { get; set; } = new(StringComparer.Ordinal);
    public List<Word> Words { get; set; } = [];
    public List<Annotation> Annotations { get; set; } = [];

    public string? Attribute(Annotation a, string key) =>
        a.Attributes.GetValueOrDefault(key) ?? Metadata.GetValueOrDefault("default." + key);

    public void Canonicalize()
    {
        Metadata = new(Metadata, StringComparer.Ordinal);
        Words = Words.OrderBy(w => w.Id).ToList();
        Annotations = Annotations.OrderBy(a => Array.IndexOf(Records.Order, a.Kind))
            .ThenBy(a => a.Id, StringComparer.Ordinal).ToList();
        foreach (var a in Annotations) a.Attributes = new(a.Attributes, StringComparer.Ordinal);
    }
}

public sealed class Word
{
    public int Id { get; set; }
    public string Verse { get; set; } = "";
    public int Position { get; set; }
    public string SourceBcv { get; set; } = "";
    public string Surface { get; set; } = "";
    public string? Lemma { get; set; }
    public string? Pos { get; set; }
    public string? Parsing { get; set; }
    public string? Form { get; set; }
    public string? Normalized { get; set; }

    [JsonIgnore] public string Reference => $"{Verse}.w{Position}";
    [JsonIgnore] public bool HasMorphology => Lemma is not null && Pos is not null &&
        Parsing is not null && Form is not null && Normalized is not null;
}

public sealed class Annotation
{
    public string Kind { get; set; } = "";
    public string Id { get; set; } = "";
    public List<string> Args { get; set; } = [];
    public SortedDictionary<string, string> Attributes { get; set; } = new(StringComparer.Ordinal);
}

public static class Records
{
    public static readonly string[] Order = ["U", "K", "Q", "C", "I", "A"];
    public static readonly IReadOnlyDictionary<string, string[]> Fields = new Dictionary<string, string[]>
    {
        ["U"] = ["extent", "unitKind"],
        ["K"] = ["contextKind", "description"],
        ["Q"] = ["marker", "evidence"],
        ["C"] = ["marker", "context", "scope", "operation", "relation", "domain"],
        ["I"] = ["connection", "effect"],
        ["A"] = ["mode", "members"]
    };
}

public static class JsonFormat
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public static string Write(Corpus corpus)
    {
        corpus.Canonicalize();
        return JsonSerializer.Serialize(corpus, Options) + "\n";
    }

    public static Corpus Read(string text)
    {
        // System.Text.Json otherwise accepts duplicate object members (last value wins).
        using var tree = JsonDocument.Parse(text);
        RejectDuplicates(tree.RootElement);
        if (tree.RootElement.ValueKind != JsonValueKind.Object ||
            new[] { "format", "metadata", "words", "annotations" }.Any(name => !tree.RootElement.TryGetProperty(name, out _)))
            throw new FormatException("JSON requires format, metadata, words, and annotations.");
        var corpus = JsonSerializer.Deserialize<Corpus>(text, Options) ?? throw new FormatException("Empty JSON document.");
        if (corpus.Format != "GNTC/1") throw new FormatException("Unsupported format version.");
        if (corpus.Metadata is null || corpus.Words is null || corpus.Annotations is null ||
            corpus.Metadata.Values.Any(v => v is null) ||
            corpus.Words.Any(w => w is null || w.Verse is null || w.SourceBcv is null || w.Surface is null) ||
            corpus.Annotations.Any(a => a is null || a.Kind is null || a.Id is null || a.Args is null || a.Attributes is null || a.Attributes.Values.Any(v => v is null)))
            throw new FormatException("Required collections cannot be null.");
        return corpus;
    }

    private static void RejectDuplicates(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject())
            {
                if (!names.Add(property.Name)) throw new FormatException($"Duplicate JSON member: {property.Name}");
                RejectDuplicates(property.Value);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
            foreach (var child in element.EnumerateArray()) RejectDuplicates(child);
    }
}
