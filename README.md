# GNTC — Greek New Testament Compact annotations

GNTC is a compact, source-anchored format for grammatical and discourse analysis of the Greek New Testament. It stores Greek once and expresses connectives as a graph over explicit spans. C# converters preserve that graph across compact text, JSON, and a documented TEI P5 profile.

The repository applies the format to **all 1,591 words, 113 verses, and six chapters of 1 Timothy** from [QuantForgeSoftware/sblgnt](https://github.com/QuantForgeSoftware/sblgnt), pinned to commit [`aaed91e57c8e4a8dc9a2383e129ca5e75fe6393d`](https://github.com/QuantForgeSoftware/sblgnt/tree/aaed91e57c8e4a8dc9a2383e129ca5e75fe6393d). All seven MorphGNT columns and the supplied textual-apparatus marks are preserved.

**Research status:** 211 proposed connections, 393 units, nine interpretive effects, two groups of competing analyses, and 288 candidate records. **No interpretation has been reviewed by a specialist.** Candidate coverage is complete for the declared scanning policy; interpretive coverage is partial. Of the source's 201 conjunction-tagged tokens, 199 participate in proposals; καθώς at 1:3 and ὅτι at 6:7 remain unresolved. Candidates include many adverbs and particles that may not function as connectives. See [coverage](data/coverage.json) and the [review queue](data/1tim.candidates.tsv).

## Quick start

Requires the .NET 10 SDK. The C# core and CLI have no third-party runtime dependencies. Commands below run from the repository root.

```sh
dotnet build Gntc.sln
dotnet test Gntc.sln
dotnet run --project src/Gntc.Cli -- build .
dotnet run --project src/Gntc.Cli -- validate data/1tim.gntc --source data/source/75-1Ti-morphgnt.txt

# Convert the complete graph in either direction.
dotnet run --project src/Gntc.Cli -- convert data/1tim.gntc artifacts/1tim.json
dotnet run --project src/Gntc.Cli -- convert data/1tim.json artifacts/1tim.tei.xml
dotnet run --project src/Gntc.Cli -- convert artifacts/1tim.tei.xml artifacts/restored.gntc

# Include the complete referenced scopes, even when they cross the requested verses.
dotnet run --project src/Gntc.Cli -- extract data/1tim.gntc artifacts/excerpt.gntc --verses 1Ti.4.7,1Ti.4.8
dotnet run --project src/Gntc.Cli -- prompt data/1tim.gntc artifacts/prompt.txt --verses 1Ti.4.7,1Ti.4.8

# Explicitly export the source layer only; discourse annotations are omitted.
dotnet run --project src/Gntc.Cli -- export-source data/1tim.gntc artifacts/75-1Ti-morphgnt.txt
```

`convert` accepts `.gntc`, `.json`, and `.xml` in every pairing. TEI input must conform to this project's profile; unsupported external TEI and inconsistent XML/feature structures are rejected rather than silently discarded. `export-source` is an intentionally narrower operation. See [CLI and C# API](docs/converters.md).

## The central record

```text
C id marker A B operation relation domain
```

Read it as: **marker M instructs the reader to interpret B in relation to context A**. A and B are semantic roles, not assumptions about text order. The operation is the processing instruction attributed to the marker; the relation is the relationship identified in this context.

The main worked example uses whole-book word IDs:

```text
U u789_793 789-793 clause
U u794_801 794-801 clause
U u802_816 802-816 complex
U u794_816 794-816 discourse
C c795 795 u789_793 u794_816 support ground act src=levinsohn-1tim
C c803 803 u794_801 u802_816 develop contrast content src=levinsohn-1tim
```

Word 795 is γάρ at 1 Timothy 4:8, word 2. The scope of its supporting material is **794–816**, encompassing both main clauses of verse 8. The δέ at word 803 introduces an internal development with contextual contrast. Supporting an exhortation is represented in the `act` domain, distinct from one event causing another. The file metadata identifies these as proposals by `codex`; the snippets above inherit those defaults.

This example is informed by [Stephen H. Levinsohn's discussion of 1 Timothy 4:7–8](https://scholars.sil.org/sites/scholars/files/stephen_h_levinsohn/discourse/1timcomments.pdf). The project is not an endorsed edition of that scholar's annotations. Read the [complete example](examples/1tim-4.7-8.gntc), [TEI version](examples/1tim-4.7-8.tei.xml), and [AI prompt](examples/1tim-4.7-8.prompt.txt). The example includes context from 4:6 because a selected connection points there.

## Repository contents

| Location | Purpose |
|---|---|
| [docs/design.md](docs/design.md) | Rationale, the researcher's three concerns, and the separation of observation and interpretation |
| [docs/format.md](docs/format.md) | Normative GNTC/1 syntax, IDs, ranges, escaping, defaults, and validation |
| [docs/annotation-guide.md](docs/annotation-guide.md) | Annotation procedure, uncertainty, hard cases, and review priorities |
| [docs/tei-profile.md](docs/tei-profile.md) | Exact TEI mapping, losslessness boundary, and customization requirements |
| [docs/ai-analysis.md](docs/ai-analysis.md) | Prompt construction, scope closure, and evaluation protocol |
| [docs/roadmap.md](docs/roadmap.md) | Further research and implementation work |
| [profiles/](profiles/) | Versioned processing instructions and directional relation definitions |
| [schemas/](schemas/) | JSON Schema, TEI Schematron constraints, and pinned official TEI Relax NG schema |
| [data/source/](data/source/) | Unmodified source, upstream README, and checksum manifest |
| [data/annotations/](data/annotations/) | Editable interpretive overlay, separate from source words |
| [data/1tim.gntc](data/1tim.gntc) | Entire book in compact notation |
| [data/1tim.json](data/1tim.json) | Entire book as a conventional typed JSON graph |
| [data/1tim.tei.xml](data/1tim.tei.xml) | Entire book as TEI P5 with stand-off annotations |
| [src/](src/) | C# library and command-line application |
| [tests/](tests/) | Whole-book round trips, Unicode, discontinuity, alternatives, source integrity, and rejection tests |
| [scripts/](scripts/) | Independent schema checks and reproducible tokenizer measurements |

## Efficiency and validation

The compact format removes repeated XML wrappers and uses shared word references, metadata defaults, and short readable operation labels. AI prompts can additionally omit morphology not needed for the question; this is explicitly marked as a projection. [Measured results](docs/benchmarks.md) distinguish equal-information format comparisons from reduced-information prompts.

The test suite checks semantic preservation through every pair of supported graph formats. Source verification checks the exact source SHA-256 and every retained field, and the complete source can be exported byte for byte. Official TEI Relax NG validation and JSON Schema checks supplement the C# graph validator. Passing these checks establishes structural consistency, not the correctness of a linguistic analysis.

## Licensing and contribution

Code and original documentation: MIT. SBLGNT text: CC BY 4.0. MorphGNT morphology and the adapted research data: CC BY-SA 3.0. The pinned TEI schema has its own license. See [NOTICE.md](NOTICE.md) and [LICENSE](LICENSE) before redistributing a combined artifact.

Read [CONTRIBUTING.md](CONTRIBUTING.md) to propose annotations or converter changes. High-priority next work is independent Greek discourse review, adjudicated reference annotations, and an AI evaluation comparing accuracy as well as token counts.
