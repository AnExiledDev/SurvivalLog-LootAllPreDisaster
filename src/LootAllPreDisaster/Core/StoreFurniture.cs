using System.Collections.Generic;

namespace LootAllPreDisaster.Core;

public readonly record struct Spot(float X, float Y, float Z)
{
    public float DistanceSquaredTo(Spot other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        var dz = Z - other.Z;

        return dx * dx + dy * dy + dz * dz;
    }
}

/// <summary>What one furniture button does, as far as looting cares.</summary>
public abstract record FurnitureFunc(int FuncId, IReadOnlyList<int> ActionIds)
{
    /// <summary>A timed search that opens the furniture's bag when it ends.</summary>
    public sealed record Search(int FuncId, IReadOnlyList<int> ActionIds) : FurnitureFunc(FuncId, ActionIds);

    /// <summary>An interaction that gives a reward a set number of times a day.</summary>
    public sealed record Reward(int FuncId, IReadOnlyList<int> ActionIds, int DailyMax, int UsedToday) : FurnitureFunc(FuncId, ActionIds)
    {
        public bool IsUsedUp => UsedToday >= DailyMax;
    }
}

/// <summary>
/// One piece of furniture in the store, read from the game. Only the buttons that matter to looting are listed; shop
/// shelves have none of them and so are never touched.
/// </summary>
public sealed record StoreFurniture(long Id, int WalkActionId, Spot Position, bool HasSearched, int ItemCount, IReadOnlyList<FurnitureFunc> Funcs);
