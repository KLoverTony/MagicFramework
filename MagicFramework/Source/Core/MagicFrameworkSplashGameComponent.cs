using Verse;

namespace MagicFramework.Core;

public sealed class MagicFrameworkSplashGameComponent : GameComponent
{
    private int ticksUntilCheck = 120;
    private bool checkedThisGame;

    public MagicFrameworkSplashGameComponent(Game game)
    {
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();
        if (checkedThisGame)
        {
            return;
        }

        ticksUntilCheck--;
        if (ticksUntilCheck > 0)
        {
            return;
        }

        checkedThisGame = true;
        MagicFrameworkSplashUtility.ShowIfNew();
    }
}
