# MagicFramework Source

This folder contains the SDK-style C# project for the Magic Framework RimWorld 1.6 mod.

## Current Status

- Core spell execution, costs, requirements, target selection, conditions, delayed actions, projectile callbacks, persistent markers/zones, summons, stat buffs, force fields, scaling, and procedural magic FX are implemented in source.
- Authored validation spell XML lives in `Mods/MFVanilla/Defs/SpellDefs`; MagicFramework keeps framework code, runtime state, marker defs, procedural FX defs, and debug fallback spells.
- The project uses Harmony for runtime patches and references RimWorld/Unity assemblies from the local Steam install.

## Author Documentation

- [Spell Design Guide](Documentation/SpellDesignGuide.md) is the MF-031 authoring guide for building MagicFramework spells.
- [Targeting Policy](Documentation/TargetingPolicy.md) and [Lifecycle Hooks](Documentation/LifecycleHooks.md) provide focused reference material for two high-risk authoring areas.

## Requirements

1. Install a .NET SDK with MSBuild support for `net472`.
2. Install the .NET Framework 4.7.2 Developer Pack if MSBuild reports missing reference assemblies.
3. Make sure `RimWorldDir` in `MagicFramework.csproj` points at your RimWorld install folder, or override it on the command line.
4. Keep Harmony available at the path referenced by the project file, or update the reference path for your install.

## Build

```powershell
dotnet build .\MagicFramework.csproj -p:RimWorldDir="C:\Path\To\RimWorld"
```

The compiled DLL is placed in `..\Assemblies\`. The solution-level build can also deploy the DLL into the active mod assemblies folder when `DeployToModAssemblies=true` is supplied.
