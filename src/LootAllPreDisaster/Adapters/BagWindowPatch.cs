using System;
using System.Collections.Generic;
using ExiledToolkit.Game;
using GameCore.HotUpdate.ReduxUI;

namespace LootAllPreDisaster.Adapters;

/// <summary>
/// Tells the run which bags a bag window was just opened with. A search that ends sends
/// Ac_BackpackUI_OpenUI.SendAction(bags, waits, furniture), and the window it opens can show a moment later, after the
/// run has already emptied that container. The bags are read rather than the furniture id, which the game leaves at 0
/// for a container that is not furniture. Static because Harmony hooks are.
/// </summary>
internal static class BagWindowPatch
{
    private static PatchKit? _hooks;

    /// <summary>Called on the game's main thread with the owner of every bag the window lists.</summary>
    public static Action<IReadOnlyList<long>> Opened = _ => { };

    /// <summary>Without the hook, the run closes the window right after it empties a container, as best it can.</summary>
    public static bool IsHooked { get; private set; }

    public static void Apply(PatchKit hooks)
    {
        _hooks = hooks;

        IsHooked = hooks.Patch(
            typeof(BagWindowPatch),
            typeof(Ac_BackpackUI_OpenUI),
            nameof(Ac_BackpackUI_OpenUI.SendAction),
            new[] { typeof(Il2CppSystem.Collections.Generic.List<Data_Bag>), typeof(bool), typeof(long) },
            postfix: nameof(AfterOpen),
            done: "bag windows hooked: a window that shows after its container was emptied is closed",
            consequence: "a window may stay open after its container is emptied");
    }

    /// <summary>Runs inside the game's own call; an escaping exception would reach native code.</summary>
    private static void AfterOpen(Il2CppSystem.Collections.Generic.List<Data_Bag> __0)
    {
        try
        {
            var owners = new List<long>();

            for (var i = 0; i < (__0?.Count ?? 0); i++)
            {
                if (__0![i] is { } bag) owners.Add(bag.OwnerId);
            }

            Opened(owners);
        }
        catch (Exception ex)
        {
            _hooks?.Fail("handling a bag window", ex);
        }
    }
}
