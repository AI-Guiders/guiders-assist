# Vendored: TomlSchema (.NET)

Upstream: https://github.com/brunoborges/toml-schema  
Path: `reference-implementations/dotnet/TomlSchema`  
License: MIT (see LICENSE)  
Version: 1.0.0-rc.2 (upstream commit vendored 2026-09-17)

# Windows-specific vendored patches in SchemaDiscovery.cs:
# - reject opaque file: URIs before retrieval
# - Uri.UnescapeDataString for file URI paths (spaces)
