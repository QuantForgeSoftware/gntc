# Annotation guide and current coverage

The text is a source observation; every connection in this release is an AI-assisted proposal. No reviewer has certified the analysis. The absence of an annotation is not evidence of absent discourse structure.

## Procedure

1. Verify the edition, revision, tokenization, and exact word references. Retain source morphology even when its terminology differs from the annotation framework.
2. Inspect the full construction and surrounding discourse. Use the broad candidate list as a reminder, not a lexical classifier. Conjunction tags include syntactic uses; adverbs and expressions can have connective functions.
3. Identify the marker extent independently of scope. Account for postpositive position, multiword expressions, correlative pairs, shared negation, and ellipsis.
4. Identify A and B explicitly, including their boundaries. Retrieve nonadjacent context when necessary. Add a K assumption only with an explanation and proposed status; never invent an antecedent to satisfy the schema.
5. State the processing operation and contextual relation separately. Select the domain. A complement's content, a causal event relation, evidence for a belief, and motivation for an exhortation are different analyses.
6. Add an I effect when it clarifies the passage-specific inference. Specify whether it is defeasible, hypothetical, or entailed; explain the marker's contribution alongside other contextual evidence.
7. Record your identity, supporting source, status, and any field-specific uncertainty. Represent incompatible analyses as full alternative records in an exclusive A group.
8. Validate the graph and compare the passage in compact, JSON, and TEI views. An independent reviewer evaluates the linguistic claims and records any adjudication separately.

## Source-bound worked example

The original conversation's sample used local excerpt indices and Friberg data. The repository uses the requested SBLGNT/MorphGNT source, with whole-book indices. The principal correspondence is:

| Passage | Whole-book IDs | Role |
|---|---|---|
| 4:7, words 8–12 | 789–793 | Training exhortation |
| 4:8, words 1–8 | 794–801 | First main clause |
| 4:8, words 9–23 | 802–816 | Second main clause and dependent material |
| γάρ in 4:8 | 795 | Marker of the proposed support connection |
| δέ in 4:8 | 803 | Marker of the internal development |

The proposed γάρ scope is 794–816. The contextual contrast at δέ does not automatically become a dictionary definition of δέ. The effect describes the benefits of godliness as motivating the exhortation, with defeasible force.

## Initial dataset

The current draft contains 211 connections and covers 199 of 201 source conjunction-tagged words, together with selected markers tagged as adverbs, paired expressions, and one implicit relation. The candidate scanner creates 288 records across all C-, D-, and X-tagged words plus two lexical-expression patterns. These counts measure presence of proposals, not correctness or reviewer completion.

The 393 U records are useful spans for those connections, not a complete syntactic segmentation of 1 Timothy. Generic discourse units are used where a more precise syntactic classification has not been established. Short phrase/word classifications in the draft also warrant review.

Two conjunction-tagged occurrences intentionally have no C proposal:

- **1:3 καθώς (word 33):** its relation to the opening construction and incomplete/implicit continuation should be assessed with the larger discourse before fixing endpoints.
- **6:7 ὅτι (word 1361):** interpretation and the supplied reading require focused review. The source surface and apparatus sign are retained without substitution.

Additional candidates, including negators, focus adverbs, and discourse-related adverbs, remain unresolved or may ultimately be classified as nonconnective. The review queue labels overlapping proposals as `has-proposal`; this is not an adjudicated classification. An explicit negative-assessment record and reviewer completion model are roadmap items.

## Cases that require special attention

- **2:1 and 4:9–10:** the committed A groups retain alternative antecedents/scopes. Do not flatten them into a single union or assume the first option is selected.
- **2:12–15:** coordination, shared negation, the scope of the rationale, and the reference of the following clause are open research questions. The graph does not settle doctrinal applications.
- **3:11:** the ὡσαύτως comparison is represented without resolving the identification of γυναῖκας.
- **5:13:** a discontinuous not-only/but-also expression is separate from the intervening δέ connection.
- **5:19:** ἐκτὸς εἰ μή is treated as a multiword exception marker.
- **6:2:** the ὅτι rationale at 1278 falls under the prohibition. Its attributes `negationScope=ground` and `attribution=disallowed-motive` prevent reading the ground as endorsement of contempt. These attributes are preserved by converters; the validator does not reason about their semantics.
- **Implicit 4:11:** a proposed unmarked elaboration demonstrates `marker=-` and `operation=-`. It does not establish that every unmarked boundary has the same relation.

## Review record

A review should state the source revision and word references, what was reviewed (scope, operation, relation, inference, or all), the evidence, reviewer identity, and outcome. Proposed annotations should not be changed to reviewed solely because they parse successfully or an AI agrees. For disagreement, retain both analyses with attribution and a documented adjudication rather than overwriting the dissenting view.

Avoid numeric confidence without a calibration protocol. `high`, `medium`, and `low` express annotator judgments, not measured probabilities. Annotation agreement and linguistic validity need separate evaluation.
