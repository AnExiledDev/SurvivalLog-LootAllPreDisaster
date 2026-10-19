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
/// The run a press started, until it ends: which actions belong to it (for the instant clock) and which containers it
/// still has to empty. A container is emptied once its search has opened it; one that does not fit stops the run.
/// </summary>
public sealed class LootRun
{
    public static readonly LootRun None = new();

    private readonly HashSet<long> _targets;
    private readonly HashSet<int> _actions;
    private readonly List<long> _toTake;

    public LootRun(RunPlan plan, LootSettings settings)
    {
        Settings = settings;
        _targets = plan.Queue.Select(action => action.FurnitureId).ToHashSet();
        _actions = plan.Queue.Select(action => action.ActionId).ToHashSet();
        _toTake = settings.TakesContents ? plan.Leftovers.Concat(plan.Searches).Distinct().ToList() : new List<long>();
        IsActive = true;
    }

    private LootRun()
    {
        Settings = new LootSettings(LootMode.WalkAndOpen, ActionTime.Runs);
        _targets = new HashSet<long>();
        _actions = new HashSet<int>();
        _toTake = new List<long>();
    }

    public LootSettings Settings { get; }

    public bool IsActive { get; private set; }

    public IReadOnlyCollection<long> Targets => _targets;

    /// <summary>The character's current action is one this run lined up.</summary>
    public bool Covers(int actionId, long targetId) => IsActive && _actions.Contains(actionId) && _targets.Contains(targetId);

    /// <summary>The containers to empty now: those the run takes from that have been opened.</summary>
    public IReadOnlyList<long> DueForTake(Func<long, bool> isSearched) =>
        IsActive ? _toTake.Where(isSearched).ToList() : Array.Empty<long>();

    public TakeOutcome Took(long containerId, int itemsLeft)
    {
        _toTake.Remove(containerId);

        if (itemsLeft <= 0) return TakeOutcome.Emptied;

        End();

        return TakeOutcome.StorageFull;
    }

    /// <summary>
    /// Over once the character has none of the run's targets lined up and nothing opened is waiting to be emptied. A
    /// container whose search the player broke off is never opened, so it does not hold the run open.
    /// </summary>
    public bool IsOver(Func<long, bool> isTargetQueued, Func<long, bool> isSearched) =>
        !IsActive || (!_targets.Any(isTargetQueued) && DueForTake(isSearched).Count == 0);

    public void End() => IsActive = false;
}
