# TEI P5 profile

The output is TEI P5 in namespace `http://www.tei-c.org/ns/1.0`, validated against the pinned [TEI 4.12.0 Relax NG schema](https://www.tei-c.org/Vault/P5/4.12.0/xml/tei/custom/schema/relaxng/tei_all.rng). The project uses standard TEI elements; its linguistic vocabulary and stricter round-trip profile are project conventions.

## Mapping

| Graph item | TEI representation |
|---|---|
| Corpus metadata | `standOff/fs[@xml:id='gntc_metadata']`, named string features |
| Verse/run | `text/body/ab`, verse citation in `@n` |
| Source token | `seg[@type='token']` containing `w` plus prefix/suffix `pc` elements |
| Word identity | `w/@xml:id='w<ID>'`; verse position in `@n` |
| Lemma/POS/parsing | `w/@lemma`, `@pos`, `@msd` |
| Source BCV and normalized form | Token `fs` referenced by `w/@ana` |
| Continuous U or Q | `span` with inclusive `@from`/`@to` pointers to words |
| Discontinuous U or Q | `join` targeting all member words in order |
| K context | `interp` with its kind and description |
| Overt C | `linkGrp` with `targFunc='marker context scope'` |
| Implicit C | Separate `linkGrp` with `targFunc='context scope'` |
| Multiword C marker | A generated `span` or `join` referenced by the link |
| I effect | `interp type='effect'`, linked to C through `@corresp` |
| A alternatives | `alt`, with `@mode='excl'` or `incl` and member targets |
| Annotation features/attributes | `fs type='gntc-annotation'` referenced by `@ana` |
| Unknown endpoint/marker | Explicit `gntc_unknown` interpretation node |

Annotation feature structures use named features from the record definition: `marker`, `context`, `scope`, `operation`, `relation`, `domain`, etc. Extension attributes are stored as `attr.<name>`. They are not embedded JSON. Provenance and field-specific confidence round-trip through these named features and metadata defaults; this release does not expand them into TEI `respStmt`/`certainty` networks. A future interoperable ODD should specify that additional mapping.

The source surface may contain punctuation and apparatus signs outside the source word form. The writer retains these as `pc` before or after `w` inside a token segment. This is a faithful lexical-layer rendering of the supplied signs, not a reconstruction of a critical apparatus with alternative readings. A form that cannot be located in its surface is rejected because a safe split cannot be assumed.

## Consistency and scope of support

The importer reconstructs the graph from named features and text, validates it, and compares the entire document structure with the canonical profile representation. A link whose target disagrees with its scope feature is rejected. Additional editorial notes, unrecognized XML, or changed generated metadata cannot be discarded silently. Insignificant XML formatting and attribute order do not affect the profile; textual whitespace inside values remains significant.

This strict contract is intentional for the initial converter. General TEI import needs explicit handling of additional editorial phenomena, `app/rdg` textual variants, manuscript witnesses, alternative tokenizations, and source metadata. Preserve such documents as originals and implement a migration adapter rather than forcing them through this importer.

`gntc_` is reserved for generated XML IDs; `w` followed by digits is reserved for word IDs. User annotation IDs must be XML NCNames and unique. All known pointers are local and resolve. Unknown values use a defined node rather than a dangling URI. XML DTDs and external entity resolution are disabled.

## Validation

```sh
xmllint --noout --relaxng schemas/tei_all-4.12.0.rng data/1tim.tei.xml
dotnet run --project src/Gntc.Cli -- validate data/1tim.tei.xml --source data/source/75-1Ti-morphgnt.txt
```

The included [Schematron](../schemas/gntc.sch) documents extra structural assertions. The C# reader enforces equivalent and stronger profile consistency checks. CI runs Relax NG and the C# checks; it does not claim that the Schematron file has been compiled into an XSLT validation pipeline.

TEI documentation: [standOff](https://www.tei-c.org/release/doc/tei-p5-doc/en/html/ref-standOff.html), [span](https://www.tei-c.org/release/doc/tei-p5-doc/en/html/ref-span.html), [join](https://www.tei-c.org/release/doc/tei-p5-doc/en/html/ref-join.html), [linkGrp](https://www.tei-c.org/release/doc/tei-p5-doc/en/html/ref-linkGrp.html), [fs](https://www.tei-c.org/release/doc/tei-p5-doc/en/html/ref-fs.html), [alt](https://www.tei-c.org/release/doc/tei-p5-doc/en/html/ref-alt.html), and [certainty](https://www.tei-c.org/release/doc/tei-p5-doc/en/html/ref-certainty.html).
