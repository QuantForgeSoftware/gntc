using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Gntc.Core;

/// <summary>Strict, lossless converter for this project's TEI profile; not an arbitrary-TEI importer.</summary>
public static class TeiFormat
{
    private static readonly XNamespace N = "http://www.tei-c.org/ns/1.0";
    private static readonly XNamespace Xml = XNamespace.Xml;

    public static string Write(Corpus corpus)
    {
        var output = new StringBuilder();
        using (var writer = XmlWriter.Create(output, new XmlWriterSettings
        {
            Indent = true, OmitXmlDeclaration = true, NewLineChars = "\n", NewLineHandling = NewLineHandling.Entitize
        })) Build(corpus).Save(writer);
        return output + "\n";
    }

    public static Corpus Read(string text)
    {
        using var reader = XmlReader.Create(new StringReader(text), new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreWhitespace = true,
            MaxCharactersInDocument = 50_000_000
        });
        var document = XDocument.Load(reader);
        if (document.Root?.Name != N + "TEI") throw new FormatException("Expected a TEI namespace document.");
        var standOff = document.Root.Element(N + "standOff") ?? throw new FormatException("Missing standOff.");
        var meta = standOff.Elements(N + "fs").SingleOrDefault(e => (string?)e.Attribute(Xml + "id") == "gntc_metadata")
            ?? throw new FormatException("Only GNTC-profile TEI is supported; missing gntc_metadata.");
        var corpus = new Corpus();
        foreach (var (name, value) in Features(meta)) corpus.Metadata.Add(name, value);
        foreach (var ab in document.Root.Element(N + "text")?.Element(N + "body")?.Elements(N + "ab") ?? [])
        {
            foreach (var segment in ab.Elements(N + "seg"))
            {
                var word = segment.Element(N + "w") ?? throw new FormatException("Token segment is missing its word.");
                var id = int.Parse(Required(segment, "n"), CultureInfo.InvariantCulture);
                var tokenFs = standOff.Elements(N + "fs").Single(e => (string?)e.Attribute(Xml + "id") == $"gntc_t{id}");
                var features = Features(tokenFs);
                var hasMorphology = word.Attribute("lemma") is not null;
                corpus.Words.Add(new Word
                {
                    Id = id, Verse = Required(ab, "n"), Position = int.Parse(Required(word, "n"), CultureInfo.InvariantCulture),
                    SourceBcv = features["sourceBcv"], Surface = segment.Value,
                    Lemma = (string?)word.Attribute("lemma"), Pos = (string?)word.Attribute("pos"), Parsing = (string?)word.Attribute("msd"),
                    Form = hasMorphology ? word.Value : null, Normalized = hasMorphology ? features["normalized"] : null
                });
            }
        }
        foreach (var fs in standOff.Elements(N + "fs").Where(e => (string?)e.Attribute("type") == "gntc-annotation"))
        {
            var features = Features(fs);
            var kind = features["record"];
            if (!Records.Fields.TryGetValue(kind, out var names)) throw new FormatException("Unknown annotation record in TEI.");
            var a = new Annotation { Kind = kind, Id = features["id"], Args = names.Select(name => features[name]).ToList() };
            foreach (var (name, value) in features.Where(x => x.Key.StartsWith("attr.", StringComparison.Ordinal)))
                a.Attributes.Add(name[5..], value);
            corpus.Annotations.Add(a);
        }
        Validator.RequireValid(corpus);
        // Reject extensions and disagreements between XML topology and the feature structures.
        // This also prevents silently dropping unrecognized TEI content on round-trip.
        var rebuilt = Build(corpus).Root!;
        if (!XNode.DeepEquals(document.Root, rebuilt))
            throw new FormatException("TEI differs from the supported GNTC profile. Unsupported content or inconsistent structural/feature annotations would be lost. " + Difference(document.Root!, rebuilt));
        return corpus;
    }

    public static XDocument Build(Corpus corpus)
    {
        Validator.RequireValid(corpus);
        corpus.Canonicalize();
        var standOff = new XElement(N + "standOff", new XAttribute("type", "gntc"));
        standOff.Add(Fs("gntc_metadata", "gntc-metadata", corpus.Metadata));
        standOff.Add(new XElement(N + "interp", Id("gntc_unknown"), new XAttribute("type", "unresolved"), "Unknown annotation value; not an empty text span."));
        var body = new XElement(N + "body");
        XElement? current = null;
        Word? previous = null;
        foreach (var word in corpus.Words)
        {
            if (previous is null || previous.Verse != word.Verse || previous.Id + 1 != word.Id)
            {
                current = new XElement(N + "ab", Id($"gntc_v{word.Id}"), new XAttribute("n", word.Verse));
                body.Add(current);
            }
            previous = word;
            var wordFs = new SortedDictionary<string, string>(StringComparer.Ordinal) { ["sourceBcv"] = word.SourceBcv };
            if (word.HasMorphology) wordFs["normalized"] = word.Normalized!;
            standOff.Add(Fs($"gntc_t{word.Id}", "gntc-token", wordFs));
            var w = new XElement(N + "w", Id($"w{word.Id}"), new XAttribute("n", word.Position), new XAttribute("ana", $"#gntc_t{word.Id}"));
            var segment = new XElement(N + "seg", new XAttribute("type", "token"), new XAttribute("n", word.Id));
            if (word.HasMorphology)
            {
                w.Add(new XAttribute("lemma", word.Lemma!), new XAttribute("pos", word.Pos!), new XAttribute("msd", word.Parsing!));
                var offset = word.Surface.IndexOf(word.Form!, StringComparison.Ordinal);
                if (offset < 0) throw new FormatException($"Token {word.Id}: source form is not a substring of surface; profile cannot split punctuation safely.");
                if (offset > 0) segment.Add(new XElement(N + "pc", word.Surface[..offset]));
                w.Value = word.Form!;
                segment.Add(w);
                var tail = word.Surface[(offset + word.Form!.Length)..];
                if (tail.Length > 0) segment.Add(new XElement(N + "pc", tail));
            }
            else { w.Value = word.Surface; segment.Add(w); }
            current!.Add(segment);
        }
        var overt = new XElement(N + "linkGrp", new XAttribute("type", "connective"), new XAttribute("targFunc", "marker context scope"));
        var implicitLinks = new XElement(N + "linkGrp", new XAttribute("type", "implicit"), new XAttribute("targFunc", "context scope"));
        foreach (var a in corpus.Annotations)
        {
            var values = new SortedDictionary<string, string>(StringComparer.Ordinal) { ["record"] = a.Kind, ["id"] = a.Id };
            for (var i = 0; i < a.Args.Count; i++) values[Records.Fields[a.Kind][i]] = a.Args[i];
            foreach (var (key, value) in a.Attributes) values["attr." + key] = value;
            standOff.Add(Fs($"gntc_a_{a.Id}", "gntc-annotation", values));
            var ana = new XAttribute("ana", $"#gntc_a_{a.Id}");
            switch (a.Kind)
            {
                case "U":
                case "Q":
                    var span = Span(a.Id, a.Args[0], a.Kind == "U" ? a.Args[1] : "candidate");
                    span.Add(ana); standOff.Add(span);
                    break;
                case "K":
                    standOff.Add(new XElement(N + "interp", Id(a.Id), new XAttribute("type", a.Args[0]), ana, a.Args[1]));
                    break;
                case "I":
                    standOff.Add(new XElement(N + "interp", Id(a.Id), new XAttribute("type", "effect"), new XAttribute("corresp", "#" + a.Args[0]), ana, a.Args[1]));
                    break;
                case "A":
                    standOff.Add(new XElement(N + "alt", Id(a.Id), new XAttribute("mode", a.Args[0] == "exclusive" ? "excl" : "incl"),
                        new XAttribute("target", string.Join(' ', a.Args[1].Split(',').Select(id => "#" + id))), ana));
                    break;
                case "C":
                    var targets = new List<string>();
                    if (a.Args[0] != "-")
                    {
                        if (a.Args[0] == "?") targets.Add("#gntc_unknown");
                        else
                        {
                            var marker = Extent.Parse(a.Args[0]);
                            if (marker.Count == 1) targets.Add($"#w{marker[0]}");
                            else
                            {
                                var markerId = $"gntc_m_{a.Id}";
                                standOff.Add(Span(markerId, a.Args[0], "marker"));
                                targets.Add("#" + markerId);
                            }
                        }
                    }
                    targets.Add(Pointer(a.Args[1])); targets.Add(Pointer(a.Args[2]));
                    var link = new XElement(N + "link", Id(a.Id), new XAttribute("target", string.Join(' ', targets)), ana);
                    (a.Args[0] == "-" ? implicitLinks : overt).Add(link);
                    break;
            }
        }
        if (overt.HasElements) standOff.Add(overt);
        if (implicitLinks.HasElements) standOff.Add(implicitLinks);
        var title = corpus.Metadata.GetValueOrDefault("title") ?? "GNTC annotated Greek text";
        var source = corpus.Metadata.GetValueOrDefault("sourceRepository", "unspecified") + " @ " + corpus.Metadata.GetValueOrDefault("sourceRevision", "unspecified");
        var header = new XElement(N + "teiHeader",
            new XElement(N + "fileDesc",
                new XElement(N + "titleStmt", new XElement(N + "title", title)),
                new XElement(N + "publicationStmt", new XElement(N + "p", "GNTC/1 research edition. Text, morphology, annotations, and software have separate licenses; see repository NOTICE.md and source metadata.")),
                new XElement(N + "sourceDesc", new XElement(N + "p", source))),
            new XElement(N + "encodingDesc", new XElement(N + "projectDesc", new XElement(N + "p",
                "GNTC/1 profile. Linguistic analyses retain their explicit review status. Word IDs address the pinned source. Feature structures and XML topology must agree."))));
        return new XDocument(new XDeclaration("1.0", "utf-8", null),
            new XElement(N + "TEI", new XAttribute("xmlns", N.NamespaceName), header, new XElement(N + "text", new XAttribute(Xml + "lang", "grc"), body), standOff));
    }

    private static XElement Span(string id, string extent, string type)
    {
        var words = Extent.Parse(extent);
        return words[^1] - words[0] + 1 == words.Count
            ? new XElement(N + "span", Id(id), new XAttribute("type", type), new XAttribute("from", $"#w{words[0]}"), new XAttribute("to", $"#w{words[^1]}"))
            : new XElement(N + "join", Id(id), new XAttribute("type", type), new XAttribute("target", string.Join(' ', words.Select(w => $"#w{w}"))));
    }

    private static XElement Fs(string id, string type, IEnumerable<KeyValuePair<string, string>> values) =>
        new(N + "fs", Id(id), new XAttribute("type", type), values.Select(x => new XElement(N + "f", new XAttribute("name", x.Key),
            new XElement(N + "string", x.Value.Length > 0 && string.IsNullOrWhiteSpace(x.Value) ? new XAttribute(Xml + "space", "preserve") : null,
                x.Value.Length == 0 ? null : x.Value))));

    private static SortedDictionary<string, string> Features(XElement fs)
    {
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var feature in fs.Elements(N + "f"))
        {
            var value = feature.Element(N + "string") ?? throw new FormatException("Profile features require string values.");
            if (!values.TryAdd(Required(feature, "name"), value.Value)) throw new FormatException("Duplicate TEI feature.");
        }
        return values;
    }

    private static XAttribute Id(string id) => new(Xml + "id", id);

    private static string Difference(XElement actual, XElement expected)
    {
        var location = (string?)expected.Attribute(Xml + "id") ?? expected.Name.LocalName;
        if (actual.Name != expected.Name) return $"At {location}: unexpected element {actual.Name}.";
        if (!actual.Attributes().OrderBy(a => a.Name.ToString()).Select(a => (a.Name, a.Value)).SequenceEqual(
            expected.Attributes().OrderBy(a => a.Name.ToString()).Select(a => (a.Name, a.Value)))) return $"At {location}: attributes differ.";
        var aNodes = actual.Nodes().ToArray(); var eNodes = expected.Nodes().ToArray();
        if (aNodes.Length != eNodes.Length) return $"At {location}: child counts differ ({aNodes.Length}/{eNodes.Length}).";
        for (var i = 0; i < aNodes.Length; i++)
        {
            if (XNode.DeepEquals(aNodes[i], eNodes[i])) continue;
            if (aNodes[i] is XElement a && eNodes[i] is XElement e) return Difference(a, e);
            return $"At {location}: content differs.";
        }
        return $"At {location}: XML representation differs.";
    }
    private static string Pointer(string id) => id == "?" ? "#gntc_unknown" : "#" + id;
    private static string Required(XElement element, string attribute) => (string?)element.Attribute(attribute) ?? throw new FormatException($"Missing {attribute}.");
}
