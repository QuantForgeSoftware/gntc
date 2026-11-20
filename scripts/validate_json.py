"""Independent JSON Schema validation; graph/source checks live in the C# validator."""
import json
from pathlib import Path

from jsonschema import Draft202012Validator

root = Path(__file__).resolve().parents[1]
schema = json.loads((root / "schemas/gntc.schema.json").read_text(encoding="utf-8"))
Draft202012Validator.check_schema(schema)
validator = Draft202012Validator(schema)
for relative in ["data/1tim.json", "examples/1tim-4.7-8.json"]:
    validator.validate(json.loads((root / relative).read_text(encoding="utf-8")))
    print(f"JSON Schema valid: {relative}")
