# Oracle RimWorld Mods Docs Site

This folder contains the static documentation site for MagicFramework, MFVanilla, and related authoring tools.

## SpellForge Copy

`docs/spell-def-builder` is the publishable docs-site copy of the workspace-level `../SpellDefBuilder` tool.

Use the sync script from the `Mods` folder after editing the source builder:

```powershell
./sync-docs.ps1
```

Use check mode before publishing:

```powershell
./sync-docs.ps1 -Check
```

The source builder and docs copy should stay equivalent for `index.html`, `style.css`, and `script.js`. The sync script rewrites the root builder's `../Mods/docs/spell-design-guide/index.html` links to the docs-site relative `../spell-design-guide/index.html` path during publish sync.
