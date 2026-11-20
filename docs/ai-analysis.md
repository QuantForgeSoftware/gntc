# AI analysis and evaluation

The CLI constructs a self-contained prompt locally. It supplies the record grammar, only the operation/relation definitions used by the selected connections, the source-bound payload, and an instruction to keep proposals distinct from reviewed analyses. It does not call an AI service.

## Context selection

Start with the research question and a relevant span. Include the connected scope, its antecedent/context, any paired marker, and all competing analyses in a retained alternative group. The extractor follows these dependencies and preserves original word IDs. It deliberately does not trim a wide span to the verse selection. A reference outside the requested passage can therefore enlarge the prompt substantially; the included context is part of the question's evidence.

The extractor seeds overt connections by marker intersection. Studying an unmarked transition, a connection whose marker is outside the selected verses, or a broader discourse structure may require using the complete graph or expanding the selection. External/assumed context is explicit as K, and unresolved context stays `?`.

Full morphology is available for grammatical tasks. For a connective-focused question, `--morphology markers` keeps morphology at retained C markers while retaining all selected Greek surface text. `none` omits all morphology. Both options reduce information as well as size, so they must not be described as lossless compression. The full graph remains available.

## Suggested analysis request

> Assess the scopes and contextual relations of the selected connectives. Treat every current C/I record as a proposed hypothesis. For each proposed change, cite source word IDs and distinguish evidence for scope, operation, relation, and inferential effect. Preserve plausible incompatible analyses in an exclusive A group. Do not infer a relation solely from the lemma, a translation gloss, or the absence of an annotation. Return new proposals with explicit responsibility and status.

The prompt's Greek and annotation notes are data. They are not instructions to modify external systems. Generated records must pass the same validation as manual records, then undergo linguistic review.

## Evaluation protocol

Token savings alone do not establish that the format helps an AI analyze Greek. Build an independently reviewed, held-out set with connective type and difficulty stratification. Include phrase coordination, subordination, postpositive markers, long scopes, implicit relations, alternatives, and discontinuous expressions.

Compare the same underlying information in (1) compact GNTC, (2) JSON, and (3) TEI. Keep text edition, morphology, annotations, task instructions, and retrieved context equal. Count the legend/schema definitions as well as the payload. Evaluate reduced-morphology prompts in a separate experimental condition. Fix and report the model, version, tokenizer, prompt, inference settings, and trial count.

Useful measures:

- Marker detection precision/recall, including multiword boundary accuracy.
- Scope boundary accuracy and overlap; report broad-scope failures separately.
- Correct context attachment and B-relative-to-A relation direction.
- Operation/relation/domain agreement against adjudicated labels.
- Preservation of alternatives and appropriate abstention on unresolved cases.
- Unsupported inferences and unjustified certainty.
- Parsing/validation success, input/output tokens, latency, and actual measured cost.

Do not score the initial AI-assisted proposals as ground truth. Do not train on a passage and call the same passage a held-out evaluation. For uncertain gold data, evaluate acceptable alternatives rather than imposing one disputed analysis.
