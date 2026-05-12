using System;

namespace MagicFramework.PawnMemory;

public enum PawnMemoryState
{
    Active,
    Dormant,
    DeadPendingRites,
    Released,
    SpiritActive,
    Bound,
    Corrupted,
    ResurrectionConsumed,
    Invalidated
}

public enum PawnMemoryUpdateReason
{
    EnteredMap,
    DailyMaintenance,
    BeforeDeath,
    OnDeath,
    ManualDebug,
    Ritual,
    Resurrection
}

public enum RiteOutcome
{
    ProperRites,
    ReleaseSpirit,
    BindSpirit,
    PurifyCorruption,
    CorruptImprint,
    PreserveForResurrection
}

public enum SpiritDepartureReason
{
    Resurrected,
    Banished,
    Released,
    TimeExpired,
    Other
}

public enum CorpseDispositionReason
{
    Butchered,
    Cremated,
    Destroyed,
    Rotted,
    Other
}
