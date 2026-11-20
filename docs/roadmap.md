# What else belongs in this repository?

## Included in the initial implementation

- A source manifest with exact revision and checksum, the untouched upstream file and README, attribution, and component-specific licenses.
- A documented grammar, versioned vocabulary, explicit directionality, escaping rules, source IDs, and separate candidate/interpretation status.
- Whole-book source conversion; a substantial draft application; competing analyses; an unresolved review queue; and a smaller worked example.
- C# library/CLI converters, validation, source export, extraction with dependency closure, and local AI prompt generation.
- Unit/integration tests, JSON Schema, pinned TEI Relax NG, supplemental Schematron, tokenizer measurements, reproducible generation, and CI.
- A contribution process and an annotation issue template with source anchors and evidence.

## Research priorities

1. **Independent expert review.** Audit every C/I/U classification, especially wide scopes, 1:3, 2:12–15, 3:11, 4:9–10, 5:13, 6:2, and 6:7. Assign responsibility and adjudicate disagreement without erasing alternatives.
2. **An adjudicated reference set.** Separate exploratory annotation from accepted labels; define reviewer agreement measures and retain evidence for minority readings.
3. **Explicit candidate assessment.** Add versioned records for connective, nonconnective, uncertain, and composite-expression judgments. An overlapping C currently does not prove complete assessment of a Q.
4. **An annotation handbook with counterexamples.** Document how to distinguish lexical constraint, contextual coherence, content complement, inference, negation scope, speech-act support, and text order. Test difficult cases against more than one theoretical framework.
5. **AI accuracy evaluation.** Run the protocol in `ai-analysis.md`, measuring interpretive quality alongside tokens. The tokenizer report is a size measurement only.

## Engineering extensions

1. A TEI ODD customization and compiled schema for the exact profile, including conventional `respStmt`, `certainty`, and `interp` mappings for provenance and confidence. The current strict profile is specified in prose, named feature structures, Schematron, and C# validation.
2. Adapters for broader TEI documents, with explicit migration reports for unsupported information. Keep original documents available; never silently discard witness/apparatus or editorial information.
3. Separate syntax, semantic-role, information-structure, and coreference layers. A dedicated rule for negation/attribution would be preferable to the initial string attributes once enough cases are reviewed.
4. Cross-edition/witness alignment, apparatus readings, token splits/merges, and migrations when the source changes. Word IDs cannot simply be reused across editions.
5. A small annotation/review UI with parallel Greek and graph views, alternative selection, scope highlighting, and reviewer history.
6. Standard packaging: versioned NuGet library, CLI tool package, signed releases, and a larger corpus after the 1 Timothy pilot is reviewed.
7. Deterministic prompt budgets with explicit context inclusion reports, and optional tokenizer-aware exports that never truncate a referenced scope silently.

These are planned extensions, not claims about features already implemented.
