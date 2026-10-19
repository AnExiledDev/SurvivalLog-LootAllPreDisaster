using LootAllPreDisaster.Core;
using Xunit;

namespace LootAllPreDisaster.Tests;

public class RunPlannerTests
{
    private const int Walk = 100;
    private const int SearchAction = 1919;
    private const int RewardAction = 500;

    private static readonly LootSettings WalkAndOpen = new(LootMode.WalkAndOpen);
    private static readonly LootSettings WalkAndTake = new(LootMode.WalkAndTake);
    private static readonly LootSettings TakeInPlace = new(LootMode.TakeInPlace);

    private static StoreFurniture Container(long id, float x = 0f, bool searched = false, int items = 3) =>
        new(id, Walk, new Spot(x, 0f, 0f), searched, items, new FurnitureFunc[] { new FurnitureFunc.Search(1721, new[] { SearchAction }) });

    private static StoreFurniture Arcade(long id, int usedToday, int dailyMax = 2, float x = 0f) =>
        new(id, Walk, new Spot(x, 0f, 0f), false, 0, new FurnitureFunc[] { new FurnitureFunc.Reward(900, new[] { RewardAction }, dailyMax, usedToday) });

    [Fact]
    public void A_container_searched_before_is_not_searched_again()
    {
        var plan = RunPlanner.Plan(new[] { Container(1, searched: true), Container(2) }, default, WalkAndOpen);

        Assert.Equal(new long[] { 2 }, plan.Searches);
        Assert.DoesNotContain(plan.Queue, action => action.FurnitureId == 1);
    }

    [Fact]
    public void An_interaction_that_gave_all_its_rewards_today_is_skipped()
    {
        var plan = RunPlanner.Plan(new[] { Arcade(1, usedToday: 2), Arcade(2, usedToday: 1) }, default, WalkAndOpen);

        Assert.Equal(1, plan.Rewards);
        Assert.All(plan.Queue, action => Assert.Equal(2, action.FurnitureId));
    }

    [Fact]
    public void Leftovers_in_containers_searched_before_are_taken_only_in_the_modes_that_take()
    {
        var store = new[] { Container(1, searched: true, items: 2), Container(2, searched: true, items: 0) };

        Assert.Empty(RunPlanner.Plan(store, default, WalkAndOpen).Leftovers);
        Assert.Equal(new long[] { 1 }, RunPlanner.Plan(store, default, WalkAndTake).Leftovers);
        Assert.Equal(new long[] { 1 }, RunPlanner.Plan(store, default, TakeInPlace).Leftovers);
    }

    [Fact]
    public void A_searched_store_with_nothing_left_is_an_empty_plan()
    {
        Assert.True(RunPlanner.Plan(new[] { Container(1, searched: true, items: 0), Arcade(2, usedToday: 2) }, default, WalkAndTake).IsEmpty);
    }

    [Fact]
    public void Walking_modes_walk_to_each_target_before_its_actions()
    {
        var plan = RunPlanner.Plan(new[] { Container(1) }, default, WalkAndTake);

        Assert.Equal(new[] { new QueuedAction(Walk, 1), new QueuedAction(SearchAction, 1) }, plan.Queue);
    }

    [Fact]
    public void Mode_three_lines_up_no_walks()
    {
        var plan = RunPlanner.Plan(new[] { Container(1), Arcade(2, usedToday: 0) }, default, TakeInPlace);

        Assert.DoesNotContain(plan.Queue, action => action.ActionId == Walk);
        Assert.Equal(2, plan.Queue.Count);
    }

    [Fact]
    public void Targets_are_visited_nearest_first_from_where_the_player_stands()
    {
        var store = new[] { Container(1, x: 10f), Container(2, x: 1f), Container(3, x: 4f) };

        var plan = RunPlanner.Plan(store, new Spot(0f, 0f, 0f), WalkAndOpen);

        Assert.Equal(new long[] { 2, 3, 1 }, plan.Searches);
    }

    [Fact]
    public void The_order_follows_the_path_not_the_distance_from_the_start()
    {
        var store = new[] { Container(1, x: -3f), Container(2, x: 2f), Container(3, x: 4f) };

        var plan = RunPlanner.Plan(store, new Spot(0f, 0f, 0f), WalkAndOpen);

        Assert.Equal(new long[] { 2, 3, 1 }, plan.Searches);
    }

    [Fact]
    public void Furniture_with_no_walk_action_still_gets_its_actions()
    {
        var store = new[] { Container(1) with { WalkActionId = 0 } };

        Assert.Equal(new[] { new QueuedAction(SearchAction, 1) }, RunPlanner.Plan(store, default, WalkAndOpen).Queue);
    }
}
