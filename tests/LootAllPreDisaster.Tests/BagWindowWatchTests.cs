using LootAllPreDisaster.Core;
using Xunit;

namespace LootAllPreDisaster.Tests;

public class BagWindowWatchTests
{
    private const long Container = 5;

    private static bool Taken(long id) => id == Container;

    private static bool NotTaken(long id) => false;

    private static bool Due(long id) => id == Container;

    private static bool Shown() => true;

    private static bool Hidden() => false;

    private static bool NotDue(long id) => false;

    [Fact]
    public void A_window_that_shows_after_the_run_emptied_its_container_is_closed()
    {
        var watch = new BagWindowWatch();

        watch.Opened(Container, 10f);

        Assert.False(watch.ShouldClose(Hidden, Taken, NotDue, 10.2f));
        Assert.True(watch.ShouldClose(Shown, Taken, NotDue, 10.5f));
    }

    [Fact]
    public void A_window_is_closed_once_only()
    {
        var watch = new BagWindowWatch();

        watch.Opened(Container, 10f);

        Assert.True(watch.ShouldClose(Shown, Taken, NotDue, 10f));
        Assert.False(watch.ShouldClose(Shown, Taken, NotDue, 10.1f));
    }

    [Fact]
    public void A_window_waits_while_the_run_is_about_to_empty_its_container()
    {
        var watch = new BagWindowWatch();

        watch.Opened(Container, 10f);

        Assert.False(watch.ShouldClose(Shown, NotTaken, Due, 10f));
        Assert.True(watch.ShouldClose(Shown, Taken, NotDue, 10.25f));
    }

    [Fact]
    public void A_window_for_a_container_the_run_does_not_take_from_stays_open()
    {
        var watch = new BagWindowWatch();

        watch.Opened(Container, 10f);

        Assert.False(watch.ShouldClose(Shown, NotTaken, NotDue, 10f));
        Assert.False(watch.ShouldClose(Shown, Taken, NotDue, 10.25f));
    }

    [Fact]
    public void A_window_that_never_shows_is_given_up_on()
    {
        var watch = new BagWindowWatch();

        watch.Opened(Container, 10f);

        Assert.False(watch.ShouldClose(Hidden, Taken, NotDue, 10f + BagWindowWatch.WaitSeconds + 0.1f));
        Assert.False(watch.ShouldClose(Shown, Taken, NotDue, 10f + BagWindowWatch.WaitSeconds + 0.2f));
    }

    [Fact]
    public void Nothing_is_closed_before_any_window_opened()
    {
        Assert.False(new BagWindowWatch().ShouldClose(Shown, Taken, NotDue, 0f));
    }
}
