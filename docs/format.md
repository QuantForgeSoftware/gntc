# GNTC/1 format specification

Status: implemented project format, version 1. Vocabulary: `gntc-connectives-1`. This is not an official TEI or linguistic standard.

## Encoding and grammar

UTF-8; LF is the canonical line ending. Readers accept CRLF and an initial UTF-8 BOM. Blank lines and whole-line `#` comments are ignored. The first non-comment record must be exactly `@GNTC/1`. Unknown record types and versions are errors.

String values must be representable in XML 1.0 so the TEI round-trip contract holds. Invalid control characters are rejected. MorphGNT source export additionally forbids whitespace inside a field because that source format uses whitespace as its delimiter.

```text
@GNTC/1
@meta key=value
R firstId verse firstWordPosition sourceBCV
T firstId word|word|word...
M wordId lemma POS parsing form normalized
U id extent unitKind attributes...
K id contextKind description attributes...
Q id marker evidence attributes...
C id marker A B operation relation domain attributes...
I id connectionId effect attributes...
A id exclusive|inclusive memberId,memberId... attributes...
```

Records use whitespace-separated fields, except the pipe-separated cells following `T firstId`. Strings containing whitespace, quotes, backslashes, pipes, or equals signs are JSON-quoted. Escapes use JSON rules. A quoted cell can contain a literal pipe. For example:

```text
K k1 assumption "An expectation involving a|b" status=proposed by=r1
I i1 c1 "A proposed effect.\nA second line." force=defeasible
```

Attribute and metadata names contain ASCII letters, digits, periods, underscores, or hyphens. Duplicate keys, duplicate word or annotation IDs, malformed quoting, reversed ranges, and overlapping segments within one extent are errors. Attribute values are strings; extension attributes round-trip without interpretation. Comments, whitespace layout, original attribute order, and unexpanded range spelling are not part of semantic identity.

## Source identity

`R 789 1Ti.4.7 8 150407` maps word ID 789 to the eighth source word of 1 Timothy 4:7. Subsequent IDs advance that position until the next `R`. An excerpt preserves original IDs and positions; it does not renumber them. Every text word needs a reference mapping. A mapping cannot begin at a nonexistent word.

`T` stores surface strings exactly, including source punctuation and apparatus marks. MorphGNT words, not LLM tokens, determine IDs. `M` stores the other source fields. Its final two columns are source word form and normalized form. This extends the original sketch's three-column morphology record so the full seven-column source is recoverable. Neither accents nor Unicode normalization are silently changed.

Production metadata binds IDs to `sourceRepository`, `sourceRevision`, `sourcePath`, `sourceSha256`, `book`, and `tokenization`. The checksum is SHA-256 of the vendored source bytes. Identical word IDs in different source revisions do not establish alignment. The corpus has a single book. `textCoverage=full` means full coverage of the imported source file, not a claim that every possible textual witness is represented.

`M` may be omitted for a word in a declared projection. All five morphology fields are present together or absent together. `morphologyCoverage=partial|none` distinguishes such a projection from the full source-bearing graph. Missing morphology cannot be reconstructed by guessing.

## Units and endpoints

An extent is a positive integer, an inclusive range, or an ascending comma-separated list of such pieces. `6-13,24-28` includes both spans. Pieces must not overlap or repeat IDs. Different units may overlap freely. All retained extent words must exist in the document. A safety limit prevents expanding extents over one million words.

`U` kinds are `word`, `phrase`, `clause`, `complex`, `verse`, and `discourse`. A `discourse` unit is a text span without a more specific syntactic commitment. A `complex` is a clause complex. Kinds are annotations, not segmentation inferred from punctuation. The first draft uses generic units extensively; its small-span classifications also need review.

`K` describes non-span context. Examples are `assumption`, `expectation`, and `external`. For an external text, include a precise `ref` attribute and say whether it has actually been retrieved. A K node is not proof of an author's unstated belief.

For `C`, A is context and B is the connected scope. Each is a `U` or `K` ID or `?`. A need not precede B. `marker` is an extent, `?`, or `-`. Use `-` only when there is no overt marker. An implicit connection must also have operation `-`; its contextual relation remains explicit. `?` means unresolved. Unknown operation, relation, or domain is `?`; these fields are never silently defaulted from a lemma.

Unit membership and connector position are independent. In phrase coordination, the marker may lie between the spans. In a postpositive construction it may lie inside B. These are legal. A paired expression may use `90,93`; annotations on its members can additionally be recorded separately when their roles differ.

## Vocabulary and inference

The complete directional vocabulary is in [the generated profile](../profiles/gntc-connectives-1.json); its implementation source is `Profile.cs`. Relation direction is **B relative to A**. For example, `ground` means B justifies A, `cause` means B causes A, and `result` means B results from A. `syntax` was added to the initial four-domain sketch to represent complement and grammatical links without conflating them with discourse inference.

`I` references one `C` and requires `force=defeasible|hypothesis|entailed`. Inferences are not source text. The current software preserves their wording; it does not prove entailment or evaluate a counterfactual experiment. Marker contribution and broader contextual support should be explained in the effect or a note.

`A exclusive c1a,c1b` states that at most one listed connection is selected in an analysis. It does not declare either one established. `inclusive` permits multiple members. Each group needs at least two different, existing C IDs. The release does not implement probability weights.

## Candidates, responsibility, and confidence

`Q` is an engineering extension to the initial proposal: a review candidate, not an asserted connection. Its extent anchors the candidate and its evidence says why it was selected. The scanner adds all tokens tagged C-, D-, or X-, plus the lemma sequences διά οὗτος and ἐκτός εἰ μή. This deliberately overgenerates; completeness is only with respect to that policy. A Q with no C is unresolved, not nonconnective.

Interpretive U/K/C/I/A records require `by` and `status`, directly or through metadata keys `default.by` and `default.status`. Local attributes override defaults; defaults never supply missing positional fields. Q records have their own `candidate` status. Status values are `candidate`, `proposed`, `reviewed`, and `rejected`. A reviewed C cannot contain unknown positional fields. Interpretations rejected after review should be retained with their provenance when appropriate.

`src` identifies supporting evidence, typically through a `source.<id>` metadata declaration. `by` can similarly use `agent.<id>`. The current validator preserves these strings but does not dereference external sources or authenticate an annotator. `conf.scope`, `conf.operation`, and `conf.relation` accept `high`, `medium`, `low`, or `unknown`. Omission means unspecified. Numeric probabilities are deliberately not inferred from these labels.

Metadata `analysisCoverage=partial` prevents treating a missing connection as a negative judgment. Source and candidate coverage are independent of analysis coverage. Reviewer completion and adjudication are research judgments outside schema validation.

## Canonical graph and integrity

JSON represents `format`, `metadata`, `words`, and `annotations`. Annotation arguments remain ordered according to the record definitions, while attributes are named strings. Canonical writers sort metadata and attributes ordinally, words by numeric ID, and annotations by record kind then ordinal ID. Converters preserve the normalized graph, including unknown values, statuses, effects, and alternatives.

The CLI rejects dangling endpoints, invalid extents, incorrect arity, unsupported vocabulary, malformed source references, and incomplete morphology. Source-aware validation additionally checks the checksum and retained source fields. Schema validation alone cannot prove scope, lexical semantics, attribution of unstated context, or theological conclusions.
