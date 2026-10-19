using LootAllPreDisaster.Core;
using Xunit;

namespace LootAllPreDisaster.Tests;

public class LootRunTests
{
    private static readonly LootSettings Take = new(LootMode.WalkAndTake, ActionTime.Runs);
    private static readonly LootSettings Open = new(LootMode.WalkAndOpen, ActionTime.Runs);

    private static RunPlan Plan(long[] searches, long[]? leftovers = null) => new(
        searches.SelectMany(id => new[] { new QueuedAction(100, id), new QueuedAction(1919, id) }).ToList(),
        searches,
        0,
        leftovers ?? Array.Empty<long>());

    [Fact]
    public void The_run_covers_its_own_actions_on_its_own_targets_only()
    {
        var run = new LootRun(Plan(new long[] { 1 }), Take);

        Assert.True(run.Covers(1919, 1));
        Assert.False(run.Covers(1919, 2));
        Assert.False(run.Covers(42, 1));
    }

    [Fact]
    public void An_ended_run_covers_nothing()
    {
        var run = new LootRun(Plan(new long[] { 1 }), Take);

        run.End();

        Assert.False(run.Covers(1919, 1));
        Assert.False(LootRun.None.Covers(1919, 1));
    }

    [Fact]
    public void A_container_is_due_for_taking_once_its_search_has_opened_it()
    {
        var run = new LootRun(Plan(new long[] { 1, 2 }), Take);

        Assert.Empty(run.DueForTake(_ => false));
        Assert.Equal(new long[] { 2 }, run.DueForTake(id => id == 2));
    }

    [Fact]
    public void Leftovers_are_due_at_once_and_before_the_searches()
    {
        var run = new LootRun(Plan(new long[] { 1 }, new long[] { 9 }), Take);

        Assert.Equal(new long[] { 9, 1 }, run.DueForTake(_ => true));
    }

    [Fact]
    public void Mode_one_takes_nothing_for_the_player()
    {
        var run = new LootRun(Plan(new long[] { 1 }, new long[] { 9 }), Open);

        Assert.Empty(run.DueForTake(_ => true));
    }

    [Fact]
    public void A_container_emptied_is_not_taken_from_again()
    {
        var run = new LootRun(Plan(new long[] { 1 }), Take);

        Assert.Equal(TakeOutcome.Emptied, run.Took(1, 0));
        Assert.Empty(run.DueForTake(_ => true));
        Assert.True(run.IsActive);
    }

    [Fact]
    public void A_container_that_only_part_fits_stops_the_run()
    {
        var run = new LootRun(Plan(new long[] { 1, 2 }), Take);

        Assert.Equal(TakeOutcome.StorageFull, run.Took(1, 3));
        Assert.False(run.IsActive);
        Assert.Empty(run.DueForTake(_ => true));
    }

    [Fact]
    public void The_run_is_not_over_while_a_target_is_still_lined_up()
    {
        var run = new LootRun(Plan(new long[] { 1 }), Take);

        Assert.False(run.IsOver(id => id == 1, _ => false));
    }

    [Fact]
    public void The_run_is_not_over_while_an_opened_container_waits_to_be_emptied()
    {
        var run = new LootRun(Plan(new long[] { 1 }), Take);

        Assert.False(run.IsOver(_ => false, _ => true));
        run.Took(1, 0);
        Assert.True(run.IsOver(_ => false, _ => true));
    }

    [Fact]
    public void A_search_the_player_broke_off_does_not_hold_the_run_open()
    {
        var run = new LootRun(Plan(new long[] { 1 }), Take);

        Assert.True(run.IsOver(_ => false, _ => false));
    }
}
