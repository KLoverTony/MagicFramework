using System.Collections.Generic;
using System.Linq;
using MagicFramework.Definitions;

namespace MagicFramework.Scheduling;

/// <summary>
/// Locates authored spell actions by walking the nested spell graph.
/// </summary>
public static class SpellActionPathUtility
{
    public static bool TryCreatePath(SpellDef spellDef, SpellActionDef targetAction, out List<int> path)
    {
        path = new List<int>();
        return spellDef != null
            && targetAction != null
            && TryCreatePath(spellDef.actions, targetAction, path);
    }

    public static SpellActionDef ResolveAction(SpellDef spellDef, IReadOnlyList<int> path)
    {
        if (spellDef?.actions == null || path == null || path.Count == 0)
        {
            return null;
        }

        SpellActionDef currentAction = null;
        IReadOnlyList<SpellActionDef> currentLevel = spellDef.actions;
        for (int i = 0; i < path.Count; i++)
        {
            int index = path[i];
            if (currentLevel == null || index < 0 || index >= currentLevel.Count)
            {
                return null;
            }

            currentAction = currentLevel[index];
            if (currentAction == null)
            {
                return null;
            }

            if (i < path.Count - 1)
            {
                currentLevel = currentAction.GetChildActions()?.ToList();
            }
        }

        return currentAction;
    }

    private static bool TryCreatePath(
        IReadOnlyList<SpellActionDef> actions,
        SpellActionDef targetAction,
        List<int> path)
    {
        if (actions == null)
        {
            return false;
        }

        for (int i = 0; i < actions.Count; i++)
        {
            SpellActionDef action = actions[i];
            if (action == null)
            {
                continue;
            }

            path.Add(i);
            if (ReferenceEquals(action, targetAction))
            {
                return true;
            }

            List<SpellActionDef> childActions = action.GetChildActions()?.ToList();
            if (childActions != null && TryCreatePath(childActions, targetAction, path))
            {
                return true;
            }

            path.RemoveAt(path.Count - 1);
        }

        return false;
    }
}
