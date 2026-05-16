# csdevlib/ddd Origin

## Source

- Repository: https://github.com/csdevlib/ddd
- Downloaded commit: `95b4a424ae20c7887b6760962d82bff4a26882dd`
- Download date: 2026-05-15
- Local path: `src/libs/shell/ddd`
- License: MIT, preserved in `LICENSE`

## Integration Status

This library is vendored as an isolated local shell library.

Current status:

- References `Ums.Shell.Factory` only inside the isolated shell library area.
- Not referenced by `Ums.Domain`.
- Not referenced by `Ums.Application`.
- Not referenced by `Ums.Infrastructure`.
- Not registered in the UMS build graph.
- Renamed to the local UMS shell convention (`Ums.Shell.Ddd*`).

## Technical Notes

The downloaded source contains .NET projects targeting `net9.0`:

- `src/Ums.Shell.Ddd/Ums.Shell.Ddd.csproj`
- `src/Ums.Shell.Ddd.ValueObjects/Ums.Shell.Ddd.ValueObjects.csproj`
- `src/Ums.Shell.Ddd.AutoMapper/Ums.Shell.Ddd.AutoMapper.csproj`
- `src/Ums.Shell.Ddd.Test/Ums.Shell.Ddd.Test.csproj`

The UMS API currently follows a .NET 8 architecture. Before referencing this library from UMS projects, perform a compatibility and architecture review:

- Verify target framework alignment (`net9.0` vs UMS .NET target).
- Review dependency policy for `MediatR`, `AutoMapper`, and `Ums.Shell.Factory`.
- Check whether `Ums.Domain` may depend on external packages under the current DDD rules.
- Decide whether to consume the library directly, fork/adapt primitives, or keep it as reference material.

## Governance Rule

Do not modify UMS domain projects to reference this library without an explicit ADR or implementation task. This copy is intentionally isolated to preserve architectural safety while the DDD primitive strategy is reviewed.
