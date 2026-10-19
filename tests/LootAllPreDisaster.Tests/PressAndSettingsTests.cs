using LootAllPreDisaster.Core;
using Xunit;

namespace LootAllPreDisaster.Tests;

public class PressAndSettingsTests
{
    private static readonly RunPlan Empty = new(Array.Empty<QueuedAction>(), Array.Empty<long>(), 0, Array.Empty<long>());
    private static readonly RunPlan OneSearch = new(new[] { new QueuedAction(1919, 1) }, new long[] { 1 }, 0, Array.Empty<long>());

    [Theory]
    [InlineData(Place.NoGame)]
    [InlineData(Place.AfterDisaster)]
    [InlineData(Place.Home)]
    public void A_press_outside_a_store_before_the_disaster_is_refused_without_reading_the_store(Place place)
    {
        var decision = Press.Decide(place, () => throw new InvalidOperationException("the store was read"));

        Assert.IsType<PressDecision.Refuse>(decision);
    }

    [Fact]
    public void Each_refusal_says_why()
    {
        Assert.Equal(Texts.AfterDisaster, Assert.IsType<PressDecision.Refuse>(Press.Decide(Place.AfterDisaster, () => OneSearch)).Message);
        Assert.Equal(Texts.AtHome, Assert.IsType<PressDecision.Refuse>(Press.Decide(Place.Home, () => OneSearch)).Message);
    }

    [Fact]
    public void A_store_with_nothing_left_says_so()
    {
        Assert.Equal(Texts.NothingLeft, Assert.IsType<PressDecision.Refuse>(Press.Decide(Place.Store, () => Empty)).Message);
    }

    [Fact]
    public void A_store_with_something_left_starts_its_plan()
    {
        Assert.Same(OneSearch, Assert.IsType<PressDecision.Start>(Press.Decide(Place.Store, () => OneSearch)).Plan);
    }

    [Theory]
    [InlineData(1, LootMode.WalkAndOpen)]
    [InlineData(2, LootMode.WalkAndTake)]
    [InlineData(3, LootMode.TakeInPlace)]
    [InlineData(0, LootMode.WalkAndOpen)]
    [InlineData(7, LootMode.WalkAndOpen)]
    public void A_mode_out_of_range_falls_back_to_mode_one(int value, LootMode expected)
    {
        Assert.Equal(expected, LootSettings.ModeFrom(value));
    }

    [Fact]
    public void Only_mode_three_stands_still_and_only_mode_one_leaves_the_taking_to_the_player()
    {
        Assert.True(new LootSettings(LootMode.WalkAndOpen).Walks);
        Assert.False(new LootSettings(LootMode.WalkAndOpen).TakesContents);
        Assert.True(new LootSettings(LootMode.WalkAndTake).Walks);
        Assert.True(new LootSettings(LootMode.WalkAndTake).TakesContents);
        Assert.False(new LootSettings(LootMode.TakeInPlace).Walks);
        Assert.True(new LootSettings(LootMode.TakeInPlace).TakesContents);
    }

    [Fact]
    public void The_started_message_counts_in_english_singular_and_plural()
    {
        Assert.Equal("Looting 1 container and 2 interactions.", Texts.Started(1, 2).English);
        Assert.Equal("Looting 3 containers and 1 interaction.", Texts.Started(3, 1).English);
    }
}
