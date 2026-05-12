using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.PawnMemory;

public class SkillSnapshot : IExposable
{
    public SkillDef skillDef;
    public int level;
    public Passion passion;
    public float xpSinceLastLevel;

    public void ExposeData()
    {
        Scribe_Defs.Look(ref skillDef, "skillDef");
        Scribe_Values.Look(ref level, "level");
        Scribe_Values.Look(ref passion, "passion");
        Scribe_Values.Look(ref xpSinceLastLevel, "xpSinceLastLevel");
    }
}

public class PawnMemoryRecord : IExposable
{
    // Identity
    public string uniquePawnId;
    public Name name;
    public Gender gender;
    public ThingDef raceDef;
    public PawnKindDef pawnKindDef;
    public FactionDef factionDef;
    public bool wasColonist;
    public bool wasPrisoner;
    public bool wasSlave;
    public bool wasGuest;
    public bool wasHostileToPlayer;

    // Map and Location Context
    public int? lastKnownMapId;
    public string lastKnownMapName;
    public IntVec3? lastKnownCell;
    public int? deathMapId;
    public IntVec3? deathCell;
    public int lastSeenTick;
    public int createdTick;
    public int lastUpdatedTick;
    public int? deathTick;

    // Appearance
    public BodyTypeDef bodyType;
    public HeadTypeDef headType;
    public HairDef hairDef;
    public BeardDef beardDef;
    public Color skinColor;
    public Color hairColor;

    // Backgrounds
    public string childhoodBackstory;
    public string adulthoodBackstory;

    // Traits (DefName, Degree)
    public Dictionary<string, int> traits = new Dictionary<string, int>();

    // Skills
    public List<SkillSnapshot> skills = new List<SkillSnapshot>();

    // Ideology
    public Ideo ideo;

    // Health Snapshot
    public bool deadAtCapture;
    public List<string> majorHediffs = new List<string>();
    public List<string> missingBodyParts = new List<string>();

    // Imprint Lifecycle State
    public PawnMemoryState state = PawnMemoryState.Active;
    public bool resurrectionAllowed = true;
    public bool spiritActive;
    public string activeSpiritThingId;
    public bool properRitesPerformed;
    public bool standardBurialObserved;
    public bool rituallyReleased;
    public bool corrupted;
    public bool bodyDestroyed;
    public bool corpseAnchorKnown;
    public string invalidationReason;

    public void ExposeData()
    {
        Scribe_Values.Look(ref uniquePawnId, "uniquePawnId");
        Scribe_Deep.Look(ref name, "name");
        Scribe_Values.Look(ref gender, "gender");
        Scribe_Defs.Look(ref raceDef, "raceDef");
        Scribe_Defs.Look(ref pawnKindDef, "pawnKindDef");
        Scribe_Defs.Look(ref factionDef, "factionDef");
        
        Scribe_Values.Look(ref wasColonist, "wasColonist");
        Scribe_Values.Look(ref wasPrisoner, "wasPrisoner");
        Scribe_Values.Look(ref wasSlave, "wasSlave");
        Scribe_Values.Look(ref wasGuest, "wasGuest");
        Scribe_Values.Look(ref wasHostileToPlayer, "wasHostileToPlayer");

        Scribe_Values.Look(ref lastKnownMapId, "lastKnownMapId");
        Scribe_Values.Look(ref lastKnownMapName, "lastKnownMapName");
        Scribe_Values.Look(ref lastKnownCell, "lastKnownCell");
        Scribe_Values.Look(ref deathMapId, "deathMapId");
        Scribe_Values.Look(ref deathCell, "deathCell");
        Scribe_Values.Look(ref lastSeenTick, "lastSeenTick");
        Scribe_Values.Look(ref createdTick, "createdTick");
        Scribe_Values.Look(ref lastUpdatedTick, "lastUpdatedTick");
        Scribe_Values.Look(ref deathTick, "deathTick");

        Scribe_Defs.Look(ref bodyType, "bodyType");
        Scribe_Defs.Look(ref headType, "headType");
        Scribe_Defs.Look(ref hairDef, "hairDef");
        Scribe_Defs.Look(ref beardDef, "beardDef");
        Scribe_Values.Look(ref skinColor, "skinColor");
        Scribe_Values.Look(ref hairColor, "hairColor");

        Scribe_Values.Look(ref childhoodBackstory, "childhoodBackstory");
        Scribe_Values.Look(ref adulthoodBackstory, "adulthoodBackstory");

        Scribe_Collections.Look(ref traits, "traits", LookMode.Value, LookMode.Value);
        Scribe_Collections.Look(ref skills, "skills", LookMode.Deep);
        
        Scribe_References.Look(ref ideo, "ideo");

        Scribe_Values.Look(ref deadAtCapture, "deadAtCapture");
        Scribe_Collections.Look(ref majorHediffs, "majorHediffs", LookMode.Value);
        Scribe_Collections.Look(ref missingBodyParts, "missingBodyParts", LookMode.Value);

        Scribe_Values.Look(ref state, "state", PawnMemoryState.Active);
        Scribe_Values.Look(ref resurrectionAllowed, "resurrectionAllowed", true);
        Scribe_Values.Look(ref spiritActive, "spiritActive");
        Scribe_Values.Look(ref activeSpiritThingId, "activeSpiritThingId");
        Scribe_Values.Look(ref properRitesPerformed, "properRitesPerformed");
        Scribe_Values.Look(ref standardBurialObserved, "standardBurialObserved");
        Scribe_Values.Look(ref rituallyReleased, "rituallyReleased");
        Scribe_Values.Look(ref corrupted, "corrupted");
        Scribe_Values.Look(ref bodyDestroyed, "bodyDestroyed");
        Scribe_Values.Look(ref corpseAnchorKnown, "corpseAnchorKnown");
        Scribe_Values.Look(ref invalidationReason, "invalidationReason");

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            traits ??= new Dictionary<string, int>();
            skills ??= new List<SkillSnapshot>();
            majorHediffs ??= new List<string>();
            missingBodyParts ??= new List<string>();
        }
    }
}
