# Icons Filter

Adds text and alias filtering to the native icon picker. This passive mod operates inside its native editor; it has no empty operation window or preferences page.

Requires [Q-SYS Mods Core](https://github.com/joshua-small/qsys-mods-core) 0.1.x, Q-SYS Mod Manager 0.8+, and the exact supported Designer 10.5 build. Use the manager migration to avoid duplicate hooks. Better Shortcuts is optional.

Clone Core beside this repository. Build Designer.Modules.IconsFilter.csproj with .NET 10.0.112 and local DesignerDir references. Vendor assemblies remain local. The feature is extracted from Schematic Tools commit 13c52e6 with shared host access routed through Core; the split passes compilation and isolated-host startup. No new hardware support is claimed.

[Split tracking](https://github.com/joshua-small/qsys-schematic-tools/issues/4).
