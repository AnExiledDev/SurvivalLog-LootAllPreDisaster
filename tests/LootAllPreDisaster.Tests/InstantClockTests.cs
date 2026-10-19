using LootAllPreDisaster.Core;
using Xunit;

namespace LootAllPreDisaster.Tests;

public class InstantClockTests
{
    [Fact]
    public void A_short_action_ends_in_one_step()
    {
        Assert.Equal(5f, InstantClock.Step(0.02f, 5f, 5f, 36f));
    }

    [Fact]
    public void A_long_action_never_skips_a_payout_in_one_step()
    {
        Assert.Equal(36f, InstantClock.Step(0.02f, 600f, 400f, 36f));
    }

    [Fact]
    public void A_step_is_never_shorter_than_the_game_step()
    {
        Assert.Equal(0.5f, InstantClock.Step(0.5f, 10f, 0.1f, 36f));
    }

    [Fact]
    public void An_effect_with_no_length_keeps_the_game_step()
    {
        Assert.Equal(0.02f, InstantClock.Step(0.02f, 0f, 0f, 36f));
        Assert.Equal(0.02f, InstantClock.Step(0.02f, -1f, 3f, 36f));
    }

    [Fact]
    public void The_longest_step_is_the_shortest_payout_in_effect_time()
    {
        Assert.Equal(36f, InstantClock.LongestStep(10));
    }

    [Fact]
    public void An_unknown_time_scale_lets_the_effect_run_to_its_end()
    {
        Assert.Equal(float.PositiveInfinity, InstantClock.LongestStep(0));
        Assert.Equal(400f, InstantClock.Step(0.02f, 600f, 400f, InstantClock.LongestStep(0)));
    }
}
