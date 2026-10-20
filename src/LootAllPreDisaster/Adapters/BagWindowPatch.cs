using System;
using System.Collections.Generic;
using System.Linq;
using GameCore.HotUpdate.ReduxUI;
using HarmonyLib;

namespace LootAllPreDisaster.Adapters;

/// <summary>
/// Tells the run which bags a bag window was just opened with. A search that ends sends
/// Ac_BackpackUI_OpenUI.SendAction(bags, waits, furniture), and the window it opens can show a moment later, after the
/// run has already emptied that container. The bags are read rather than the furniture id, which the game leaves at 0
/// for a container that is not furniture. Static because Harmony hooks are.
/// </summary>
internal static class BagWindowPatch
{
    private static bool _hasReportedFailure;
    private static Action<string> _log = _ => { };

    /// <summary>Called on the game's main thread with the owner of every bag the window lists.</summary>
    public static Action<IReadOnlyList<long>> Opened = _ => { };

    /// <summary>Without the hook, the run closes the window right after it empties a container, as best it can.</summary>
    public static bool IsHooked { get; private set; }

    public static void Apply(Harmony harmony, Action<string> log)
    {
        _log = log;

        try
        {
            var target = typeof(Ac_BackpackUI_OpenUI).GetMethods().SingleOrDefault(method => method.Name == nameof(Ac_BackpackUI_OpenUI.SendAction) && method.GetParameters().Length == 3)
                         ?? throw new MissingMethodException("Ac_BackpackUI_OpenUI.SendAction(bags, waits, container)");

            harmony.Patch(target, postfix: new HarmonyMethod(typeof(BagWindowPatch), nameof(AfterOpen)));
            IsHooked = true;
            log("bag windows hooked: a window that shows after its container was emptied is closed");
        }
        catch (Exception ex)
        {
            log($"could not hook bag windows, a window may stay open after its container is emptied: {ex.Message}");
        }
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
            if (_hasReportedFailure) return;

            _hasReportedFailure = true;
            _log($"handling a bag window failed (reported once): {ex}");
        }
    }
}
