# Attribution and component licenses

## Original software and documentation

Copyright 2026 QuantForgeSoftware and contributors. MIT; see `LICENSE`. This does not replace the licenses on source text, morphological data, or the vendored TEI schema.

## SBL Greek New Testament text

SBL Greek New Testament (SBLGNT). Copyright 2010 by the Society of Biblical Literature and Logos Bible Software. The publisher distributes the text under [Creative Commons Attribution 4.0 International](https://creativecommons.org/licenses/by/4.0/); see the [publisher source repository and license](https://github.com/Faithlife/SBLGNT).

This repository uses the 1 Timothy text exactly as present in the specified QuantForgeSoftware/sblgnt revision, together with that fork's supplied apparatus signs. No replacement with another edition's readings is made. Generated formats add IDs and annotations and separate punctuation/apparatus signs in TEI. Those transformations are project changes; the source text file itself is unchanged.

The preserved upstream README links the former SBLGNT EULA. The publisher's current CC BY 4.0 notice is recorded here rather than altering that upstream README. The exact source and SHA-256 are in `data/source/manifest.json`.

## MorphGNT morphology and lemmatization

Tauber, J. K., ed. (2017). *MorphGNT: SBLGNT Edition*. Version 6.12. [DOI: 10.5281/zenodo.376200](https://doi.org/10.5281/zenodo.376200). Upstream: [morphgnt/sblgnt](https://github.com/morphgnt/sblgnt). Selected fork: [QuantForgeSoftware/sblgnt](https://github.com/QuantForgeSoftware/sblgnt), revision `aaed91e57c8e4a8dc9a2383e129ca5e75fe6393d`.

The upstream project licenses morphological parsing and lemmatization under [CC BY-SA 3.0 Unported](https://creativecommons.org/licenses/by-sa/3.0/). These fields are reproduced without relabeling. Formatting adaptations, the original annotation overlay, generated corpus graphs, review queue, and example payloads are distributed under CC BY-SA 3.0, with the SBLGNT text attribution retained. Code and original prose documentation remain MIT.

The initial interpretive overlay is an AI-assisted research draft, dated 2026-11-20. It is not an endorsed or specialist-reviewed MorphGNT, SBL, or Levinsohn annotation dataset. Named scholarly references identify evidence for specific proposals; their books/PDFs are linked, not redistributed.

## TEI schema

`schemas/tei_all-4.12.0.rng` is an unmodified schema from TEI P5 4.12.0, released 2026-07-28. Source: [TEI release archive](https://www.tei-c.org/Vault/P5/4.12.0/xml/tei/custom/schema/relaxng/tei_all.rng). Copyright TEI Consortium. TEI materials are offered under CC BY 3.0 and BSD 2-Clause; this software distribution uses the BSD 2-Clause option. See `LICENSES/TEI-BSD-2-Clause.txt` and [TEI licensing](https://tei-c.org/guidelines/licensing-and-citation/).

## License texts

`LICENSES/` retains the relevant Creative Commons license texts and TEI BSD notice. Redistribution must preserve the applicable notices and attribution. No scholarly organization named here endorses this project.
