using System;

namespace LootAllPreDisaster.Core;

/// <summary>
/// The time step an instant action's effect takes in one frame. The game's payout timers pay at most once per frame,
/// so a step is never longer than the shortest payout period; within that it runs to the effect's end, so a short
/// action ends on the frame it starts and still gives everything it gives.
/// </summary>
public static class InstantClock
{
    /// <summary>The shortest time between two payouts in any action's effect, in game seconds.</summary>
    public const float ShortestPayoutGameSeconds = 360f;

    /// <summary>The game counts effect time in game seconds divided by the chapter's time scale.</summary>
    public static float LongestStep(int timeScale) => timeScale > 0 ? ShortestPayoutGameSeconds / timeScale : float.PositiveInfinity;

    /// <summary>Only an effect that counts down a length can end sooner; one that lasts until something ends it keeps the game's step.</summary>
    public static float Step(float gameStep, float fullLife, float lifeLeft, float longestStep)
    {
        if (fullLife <= 0f || lifeLeft <= 0f) return gameStep;

        var limit = longestStep > 0f && !float.IsNaN(longestStep) ? longestStep : float.PositiveInfinity;

        return Math.Max(gameStep, Math.Min(lifeLeft, limit));
    }
}
