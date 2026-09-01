# deck — federation `.deck` emit

```bash
dotnet tool install AIGuiders.DotnetTools.DeckEmit
deck emit dashspec-studio.deck --namespace DashSpec.Studio.Generated --output Generated/DeckIds.g.cs
```

Mirrors `authoring emit` for catalog; writes `DeckIds.g.cs` from parsed deck IR.
