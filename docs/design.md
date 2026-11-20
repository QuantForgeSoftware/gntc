# Design rationale

The design responds to three requirements for a connective: it links a unit of any size to context, instructs the reader how to relate the unit to that context, and constrains the conclusions that may be drawn from the connection. This formulation appears in Stephen H. Levinsohn's adaptation of Reboul and Moeschler (1998:77), in [Self-Instruction Materials on Narrative Discourse Analysis, §6.2](https://www.bibelgriechisch.online/LEVINSOHNnarr.pdf). The researcher's quoted definition motivated the project; this document paraphrases its requirements.

## Text once, annotation separately

GNTC is a stand-off annotation graph. Immutable source words carry stable, edition-bound identity. Units are explicit inclusive ranges or ordered unions of ranges. Connections point to those units. An annotation need not be embedded in the Greek or constrained to a nested XML tree. Intersecting, nested, and discontinuous spans are all possible.

TEI remains a rich archival/interchange representation. Compact text is an AI-facing serialization of a selected annotation model. The C# code implements a common typed graph, with JSON as a conventional application-facing representation. It does not attempt to encode every feature of arbitrary TEI.

## What connects to what?

A `C` record identifies the marker, context A, and connected scope B separately. A can be an earlier unit, a following main clause, a distant section, or an explicitly proposed contextual assumption (`K`). B can be a word, phrase, clause, clause complex, or discourse section. Verse boundaries locate the source; they do not determine the boundaries of meaning.

The marker is not the beginning or entirety of B. Postpositive γάρ and δέ may occur inside a unit whose start precedes them. A marker can consist of several adjacent words or nonadjacent members. More than one connection can be associated with the same marker. The scope of one connection may include another connection.

In 1 Timothy 4:8 the supplied example has γάρ at 795, a broad scope of 794–816, and an internal δέ at 803 connecting the two main portions. This analysis is informed by [Levinsohn's 1 Timothy notes, pp. 18–19](https://scholars.sil.org/sites/scholars/files/stephen_h_levinsohn/discourse/1timcomments.pdf). The labels and boundaries remain reviewable proposals.

## Operation, relation, and domain

Operation records a proposed processing instruction. Relation records the contextual relationship. Domain distinguishes content, epistemic reasoning, speech acts, discourse organization, and syntactic complements. These are project-specific vocabulary choices, not TEI-standard categories or a claim of consensus.

An analysis can assign `develop` to a δέ connection and `contrast` to the relation in that occurrence. A γάρ connection can `support` a directive in the `act` domain: the following material supplies grounds for the exhortation. That is different from an event causing another event. A conjunction introducing reported or embedded content can be represented in `syntax` without treating it as an inferential discourse relation.

The repository deliberately does not contain a rule that assigns relation or scope from lemma alone. Candidate scanning uses morphology and two lexical expressions solely to create a review inventory. Contextual connection proposals are maintained separately.

## Inferential effects

A reusable operation definition states the general processing constraint. An optional `I` record gives a passage-specific interpretive effect and marks its force as `defeasible`, `hypothesis`, or `entailed`. The initial corpus uses defeasible effects.

Removing a connective can remove an explicit signal while leaving the same relation inferable from other evidence. Never infer “no connective, therefore no connection.” Equally, recording an operation does not prove a counterfactual claim about what every reader would think without the marker. An explicit `I` hypothesis can record that claim for later testing.

The operation and relation fields are interpretations. Surface forms and source morphology are observations imported from the selected edition. `by`, `src`, `status`, and field-specific `conf.*` attributes keep their evidential status separate. `?` is unresolved, `-` is absent/not applicable in the documented positions, and omitted annotations carry no negative assertion.

## Keeping competing analyses

Exclusive `A` groups connect full alternative `C` records; they do not combine alternative scopes into a single union. The initial corpus retains alternative antecedents at 2:1 and alternative attachments around the faithful saying at 4:9–10. No probability estimates are assigned.

Multiple readings that can coexist use separate connections or an inclusive group. A discontinuous span means that all listed pieces jointly belong to the unit; it never means “one or the other.”

## General grammatical analysis

The `M` layer retains lemma, source POS, parsing, word form, and normalized form without translating between tagsets. In this repository it is MorphGNT/CCAT morphology from the specified SBLGNT fork, not Friberg morphology from the earlier discussion. The earlier Friberg example illustrated the model only; none of its words or tags replaces this repository's source.

Additional syntax, information structure, semantic roles, textual variants, and parallel-witness alignment should be separate versioned layers over the same word identities. The current `C` vocabulary supports some grammatical connections, but is not a full dependency-tree or constituent-tree format. Richer grammatical layers require a documented schema revision or extension profile.
