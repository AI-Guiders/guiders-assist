# aig-toml-check

Validate TOML files against [JSON Schema](https://json-schema.org).

## Usage

```bash
aig-toml-check check path/to/file.toml
aig-toml-check check docs/ --schema schemas/catalog.schema.json
```

Each file may declare its schema in the header (Taplo-style):

```toml
#:schema ../docs/schemas/example.schema.json
```

Paths in `#:schema` are resolved relative to the TOML file.

## Exit codes

- `0` — all files valid
- `1` — parse error, missing schema, or validation failure

## Install

```bash
dotnet tool install AIGuiders.DotnetTools.TomlCheck
```
