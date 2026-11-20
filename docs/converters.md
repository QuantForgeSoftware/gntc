# C# converters and CLI

Requires .NET 10. The core library uses only the .NET base class library: `System.Text.Json`, LINQ to XML, cryptography, and text APIs. xUnit and test infrastructure are development dependencies.

## Formats and fidelity

| Input/output | Guarantee |
|---|---|
| GNTC ↔ JSON ↔ GNTC-profile TEI | Same normalized annotation graph, source fields, Unicode, IDs, provenance, and alternatives |
| MorphGNT → GNTC | All seven source columns imported; source-byte checksum recorded |
| GNTC → MorphGNT | Source text/morphology only; all required morphology must be present |
| Graph → AI prompt | Deliberate projection; selected morphology can be omitted and is marked as such |
| Arbitrary TEI → GNTC | Unsupported; rejected when it does not match the documented profile |

For the vendored LF, UTF-8 source file, source export is byte-identical. For arbitrary inputs with another whitespace layout, export preserves the fields but canonicalizes to single spaces and LF. Arbitrary TEI metadata, comments, custom elements, alternate schemas, and external entity definitions are not silently accepted. Original file formatting is not part of the graph's round-trip contract.

## CLI usage

Prefix commands with `dotnet run --project src/Gntc.Cli --` from the repository root, or publish the CLI:

```sh
dotnet publish src/Gntc.Cli -c Release -o artifacts/cli
dotnet artifacts/cli/Gntc.Cli.dll --help
```

`import RAW OUTPUT --book 1Ti` reads one book in MorphGNT format, preserves the source numbering, computes SHA-256, and writes a graph. Add repository/revision/license metadata before sharing an import. The repository's `build` command supplies those fields from the pinned manifest.

`convert INPUT OUTPUT` infers formats from `.gntc`, `.json`, and `.xml`. It validates before writing. Writes use a temporary file and replacement, so a conversion error does not leave a partially written target.

`validate INPUT --source RAW` checks graph structure and retained source fields against the exact bytes. `--require-reviewed` additionally rejects interpretive records without reviewed status; it does not prove linguistic correctness or certify complete candidate adjudication. The current research draft is expected to fail that additional review gate.

`scan INPUT OUTPUT` creates a deliberately broad candidate inventory. It does not add C records or infer relations. It refuses to scan a graph that already contains Q records; rebuild from the source when changing the policy.

`annotate INPUT OVERLAY OUTPUT` merges an interpretive overlay only when source hashes match and IDs/metadata do not conflict. A failed overlay application leaves the input graph unchanged. Overlay files contain no T/R/M records and are not standalone corpora.

`extract INPUT OUTPUT --verses 1Ti.4.7,1Ti.4.8` selects exact verse names. It seeds connections whose markers intersect the selection, includes candidate records and fully contained units, then follows referenced units/contexts, associated effects, and alternative-group members. All words in the retained spans are included, even outside the selected verses. It does not repeatedly select every new marker in the added context, which would grow the selection indefinitely. Unknown or unexpressed context remains explicit. Implicit connections are not automatically seeded by marker-based extraction; use the full graph when studying them.

`prompt INPUT OUTPUT --verses ... --morphology markers|full|none` produces a local text prompt with format instructions, definitions of used labels, and a fenced GNTC payload. Default morphology is `markers`, referring to words in retained C marker extents. It sends nothing to an AI service. It never presents proposals as reviewed.

`report INPUT OUTPUT.tsv` lists candidates, source references, and overlapping connection proposals. An overlap is only a review aid; it does not establish that a whole multiword candidate has been adjudicated.

`export-source INPUT OUTPUT.txt` explicitly exports only the seven MorphGNT columns. Annotation data remains in the original graph. `build [ROOT]` reproduces the full book, examples, prompt, review queue, profile, and coverage report from the source and overlay.

Exit code 0 means success; 2 means invalid arguments, formats, references, or another handled failure. Unknown commands, unsupported options, and repeated options fail rather than being ignored.

## Library example

```csharp
using Gntc.Core;

var source = File.ReadAllBytes("data/source/75-1Ti-morphgnt.txt");
var graph = MorphGntFormat.Read(source, "1Ti");
CorpusOperations.ScanCandidates(graph);
var overlay = CompactFormat.Read(
    File.ReadAllText("data/annotations/1tim.annotations.gntc"));
CorpusOperations.Apply(graph, overlay);
Validator.RequireValid(graph);
MorphGntFormat.VerifySource(graph, source);

var xml = TeiFormat.Write(graph);
var restored = TeiFormat.Read(xml);
var compact = CompactFormat.Write(restored);
var json = JsonFormat.Write(restored);
```

The `CorpusFiles` facade combines parsing and validation. Low-level readers expose a graph so callers can inspect validation errors; call `Validator.RequireValid` before relying on such a graph. `TeiFormat.Read` additionally verifies the emitted structural XML against its feature structures. `Corpus.Canonicalize` and writers sort collections in place; clone via JSON when preserving in-memory collection order matters.
