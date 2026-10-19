using System;
using System.Linq;
using GameCore.HotUpdate.ReduxUI;
using HarmonyLib;

namespace LootAllPreDisaster.Adapters;

/// <summary>
/// Tells the run which container a bag window was just opened for. A search that ends sends
/// Ac_BackpackUI_OpenUI.SendAction(bags, waits, container), and the window it opens can show a moment later, after the
/// run has already emptied that container. Static because Harmony hooks are.
/// </summary>
internal static class BagWindowPatch
{
    private static bool _hasReportedFailure;
    private static Action<string> _log = _ => { };

    /// <summary>Called on the game's main thread with the container the window was opened for.</summary>
    public static Action<long> Opened = _ => { };

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
    private static void AfterOpen(long __2)
    {
        try
        {
            Opened(__2);
        }
        catch (Exception ex)
        {
            if (_hasReportedFailure) return;

            _hasReportedFailure = true;
            _log($"handling a bag window failed (reported once): {ex}");
        }
    }
}
