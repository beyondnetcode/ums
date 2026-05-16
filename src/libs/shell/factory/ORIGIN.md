# csdevlib/factory-pattern Origin

## Source

- Repository: https://github.com/csdevlib/factory-pattern
- Downloaded commit: `b8b9127de9eda8c5034c826bdb2f8be118fb375d`
- Download date: 2026-05-15
- Local path: `src/libs/shell/factory`
- License: MIT, preserved in `LICENSE`

## Local Shell Naming

The original `BeyondNet.Factory*` projects were vendored and renamed under the UMS shell convention:

- `BeyondNet.Factory` -> `Ums.Shell.Factory`
- `BeyondNet.Factory.Installer` -> `Ums.Shell.Factory.Installer`
- `BeyondNet.Factory.Test` -> `Ums.Shell.Factory.Test`
- `BeyondNet.Factory.Demo` -> `Ums.Shell.Factory.Demo`

Namespaces, project names, solution references, and project references were updated to match the `Ums.Shell.Factory` convention.

## Integration Status

This library is vendored as an isolated shared shell library.

Current status:

- Referenced only by the isolated `Ums.Shell.Ddd` shell library.
- Not referenced by `Ums.Domain`.
- Not referenced by `Ums.Application`.
- Not referenced by `Ums.Infrastructure`.
- Not registered in the UMS build graph.

## Governance Rule

Do not reference this shell from UMS production projects without an explicit architecture decision or implementation task. This copy exists to remove external package dependency from the isolated DDD shell and to allow local compatibility review.
