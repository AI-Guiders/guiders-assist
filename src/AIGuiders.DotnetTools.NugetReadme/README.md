# AIGuiders.DotnetTools.NugetReadme (`nuget-readme`)

Generates per-package NuGet readme pages (`NUGET.md`) from fact files: one fact per package, a fixed six-section template, EOL from the repo's own git attribute policy.

## What you get

- one fact file per package → one `NUGET.md` page next to its project file
- fixed template enforced on every regenerate: `What you get` / `Quick start` / `Problem` / `Solution` / `When not to use` / `Learn more`
- EOL decided by git itself — `git check-attr` per output file (nested `.gitattributes`, `info/attributes`, global chain all respected)
- status + coverage report: `generated`/`unchanged` per package, packages without facts listed (they keep the repo README)
- `--check` mode for CI: validate and report without writing

## Quick start

```bash
dotnet tool install --global AIGuiders.DotnetTools.NugetReadme
```

Run from the target repo root (or pass the repo path):

```bash
nuget-readme                  # regenerate NUGET.md for every fact
nuget-readme --check          # validate only, no writes
nuget-readme --facts my/docs  # custom facts dir (default docs/nuget/packages)
```

Fact file — `docs/nuget/packages/<PackageId>.md` by default:

```markdown
package: Your.Package.Id
tagline: One sentence: what this is.
project: libs/Demo/MyLib.vbproj        # optional, for layouts other than src/<id>/<id>.fsproj

## What you get
...

## Quick start / ## Problem / ## Solution / ## When not to use / ## Learn more
```

`project:` is the explicit path to the project file (repo-relative) — for repo layouts that don't match the `src/<PackageId>/` convention; `.fsproj` / `.csproj` / `.vbproj` all count as projects.

## When not to use

- Your package pages are hand-crafted one-offs and you want them that way — this tool enforces one fixed section template on everything it generates.
- You want generated files out of git — this tool writes `NUGET.md` next to each project for the pack to pick up (like generated docs committed in the Guiders repos).

## Learn more

- The convention this tool serves: [guiders-fsharp docs/nuget README](https://github.com/AI-Guiders/guiders-fsharp/blob/develop/docs/nuget/README.md)
- Federation pattern catalog: [GUIDERS-ADR-0049](https://github.com/AI-Guiders/guiders-dotnet-platform/blob/main/docs/adr/GUIDERS-ADR-0049-federation-pattern-library.md)

---

MIT · [Source & issues](https://github.com/AI-Guiders/guiders-assist) · part of the Guiders federation
