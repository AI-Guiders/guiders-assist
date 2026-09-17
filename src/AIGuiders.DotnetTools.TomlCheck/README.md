# aig-toml-check

Validate TOML files against native [TOML Schema](https://toml-schema.org/) (`.tosd`) or [JSON Schema](https://json-schema.org/).

Native validation uses the vendored .NET reference implementation from [brunoborges/toml-schema](https://github.com/brunoborges/toml-schema) (`AIGuiders.TomlSchema`).

## Usage

```bash
aig-toml-check check path/to/file.toml
aig-toml-check check docs/ --schema schemas/catalog.tosd
aig-toml-check check docs/ --schema schemas/catalog.schema.json
```

## Schema resolution

1. `--schema <file>` CLI override
2. `#:schema` directive in the first 32 lines (Taplo-style)
3. `[toml-schema].location` metadata in the document

Paths in `#:schema` and `location` resolve relative to the TOML file directory.

`.tosd` schemas use the native TOML Schema engine; `.json` schemas use Tomlyn + JsonSchema.Net.

## Examples

TOML Schema via directive:

```toml
#:schema config.tosd
name = "demo"
```

TOML Schema via document metadata:

```toml
[toml-schema]
version = "1.0.0"
location = "config.tosd"
```

JSON Schema (backward compatible):

```toml
#:schema catalog.schema.json
```

## Exit codes

- `0` — all files valid
- `1` — parse error, missing schema, or validation failure

## Install

```bash
dotnet tool install AIGuiders.DotnetTools.TomlCheck
```
