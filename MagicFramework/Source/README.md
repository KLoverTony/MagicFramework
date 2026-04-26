# MagicFramework Source Setup

This folder now contains a minimal SDK-style C# project for a RimWorld mod.

## Requirements

1. Install a .NET SDK. The machine currently has runtimes only, so `dotnet build` cannot work yet.
2. If you target `net472`, install the .NET Framework 4.7.2 Developer Pack if MSBuild reports missing reference assemblies.
3. Make sure `RimWorldDir` in `MagicFramework.csproj` points at your RimWorld install folder, or override it on the command line.

## Build

```powershell
dotnet build .\MagicFramework.csproj -p:RimWorldDir="C:\Path\To\RimWorld"
```

The compiled DLL will be placed in `..\Assemblies\`.
