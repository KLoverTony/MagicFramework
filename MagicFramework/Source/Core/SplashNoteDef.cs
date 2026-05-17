using System.Collections.Generic;
using Verse;

namespace MagicFramework.Core;

public sealed class SplashNoteDef : Def
{
    public string modName;
    public string packageId;
    public string versionKey;
    public int order;
    public List<string> notes;

    public string DisplayModName => string.IsNullOrWhiteSpace(modName) ? label : modName;
    public string SeenKey => string.IsNullOrWhiteSpace(versionKey) ? defName : versionKey;
}
