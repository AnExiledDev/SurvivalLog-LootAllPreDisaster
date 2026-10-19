namespace LootAllPreDisaster.Core;

/// <summary>An order the game is about to start for some character.</summary>
public readonly record struct NewOrder(long OwnerId, bool IsFromPlayer);

public static class Interruption
{
    /// <summary>
    /// An order the player gives their own character during a run ends the run and replaces what is left of it, even
    /// where another mod makes new orders join the end of the queue. The run's own start, orders the game gives by
    /// itself and other characters' orders leave it alone.
    /// </summary>
    public static bool EndsRun(NewOrder order, bool isRunActive, long roleId, bool isRunStarting) =>
        isRunActive && !isRunStarting && order.IsFromPlayer && order.OwnerId == roleId;
}
