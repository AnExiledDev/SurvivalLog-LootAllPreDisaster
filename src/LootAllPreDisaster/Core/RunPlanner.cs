using System.Collections.Generic;
using System.Linq;

namespace LootAllPreDisaster.Core;

/// <summary>One action lined up for the character, aimed at one piece of furniture.</summary>
public readonly record struct QueuedAction(int ActionId, long FurnitureId);

/// <summary>
/// What one press does: the actions to line up, in walking order; the containers the run searches; and, in the modes
/// that take contents, the containers searched before that still hold something, emptied at once.
/// </summary>
public sealed record RunPlan(IReadOnlyList<QueuedAction> Queue, IReadOnlyList<long> Searches, int Rewards, IReadOnlyList<long> Leftovers)
{
    public bool IsEmpty => Queue.Count == 0 && Leftovers.Count == 0;
}

/// <summary>
/// Picks what is left to loot in the store and puts it in walking order. A container already searched is skipped, so
/// a second press after a stop picks up where the first left off; an interaction is skipped once it has given all its
/// rewards for the day.
/// </summary>
public static class RunPlanner
{
    public static RunPlan Plan(IEnumerable<StoreFurniture> store, Spot from, LootSettings settings)
    {
        var visits = new List<Visit>();
        var leftovers = new List<long>();

        foreach (var furniture in store)
        {
            var funcs = furniture.Funcs.Where(func => IsDue(func, furniture)).ToList();

            // A furniture can be both: a searched container with items left that also has a reward due today.
            if (funcs.Count > 0) visits.Add(new Visit(furniture, funcs));
            if (settings.TakesContents && IsLeftover(furniture)) leftovers.Add(furniture.Id);
        }

        var queue = new List<QueuedAction>();
        var searches = new List<long>();
        var rewards = 0;

        foreach (var visit in WalkingOrder(visits, from))
        {
            var furniture = visit.Furniture;

            foreach (var func in visit.Funcs)
            {
                // The game's own furniture button lines up the walk before each of its actions, so the run does too.
                if (settings.Walks && furniture.WalkActionId > 0) queue.Add(new QueuedAction(furniture.WalkActionId, furniture.Id));

                queue.AddRange(func.ActionIds.Select(actionId => new QueuedAction(actionId, furniture.Id)));

                if (func is FurnitureFunc.Search) searches.Add(furniture.Id);
                else rewards++;
            }
        }

        return new RunPlan(queue, searches, rewards, leftovers);
    }

    private static bool IsDue(FurnitureFunc func, StoreFurniture furniture) => func.ActionIds.Count > 0 && func switch
    {
        FurnitureFunc.Search => !furniture.HasSearched,
        FurnitureFunc.Reward reward => !reward.IsUsedUp,
        _ => false,
    };

    private static bool IsLeftover(StoreFurniture furniture) =>
        furniture.HasSearched && furniture.ItemCount > 0 && furniture.Funcs.Any(func => func is FurnitureFunc.Search);

    /// <summary>Always to the nearest target not yet visited, starting where the character stands.</summary>
    private static IEnumerable<Visit> WalkingOrder(List<Visit> visits, Spot from)
    {
        var left = visits.ToList();
        var at = from;

        while (left.Count > 0)
        {
            var next = left.OrderBy(visit => visit.Furniture.Position.DistanceSquaredTo(at)).First();

            left.Remove(next);
            at = next.Furniture.Position;

            yield return next;
        }
    }

    private sealed record Visit(StoreFurniture Furniture, List<FurnitureFunc> Funcs);
}
