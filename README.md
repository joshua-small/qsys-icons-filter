# Icons Filter

Adds text and alias filtering to the native icon picker. This passive mod operates inside its native editor; it has no empty operation window or preferences page.

Requires [Q-SYS Mods Core](https://github.com/joshua-small/qsys-mods-core) 0.1.x, Q-SYS Mod Manager 0.8+, and the exact supported Designer 10.5 build. Use the manager migration to avoid duplicate hooks. Better Shortcuts is optional.

Clone Core beside this repository. Build Designer.Modules.IconsFilter.csproj with .NET 10.0.112 and local DesignerDir references. Vendor assemblies remain local. The feature is extracted from Schematic Tools commit 13c52e6 with shared host access routed through Core; the split passes compilation and isolated-host startup. No new hardware support is claimed.

[Split tracking](https://github.com/joshua-small/qsys-schematic-tools/issues/4).

## Portable behavior tests

With the .NET SDK selected by `global.json`, run from the repository root:

```sh
dotnet run --project tests/IconsFilter.Tests/IconsFilter.Tests.csproj --configuration Release
```

The dependency-free console suite compiles the production `IconQuery.cs`
directly and exits nonzero on any failed assertion. It requires no Core checkout,
Designer installation, WPF, vendor assemblies, external package feeds or GUI. The portable
workflow runs this same command on Ubuntu for pushes and pull requests.

Coverage includes case/punctuation/whitespace normalization, Unicode and long
input boundaries, all-term substring matching, native-name and alias queries,
state-specific alias exclusions, exact alias-key lookup, culture independence,
invalid-argument rejection, and repeated queries against synthetic owned items.
Synthetic item checks retain original identities, order, display names and glyph
strings; they do not exercise the native collection or selection event handlers.
Aliases expand search vocabulary only. The production source is unchanged.

Physical input, keyboard confirmation/cancellation, Custom image import,
native collection/filter/event preservation, undo, popup layout and save/reopen
remain separate [application acceptance work](https://github.com/joshua-small/qsys-icons-filter/issues/2).
Passing this suite does not establish Designer, Viewer or hardware acceptance.
