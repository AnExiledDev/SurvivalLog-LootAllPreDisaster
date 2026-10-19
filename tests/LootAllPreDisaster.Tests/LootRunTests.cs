using LootAllPreDisaster.Core;
using Xunit;

namespace LootAllPreDisaster.Tests;

public class LootRunTests
{
    private static readonly LootSettings Take = new(LootMode.WalkAndTake);
    private static readonly LootSettings Open = new(LootMode.WalkAndOpen);

    private static RunPlan Plan(long[] searches, long[]? leftovers = null) => new(
        searches.SelectMany(id => new[] { new QueuedAction(100, id), new QueuedAction(1919, id) }).ToList(),
        searches,
        0,
        leftovers ?? Array.Empty<long>());

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

        Assert.False(run.IsOver(id => id == 1, _ => false, 0f));
        Assert.False(run.IsOver(id => id == 1, _ => false, 60f));
    }

    [Fact]
    public void The_run_is_not_over_while_an_opened_container_waits_to_be_emptied()
    {
        var run = new LootRun(Plan(new long[] { 1 }), Take);

        Assert.False(run.IsOver(_ => false, _ => true, 0f));
        Assert.False(run.IsOver(_ => false, _ => true, 60f));
    }

    [Fact]
    public void An_idle_run_only_ends_once_it_has_stayed_idle_for_the_settle_time()
    {
        var run = new LootRun(Plan(new long[] { 1 }), Take);

        Assert.False(run.IsOver(_ => false, _ => false, 10f));
        Assert.False(run.IsOver(_ => false, _ => false, 10f + LootRun.SettleSeconds / 2));
        Assert.True(run.IsOver(_ => false, _ => false, 10f + LootRun.SettleSeconds));
    }

    [Fact]
    public void A_container_that_opens_during_the_settle_time_is_still_emptied()
    {
        var run = new LootRun(Plan(new long[] { 1 }), Take);

        Assert.False(run.IsOver(_ => false, _ => false, 10f));
        Assert.Equal(new long[] { 1 }, run.DueForTake(_ => true));
        Assert.False(run.IsOver(_ => false, _ => true, 10.5f));
        run.Took(1, 0);
        Assert.False(run.IsOver(_ => false, _ => true, 11f));
        Assert.True(run.IsOver(_ => false, _ => true, 11f + LootRun.SettleSeconds));
    }

    [Fact]
    public void A_search_the_player_broke_off_does_not_hold_the_run_open()
    {
        var run = new LootRun(Plan(new long[] { 1 }), Take);

        Assert.False(run.IsOver(_ => false, _ => false, 0f));
        Assert.True(run.IsOver(_ => false, _ => false, LootRun.SettleSeconds));
    }

    [Fact]
    public void An_ended_run_is_over_at_once()
    {
        var run = new LootRun(Plan(new long[] { 1 }), Take);

        run.End();

        Assert.True(run.IsOver(id => id == 1, _ => true, 0f));
        Assert.False(LootRun.None.IsActive);
    }
}
