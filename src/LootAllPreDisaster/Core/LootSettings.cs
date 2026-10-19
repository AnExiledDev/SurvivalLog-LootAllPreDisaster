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

public sealed record LootSettings(LootMode Mode)
{
    /// <summary>A number outside 1 to 3 falls back to the default, mode 1.</summary>
    public static LootMode ModeFrom(int value) => value switch
    {
        2 => LootMode.WalkAndTake,
        3 => LootMode.TakeInPlace,
        _ => LootMode.WalkAndOpen,
    };

    /// <summary>Only modes 2 and 3 empty containers for the player.</summary>
    public bool TakesContents => Mode != LootMode.WalkAndOpen;

    public bool Walks => Mode != LootMode.TakeInPlace;
}
