using RimWorld;
using UnityEngine;
using Verse;

namespace MagicFramework.Core;

public sealed class SpellManaGizmo : Gizmo
{
    private static readonly Color BackgroundColor = new(0.09f, 0.09f, 0.11f, 0.92f);
    private static readonly Color BarBackColor = new(0.03f, 0.03f, 0.05f, 0.95f);
    private static readonly Color ManaColor = new(0.22f, 0.55f, 1f, 0.95f);
    private static readonly Color ManaHighlightColor = new(0.7f, 0.9f, 1f, 0.95f);
    private static readonly Color ExperienceColor = new(0.72f, 0.48f, 0.95f, 0.95f);
    private static readonly Color ExperienceHighlightColor = new(0.92f, 0.78f, 1f, 0.95f);
    private const float Width = 140f;
    private const float GizmoHeight = 86f;
    private readonly Pawn pawn;

    public SpellManaGizmo(Pawn pawn)
    {
        this.pawn = pawn;
    }

    public override float GetWidth(float maxWidth)
    {
        return Width;
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        Rect rect = new(topLeft.x, topLeft.y, GetWidth(maxWidth), GizmoHeight);
        Widgets.DrawBoxSolid(rect, BackgroundColor);
        Widgets.DrawBox(rect);

        SpellRuntimeGameComponent runtime = SpellRuntimeGameComponent.Instance;
        float currentMana = Mathf.Max(0f, runtime?.GetCurrentMana(pawn) ?? 0f);
        float maxMana = Mathf.Max(1f, runtime?.GetMaxMana(pawn) ?? 1f);
        int casterLevel = runtime?.GetCasterLevel(pawn) ?? 0;
        float manaFillPercent = Mathf.Clamp01(currentMana / maxMana);
        float experienceFillPercent = runtime?.GetCasterExperienceProgress(pawn) ?? 0f;
        float experienceToNextLevel = runtime?.GetCasterExperienceToNextLevel(pawn) ?? 0f;

        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        Widgets.Label(new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 24f), "Mana");

        Text.Anchor = TextAnchor.UpperRight;
        Widgets.Label(
            new Rect(rect.x + 8f, rect.y + 6f, rect.width - 16f, 24f),
            $"{currentMana:0.#}/{maxMana:0.#}");

        Rect barRect = new(rect.x + 8f, rect.y + 34f, rect.width - 16f, 18f);
        Widgets.DrawBoxSolid(barRect, BarBackColor);
        Rect fillRect = new(barRect.x, barRect.y, barRect.width * manaFillPercent, barRect.height);
        Widgets.DrawBoxSolid(fillRect, ManaColor);
        Rect highlightRect = new(fillRect.x, fillRect.y, fillRect.width, 3f);
        Widgets.DrawBoxSolid(highlightRect, ManaHighlightColor);
        Widgets.DrawBox(barRect);

        Rect experienceBarRect = new(rect.x + 8f, rect.y + 55f, rect.width - 16f, 6f);
        Widgets.DrawBoxSolid(experienceBarRect, BarBackColor);
        Rect experienceFillRect = new(experienceBarRect.x, experienceBarRect.y, experienceBarRect.width * experienceFillPercent, experienceBarRect.height);
        Widgets.DrawBoxSolid(experienceFillRect, ExperienceColor);
        Rect experienceHighlightRect = new(experienceFillRect.x, experienceFillRect.y, experienceFillRect.width, 2f);
        Widgets.DrawBoxSolid(experienceHighlightRect, ExperienceHighlightColor);
        Widgets.DrawBox(experienceBarRect);

        Text.Anchor = TextAnchor.MiddleCenter;
        Widgets.Label(new Rect(rect.x + 8f, rect.y + 64f, rect.width - 16f, 18f), $"Lv. {casterLevel} - {manaFillPercent.ToStringPercent("F0")}");
        Text.Anchor = TextAnchor.UpperLeft;

        string experienceTooltip = casterLevel >= 20
            ? "Caster XP: max level"
            : $"Caster XP: {experienceFillPercent.ToStringPercent("F0")} ({experienceToNextLevel:0.#} to next level)";
        TooltipHandler.TipRegion(rect, $"{pawn?.LabelShortCap ?? "Pawn"} caster level: {casterLevel}\nMana: {currentMana:0.#} / {maxMana:0.#}\n{experienceTooltip}");
        return new GizmoResult(GizmoState.Clear);
    }
}
