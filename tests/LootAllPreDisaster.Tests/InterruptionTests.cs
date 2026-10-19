using LootAllPreDisaster.Core;
using Xunit;

namespace LootAllPreDisaster.Tests;

public class InterruptionTests
{
    private const long Role = 7;

    [Fact]
    public void An_order_the_player_gives_during_a_run_ends_it()
    {
        Assert.True(Interruption.EndsRun(new NewOrder(Role, IsFromPlayer: true), isRunActive: true, Role, isRunStarting: false));
    }

    [Fact]
    public void The_runs_own_start_does_not_end_it()
    {
        Assert.False(Interruption.EndsRun(new NewOrder(Role, IsFromPlayer: true), isRunActive: true, Role, isRunStarting: true));
    }

    [Fact]
    public void An_order_the_game_gives_by_itself_does_not_end_the_run()
    {
        Assert.False(Interruption.EndsRun(new NewOrder(Role, IsFromPlayer: false), isRunActive: true, Role, isRunStarting: false));
    }

    [Fact]
    public void Another_characters_order_does_not_end_the_run()
    {
        Assert.False(Interruption.EndsRun(new NewOrder(Role + 1, IsFromPlayer: true), isRunActive: true, Role, isRunStarting: false));
    }

    [Fact]
    public void With_no_run_going_an_order_changes_nothing()
    {
        Assert.False(Interruption.EndsRun(new NewOrder(Role, IsFromPlayer: true), isRunActive: false, Role, isRunStarting: false));
    }
}
