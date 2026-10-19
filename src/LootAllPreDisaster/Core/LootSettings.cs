namespace LootAllPreDisaster.Core;

/// <summary>How a press loots the store.</summary>
public enum LootMode
{
    /// <summary>Walk to each target; each container's bag window opens and the player takes what they want.</summary>
    WalkAndOpen = 1,

    /// <summary>Walk to each target; each container is emptied into the trunk, then the bag, with no window.</summary>
    WalkAndTake = 2,

    /// <summary>No walking: every action runs where the player stands and each container is emptied with no window.</summary>
    TakeInPlace = 3,
}

/// <summary>How long the run's actions take.</summary>
public enum ActionTime
{
    /// <summary>Every action takes its normal game time.</summary>
    Runs,

    /// <summary>Every action of the run ends at once.</summary>
    Instant,
}

public sealed record LootSettings(LootMode Mode, ActionTime Time)
{
    /// <summary>A number outside 1 to 3 falls back to the default, mode 1.</summary>
    public static LootMode ModeFrom(int value) => value switch
    {
        2 => LootMode.WalkAndTake,
        3 => LootMode.TakeInPlace,
        _ => LootMode.WalkAndOpen,
    };

    /// <summary>Anything other than "Instant", in any case, falls back to the default, Runs.</summary>
    public static ActionTime TimeFrom(string? value) =>
        string.Equals(value?.Trim(), nameof(ActionTime.Instant), System.StringComparison.OrdinalIgnoreCase) ? ActionTime.Instant : ActionTime.Runs;

    /// <summary>Only modes 2 and 3 empty containers for the player.</summary>
    public bool TakesContents => Mode != LootMode.WalkAndOpen;

    public bool Walks => Mode != LootMode.TakeInPlace;
}
