using System.Text.Json;
using Gntc.Core;

try
{
    if (args.Length == 0 || args[0] is "help" or "--help" or "-h")
    {
        Console.WriteLine("""
        GNTC/1 — compact Greek grammatical and discourse annotations (.NET 10)

        import RAW.txt OUTPUT.gntc --book 1Ti
        convert INPUT OUTPUT                 (.gntc, .json, .xml)
        export-source INPUT OUTPUT.txt       (text/morphology only; annotations omitted)
        validate INPUT [--source RAW.txt] [--require-reviewed]
        scan INPUT OUTPUT                    (candidate inventory, not automatic analysis)
        annotate INPUT OVERLAY.gntc OUTPUT
        extract INPUT OUTPUT --verses 1Ti.4.7,1Ti.4.8
        prompt INPUT OUTPUT.txt [--verses ...] [--morphology markers|full|none]
        report INPUT OUTPUT.tsv
        build [REPOSITORY_ROOT]               (reproduce committed 1 Timothy artifacts)

        TEI input must match the documented GNTC profile. Arbitrary TEI is rejected.
        Commands run locally; they do not send the corpus to an AI service.
        """);
        return 0;
    }
    var allowed = args[0] switch
    {
        "import" => new[] { "--book" },
        "validate" => ["--source", "--require-reviewed"],
        "extract" => ["--verses"],
        "prompt" => ["--verses", "--morphology"],
        _ => []
    };
    var positional = new List<string>();
    var options = new Dictionary<string, string>();
    for (var i = 1; i < args.Length; i++)
    {
        if (!args[i].StartsWith("--", StringComparison.Ordinal)) { positional.Add(args[i]); continue; }
        var key = args[i];
        if (!allowed.Contains(key) || options.ContainsKey(key)) throw new ArgumentException($"Unknown or repeated option: {key}");
        if (key == "--require-reviewed") options[key] = "true";
        else
        {
            if (++i == args.Length || args[i].StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException($"Missing value for {key}.");
            options[key] = args[i];
        }
    }
    void Count(int count)
    {
        if (positional.Count != count) throw new ArgumentException($"{args[0]} requires {count} positional arguments; see --help.");
    }
    string Required(string key) => options.GetValueOrDefault(key) ?? throw new ArgumentException($"Missing {key}.");
    Corpus Input() => CorpusFiles.Read(positional[0]);
    switch (args[0])
    {
        case "import":
            Count(2);
            var imported = MorphGntFormat.Read(File.ReadAllBytes(positional[0]), Required("--book"));
            CorpusFiles.Write(imported, positional[1]);
            break;
        case "convert":
            Count(2); CorpusFiles.Write(Input(), positional[1]); break;
        case "export-source":
            Count(2); CorpusFiles.WriteText(positional[1], MorphGntFormat.Write(Input()));
            Console.WriteLine("Exported source text and morphology; discourse annotations are retained only in the original input.");
            break;
        case "validate":
            Count(1);
            var validated = Input();
            if (options.TryGetValue("--source", out var sourcePath)) MorphGntFormat.VerifySource(validated, File.ReadAllBytes(sourcePath));
            if (options.ContainsKey("--require-reviewed") && validated.Annotations.Any(a => a.Kind is "C" or "I" or "U" or "K" or "A" && validated.Attribute(a, "status") != "reviewed"))
                throw new FormatException("Interpretive annotations are not all reviewed.");
            Console.WriteLine($"Valid {validated.Format}: {validated.Words.Count} words; {validated.Annotations.Count(a => a.Kind == "C")} connections; {validated.Annotations.Count(a => a.Kind == "Q")} candidates. Validation does not establish linguistic correctness.");
            break;
        case "scan":
            Count(2); var scanned = Input(); CorpusOperations.ScanCandidates(scanned); CorpusFiles.Write(scanned, positional[1]); break;
        case "annotate":
            Count(3); var annotated = Input(); CorpusOperations.Apply(annotated, CompactFormat.Read(File.ReadAllText(positional[1]))); CorpusFiles.Write(annotated, positional[2]); break;
        case "extract":
            Count(2); CorpusFiles.Write(CorpusOperations.Extract(Input(), Required("--verses").Split(',')), positional[1]); break;
        case "prompt":
            Count(2); var promptCorpus = Input();
            if (options.TryGetValue("--verses", out var selection)) promptCorpus = CorpusOperations.Extract(promptCorpus, selection.Split(','));
            CorpusFiles.WriteText(positional[1], CorpusOperations.Prompt(promptCorpus, options.GetValueOrDefault("--morphology", "markers"))); break;
        case "report":
            Count(2); CorpusFiles.WriteText(positional[1], CorpusOperations.CandidateReport(Input())); break;
        case "build":
            if (positional.Count > 1) throw new ArgumentException("build takes zero or one repository root.");
            Build(positional.Count == 1 ? positional[0] : "."); break;
        default: throw new ArgumentException($"Unknown command: {args[0]}. Use --help.");
    }
    return 0;
}
catch (Exception exception) when (exception is ArgumentException or FormatException or IOException or JsonException or System.Xml.XmlException or InvalidOperationException or KeyNotFoundException)
{
    Console.Error.WriteLine($"error: {exception.Message}");
    return 2;
}

static void Build(string root)
{
    string At(string path) => Path.Combine(root, path);
    var manifest = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(At("data/source/manifest.json")))!;
    var raw = File.ReadAllBytes(At("data/source/" + manifest["path"]));
    var corpus = MorphGntFormat.Read(raw, manifest["book"]);
    if (corpus.Metadata["sourceSha256"] != manifest["sha256"]) throw new FormatException("Pinned source checksum failed.");
    corpus.Metadata["title"] = "1 Timothy — SBLGNT / MorphGNT, GNTC research draft";
    corpus.Metadata["sourceRepository"] = manifest["repository"];
    corpus.Metadata["sourceRevision"] = manifest["revision"];
    corpus.Metadata["sourcePath"] = manifest["path"];
    corpus.Metadata["textLicense"] = "CC-BY-4.0";
    corpus.Metadata["morphologyLicense"] = "CC-BY-SA-3.0";
    corpus.Metadata["annotationLicense"] = "CC-BY-SA-3.0";
    CorpusOperations.ScanCandidates(corpus);
    CorpusOperations.Apply(corpus, CompactFormat.Read(File.ReadAllText(At("data/annotations/1tim.annotations.gntc"))));
    MorphGntFormat.VerifySource(corpus, raw);
    foreach (var extension in new[] { "gntc", "json", "tei.xml" }) CorpusFiles.Write(corpus, At("data/1tim." + extension));
    CorpusFiles.WriteText(At("data/1tim.candidates.tsv"), CorpusOperations.CandidateReport(corpus));
    var example = CorpusOperations.Extract(corpus, ["1Ti.4.7", "1Ti.4.8"]);
    foreach (var extension in new[] { "gntc", "json", "tei.xml" }) CorpusFiles.Write(example, At("examples/1tim-4.7-8." + extension));
    CorpusFiles.WriteText(At("examples/1tim-4.7-8.prompt.txt"), CorpusOperations.Prompt(example));
    CorpusFiles.WriteText(At("profiles/gntc-connectives-1.json"), JsonSerializer.Serialize(new
    {
        id = Profile.Id, operations = Profile.Operations, relations = Profile.Relations,
        domains = Profile.Domains, unitKinds = Profile.UnitKinds, statuses = Profile.Statuses
    }, JsonFormat.Options) + "\n");
    var report = new
    {
        sourceRevision = manifest["revision"], sourceSha256 = manifest["sha256"],
        words = corpus.Words.Count, verses = corpus.Words.Select(w => w.Verse).Distinct().Count(),
        chapters = corpus.Words.Select(w => w.Verse.Split('.')[1]).Distinct().Count(),
        units = corpus.Annotations.Count(a => a.Kind == "U"), candidates = corpus.Annotations.Count(a => a.Kind == "Q"),
        connections = corpus.Annotations.Count(a => a.Kind == "C"),
        proposed = corpus.Annotations.Count(a => a.Kind == "C" && corpus.Attribute(a, "status") == "proposed"),
        reviewed = corpus.Annotations.Count(a => a.Kind == "C" && corpus.Attribute(a, "status") == "reviewed"),
        effects = corpus.Annotations.Count(a => a.Kind == "I"), alternatives = corpus.Annotations.Count(a => a.Kind == "A"),
        candidatePolicy = corpus.Metadata["candidatePolicy"], analysisCoverage = corpus.Metadata["analysisCoverage"]
    };
    CorpusFiles.WriteText(At("data/coverage.json"), JsonSerializer.Serialize(report, JsonFormat.Options) + "\n");
    Console.WriteLine(JsonSerializer.Serialize(report, JsonFormat.Options));
}
