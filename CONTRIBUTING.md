# Contributing

The source text and interpretive annotations have different maintenance paths.

For annotation changes, edit `data/annotations/1tim.annotations.gntc`. Give word IDs and verse references, a proposed operation/relation/domain, and evidence for the selected scope. Keep incompatible analyses as alternatives. Record `by`, `status`, and supporting sources. Only identify an analysis as reviewed when an identifiable reviewer has actually reviewed it. Do not rewrite source morphology to make it agree with a discourse interpretation.

Regenerate artifacts with `dotnet run --project src/Gntc.Cli -- build .`. Run `dotnet test Gntc.sln` and the schema checks documented in `schemas/README.md`. Commit both the editable overlay and generated artifacts. CI checks deterministic regeneration. An annotation proposal can be linguistically mistaken even when all software tests pass.

For source changes, update the source manifest deliberately, preserve the previous revision in Git, and migrate annotations with a reviewed alignment. A new source checksum must not be substituted merely to make an old overlay pass. Preserve upstream attribution and licenses.

For code changes, add tests for behavior that could lose information, change identity, alter scope, or accept malformed data. Document format/vocabulary changes and migrations in the changelog. Avoid treating a version-1 reader's acceptance of a string attribute as support for new semantics.

Contributions to code/documentation use the repository MIT license. Corpus/data contributions use the applicable CC BY-SA 3.0 terms; retain the source text's separate attribution. Do not add a third-party dataset without its source revision and redistribution terms.
