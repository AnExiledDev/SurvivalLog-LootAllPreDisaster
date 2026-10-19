using System;
using System.Collections.Generic;
using System.Linq;

namespace LootAllPreDisaster.Core;

public enum TakeOutcome
{
    /// <summary>Everything fit; the run carries on.</summary>
    Emptied,

    /// <summary>Only part fit, so the run stops.</summary>
    StorageFull,
}

/// <summary>
/// The run a press started, until it ends: which furniture it lined up and which containers it still has to empty. A
/// container is emptied once its search has opened it; one that does not fit stops the run.
/// </summary>
public sealed class LootRun
{
    /// <summary>
    /// How long the character must have nothing of the run left before it counts as over. A search can leave the
    /// character's line a moment before it opens its container, so ending at the first idle check could leave the last
    /// container unemptied, with its bag window open.
    /// </summary>
    public const float SettleSeconds = 1f;

    public static readonly LootRun None = new();

    private readonly HashSet<long> _targets;
    private readonly List<long> _toTake;
    private readonly HashSet<long> _takenFrom = new();
    private float? _idleSince;

    public LootRun(RunPlan plan, LootSettings settings)
    {
        _targets = plan.Queue.Select(action => action.FurnitureId).ToHashSet();
        _toTake = settings.TakesContents ? plan.Leftovers.Concat(plan.Searches).Distinct().ToList() : new List<long>();
        IsActive = true;
    }

    private LootRun()
    {
        _targets = new HashSet<long>();
        _toTake = new List<long>();
    }

    public bool IsActive { get; private set; }

    /// <summary>The containers to empty now: those the run takes from that have been opened.</summary>
    public IReadOnlyList<long> DueForTake(Func<long, bool> isSearched) =>
        IsActive ? _toTake.Where(isSearched).ToList() : Array.Empty<long>();

    /// <summary>Kept after the run ends, so a bag window that shows late for a container it took from still closes.</summary>
    public bool WasTakenFrom(long containerId) => _takenFrom.Contains(containerId);

    public TakeOutcome Took(long containerId, int itemsLeft)
    {
        _toTake.Remove(containerId);
        _takenFrom.Add(containerId);

        if (itemsLeft <= 0) return TakeOutcome.Emptied;

        End();

        return TakeOutcome.StorageFull;
    }

    /// <summary>
    /// Over once the character has had none of the run's targets lined up, and nothing opened waiting to be emptied,
    /// for <see cref="SettleSeconds"/>. A container whose search the player broke off is never opened, so it does not
    /// hold the run open past that.
    /// </summary>
    public bool IsOver(Func<long, bool> isTargetQueued, Func<long, bool> isSearched, float now)
    {
        if (!IsActive) return true;

        if (_targets.Any(isTargetQueued) || DueForTake(isSearched).Count > 0)
        {
            _idleSince = null;

            return false;
        }

        _idleSince ??= now;

        return now - _idleSince.Value >= SettleSeconds;
    }

    public void End() => IsActive = false;
}
