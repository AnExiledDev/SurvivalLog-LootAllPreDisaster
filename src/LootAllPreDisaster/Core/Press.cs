using System;

namespace LootAllPreDisaster.Core;

/// <summary>Where the player is when they press.</summary>
public enum Place
{
    NoGame,
    AfterDisaster,
    Home,
    Store,
}

public abstract record PressDecision
{
    public sealed record Refuse(Text Message) : PressDecision;

    public sealed record Start(RunPlan Plan) : PressDecision;
}

public static class Press
{
    /// <summary>The store is only read once the player is known to be in one.</summary>
    public static PressDecision Decide(Place place, Func<RunPlan> plan)
    {
        switch (place)
        {
            case Place.NoGame:
                return new PressDecision.Refuse(Texts.NoGame);
            case Place.AfterDisaster:
                return new PressDecision.Refuse(Texts.AfterDisaster);
            case Place.Home:
                return new PressDecision.Refuse(Texts.AtHome);
            case Place.Store:
                var planned = plan();

                return planned.IsEmpty ? new PressDecision.Refuse(Texts.NothingLeft) : new PressDecision.Start(planned);
            default:
                throw new ArgumentOutOfRangeException(nameof(place), place, null);
        }
    }
}
