using System.Text.RegularExpressions;
using System.Xml;

namespace Gntc.Core;

public static class Validator
{
    public static IReadOnlyList<string> Errors(Corpus corpus, bool annotationOverlay = false)
    {
        var errors = new List<string>();
        if (corpus.Format != "GNTC/1") errors.Add("Unsupported format version.");
        var strings = corpus.Metadata.Values.Concat(corpus.Words.SelectMany(w => new[] { w.Verse, w.SourceBcv, w.Surface, w.Lemma, w.Pos, w.Parsing, w.Form, w.Normalized }).OfType<string>())
            .Concat(corpus.Annotations.SelectMany(a => a.Args.Concat(a.Attributes.Values))).OfType<string>();
        foreach (var value in strings)
        {
            try { XmlConvert.VerifyXmlChars(value); }
            catch (XmlException) { errors.Add("Text contains characters not representable in XML 1.0."); break; }
        }
        if (corpus.Metadata.GetValueOrDefault("profile") is { } profile && profile != Profile.Id)
            errors.Add($"Unsupported vocabulary profile: {profile}");
        var wordIds = new HashSet<int>();
        var references = new HashSet<string>();
        foreach (var w in corpus.Words)
        {
            if (w.Id < 1 || !wordIds.Add(w.Id)) errors.Add($"Invalid or duplicate word ID {w.Id}.");
            if (string.IsNullOrEmpty(w.Surface) || string.IsNullOrEmpty(w.Verse) || w.Position < 1 || !references.Add(w.Reference))
                errors.Add($"Invalid or duplicate word reference at {w.Id}.");
            if (!Regex.IsMatch(w.SourceBcv ?? "", "^[0-9]{6}$")) errors.Add($"Invalid source BCV at {w.Id}.");
            else
            {
                var citation = w.Verse.Split('.');
                if (citation.Length != 3 || !int.TryParse(citation[1], out var chapter) || !int.TryParse(citation[2], out var verse) ||
                    chapter != int.Parse(w.SourceBcv![2..4]) || verse != int.Parse(w.SourceBcv[4..6]) ||
                    corpus.Metadata.TryGetValue("book", out var book) && citation[0] != book)
                    errors.Add($"Reference and source BCV disagree at {w.Id}.");
            }
            var morphology = new[] { w.Lemma, w.Pos, w.Parsing, w.Form, w.Normalized };
            if (morphology.Any(x => x is not null) && morphology.Any(string.IsNullOrEmpty)) errors.Add($"Incomplete morphology at {w.Id}.");
            if (w.HasMorphology && (!Regex.IsMatch(w.Pos!, "^[A-Z-]{2}$") || w.Parsing!.Length != 8)) errors.Add($"Invalid MorphGNT codes at {w.Id}.");
        }
        if (wordIds.Count == 0 && !annotationOverlay) errors.Add("Corpus has no words.");
        if (corpus.Metadata.GetValueOrDefault("textCoverage") == "full")
        {
            if (!wordIds.Order().SequenceEqual(Enumerable.Range(1, wordIds.Count))) errors.Add("Full-text word IDs must run consecutively from 1.");
            foreach (var verse in corpus.Words.GroupBy(w => w.Verse))
                if (!verse.OrderBy(w => w.Id).Select(w => w.Position).SequenceEqual(Enumerable.Range(1, verse.Count())))
                    errors.Add($"Full-text word positions are incomplete in {verse.Key}.");
        }
        var annotations = new Dictionary<string, Annotation>();
        foreach (var a in corpus.Annotations)
        {
            try { XmlConvert.VerifyNCName(a.Id); }
            catch (Exception e) when (e is XmlException or ArgumentException) { errors.Add($"Invalid annotation ID: {a.Id}"); }
            if (a.Id.StartsWith("gntc_", StringComparison.Ordinal) || Regex.IsMatch(a.Id, "^w[0-9]+$")) errors.Add($"Reserved ID: {a.Id}");
            if (!annotations.TryAdd(a.Id, a)) errors.Add($"Duplicate annotation ID: {a.Id}");
            if (!Records.Fields.TryGetValue(a.Kind, out var fields) || fields.Length != a.Args.Count || a.Args.Any(x => x is null))
                errors.Add($"Invalid fields in {a.Kind} {a.Id}.");
        }
        foreach (var a in corpus.Annotations)
        {
            if (!Records.Fields.TryGetValue(a.Kind, out var fields) || fields.Length != a.Args.Count || a.Args.Any(x => x is null)) continue;
            void Span(string value, bool special = false)
            {
                if (special && value is "?" or "-") return;
                try
                {
                    if (!annotationOverlay && Extent.Parse(value).Any(id => !wordIds.Contains(id))) errors.Add($"{a.Id}: extent references missing words.");
                    else _ = Extent.Parse(value);
                }
                catch (FormatException e) { errors.Add($"{a.Id}: {e.Message}"); }
            }
            void Ref(string id, params string[] kinds)
            {
                if (id == "?") return;
                if (!annotations.TryGetValue(id, out var target) || !kinds.Contains(target.Kind)) errors.Add($"{a.Id}: invalid reference {id}.");
            }
            if (a.Kind == "U")
            {
                Span(a.Args[0]);
                if (!Profile.UnitKinds.Contains(a.Args[1])) errors.Add($"{a.Id}: invalid unit kind.");
            }
            if (a.Kind == "Q") Span(a.Args[0]);
            if (a.Kind == "C")
            {
                Span(a.Args[0], true); Ref(a.Args[1], "U", "K"); Ref(a.Args[2], "U", "K");
                if (a.Args[3] != "?" && a.Args[3] != "-" && !Profile.Operations.ContainsKey(a.Args[3])) errors.Add($"{a.Id}: unknown operation.");
                if (a.Args[4] != "?" && !Profile.Relations.ContainsKey(a.Args[4])) errors.Add($"{a.Id}: unknown relation.");
                if (a.Args[5] != "?" && !Profile.Domains.Contains(a.Args[5])) errors.Add($"{a.Id}: unknown domain.");
                if (a.Args[0] == "-" && a.Args[3] != "-") errors.Add($"{a.Id}: an implicit relation has no marker operation (use '-').");
                if (a.Args[0] != "-" && a.Args[3] == "-") errors.Add($"{a.Id}: an overt or unknown marker needs an operation or '?'.");
                if (corpus.Attribute(a, "status") == "reviewed" && a.Args.Any(x => x == "?")) errors.Add($"{a.Id}: reviewed connection has unresolved fields.");
            }
            if (a.Kind == "K" && !Regex.IsMatch(a.Args[0], "^[A-Za-z_][A-Za-z0-9_.-]*$")) errors.Add($"{a.Id}: invalid context kind.");
            if (a.Kind == "I")
            {
                Ref(a.Args[0], "C");
                if (a.Args[0] == "?") errors.Add($"{a.Id}: inference must identify a connection.");
                if (corpus.Attribute(a, "force") is not ("defeasible" or "entailed" or "hypothesis")) errors.Add($"{a.Id}: inference needs force=defeasible|entailed|hypothesis.");
            }
            if (a.Kind == "A")
            {
                var members = a.Args[1].Split(',');
                if (a.Args[0] is not ("exclusive" or "inclusive") || members.Length < 2 || members.Distinct().Count() != members.Length)
                    errors.Add($"{a.Id}: invalid alternative group.");
                foreach (var id in members) { Ref(id, "C"); if (id == "?") errors.Add($"{a.Id}: alternative cannot be unknown."); }
            }
            var status = corpus.Attribute(a, "status");
            if (status is not null && !Profile.Statuses.Contains(status)) errors.Add($"{a.Id}: unknown status.");
            if (a.Kind is "U" or "K" or "C" or "I" or "A")
            {
                if (status is null || string.IsNullOrWhiteSpace(corpus.Attribute(a, "by"))) errors.Add($"{a.Id}: interpretation requires status and by (or defaults).");
            }
            foreach (var (key, value) in a.Attributes)
            {
                if (!Regex.IsMatch(key, "^[A-Za-z0-9_.-]+$")) errors.Add($"{a.Id}: invalid attribute key.");
                if (key.StartsWith("conf.", StringComparison.Ordinal) && value is not ("high" or "medium" or "low" or "unknown"))
                    errors.Add($"{a.Id}: confidence is high|medium|low|unknown, not an uncalibrated probability.");
            }
        }
        return errors;
    }

    public static void RequireValid(Corpus corpus)
    {
        var errors = Errors(corpus);
        if (errors.Count != 0) throw new FormatException(string.Join('\n', errors));
    }
}
