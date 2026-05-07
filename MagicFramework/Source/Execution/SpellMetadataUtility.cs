using System.Collections.Generic;
using MagicFramework.Definitions;

namespace MagicFramework.Execution;

public static class SpellMetadataUtility
{
    public static bool HasElement(this SpellDef spell, SpellElementDef element)
    {
        return ContainsDef(spell?.meta?.elements, element);
    }

    public static bool HasDomain(this SpellDef spell, SpellDomainDef domain)
    {
        return ContainsDef(spell?.meta?.domains, domain);
    }

    public static bool HasDiscipline(this SpellDef spell, SpellDisciplineDef discipline)
    {
        return ContainsDef(spell?.meta?.disciplines, discipline);
    }

    public static bool HasTag(this SpellDef spell, SpellTagDef tag)
    {
        return ContainsDef(spell?.meta?.tags, tag);
    }

    public static bool HasElement(this SpellDef spell, string elementDefName)
    {
        return ContainsDefName(spell?.meta?.elements, elementDefName);
    }

    public static bool HasDomain(this SpellDef spell, string domainDefName)
    {
        return ContainsDefName(spell?.meta?.domains, domainDefName);
    }

    public static bool HasDiscipline(this SpellDef spell, string disciplineDefName)
    {
        return ContainsDefName(spell?.meta?.disciplines, disciplineDefName);
    }

    public static bool HasTag(this SpellDef spell, string tagDefName)
    {
        return ContainsDefName(spell?.meta?.tags, tagDefName);
    }

    private static bool ContainsDef<TDef>(List<TDef> defs, TDef target)
        where TDef : Verse.Def
    {
        return target != null && defs != null && defs.Contains(target);
    }

    private static bool ContainsDefName<TDef>(List<TDef> defs, string defName)
        where TDef : Verse.Def
    {
        if (defs == null || string.IsNullOrWhiteSpace(defName))
        {
            return false;
        }

        for (int i = 0; i < defs.Count; i++)
        {
            if (defs[i]?.defName == defName)
            {
                return true;
            }
        }

        return false;
    }
}
