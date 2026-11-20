# Validation resources

- `gntc.schema.json`: JSON Schema Draft 2020-12 for the canonical graph shape and record arities.
- `tei_all-4.12.0.rng`: unmodified official TEI P5 4.12.0 Relax NG schema, obtained from the TEI version archive; see `NOTICE.md` for attribution/license.
- `gntc.sch`: supplemental Schematron using XPath 2.0. It documents key structural constraints; CI does not currently compile/run this file.

Run:

```sh
python -m pip install -r scripts/requirements.txt
python scripts/validate_json.py
xmllint --noout --relaxng schemas/tei_all-4.12.0.rng data/1tim.tei.xml examples/1tim-4.7-8.tei.xml
dotnet run --project src/Gntc.Cli -- validate data/1tim.tei.xml --source data/source/75-1Ti-morphgnt.txt
```

The C# validator adds reference resolution, actual span membership, source identity, vocabulary, provenance/status, and alternative-group checks beyond JSON Schema. The strict TEI reader checks the complete supported XML profile, including agreement between structural targets and named features. Relax NG alone is not that profile check.

`xmllint` is supplied by libxml2 (available on macOS; `libxml2-utils` on Debian/Ubuntu). Core C# converters and tests do not require Python or libxml2.
