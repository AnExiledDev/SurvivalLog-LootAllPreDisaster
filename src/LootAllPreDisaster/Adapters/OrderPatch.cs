using System;
using ExiledToolkit.Game;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using LootAllPreDisaster.Core;

namespace LootAllPreDisaster.Adapters;

/// <summary>
/// Lets the player break off a run with any order, such as a click to walk somewhere. Every order the player gives
/// reaches ActionCacheComponent.DoActionCache, which in the game's default mode clears the character's line before
/// starting the new order, and in the queue mode (Full Queue switches it on before the disaster) adds the order to the
/// end. For an order that ends the run, the hook puts the default mode on for that one call, so the game itself
/// clears what is left of the run, then puts the mode back. Static because Harmony hooks are.
/// </summary>
internal static class OrderPatch
{
    private static Action<string> _log = _ => { };
    private static PatchKit? _hooks;

    /// <summary>The run in progress; the ticker and the hook both run on the game's main thread.</summary>
    public static LootRun Run = LootRun.None;

    /// <summary>The leading role the run belongs to.</summary>
    public static long RoleId;

    /// <summary>True while the run lines up its own actions, which go through the same call.</summary>
    public static bool IsRunStarting;

    public static void Apply(PatchKit hooks, Action<string> log)
    {
        _log = log;
        _hooks = hooks;

        hooks.Patch(
            typeof(OrderPatch),
            typeof(ActionCacheComponent),
            nameof(ActionCacheComponent.DoActionCache),
            new[] { typeof(ActionSourceType) },
            prefix: nameof(BeforeOrder),
            postfix: nameof(AfterOrder),
            done: "orders hooked: an order from the player ends a run",
            consequence: "a run can only be stopped by leaving the store");
    }

    /// <summary>Runs inside the game's call; an escaping exception would reach native code.</summary>
    private static void BeforeOrder(ActionCacheComponent __instance, ActionSourceType sourceType, ref bool __state)
    {
        __state = false;

        try
        {
            if (__instance == null) return;

            var order = new NewOrder(__instance.ownerInstanceId, sourceType == ActionSourceType.ManualCreate);

            if (!Interruption.EndsRun(order, Run.IsActive, RoleId, IsRunStarting)) return;

            Run.End();
            _log("run ended: the player gave an order");

            if (__instance.Mode == ActionMode.Default) return;

            __instance.Mode = ActionMode.Default;
            __state = true;
        }
        catch (Exception ex)
        {
            ReportFailure(ex);
        }
    }

    private static void AfterOrder(ActionCacheComponent __instance, bool __state)
    {
        try
        {
            if (__state && __instance != null) __instance.Mode = ActionMode.Queue;
        }
        catch (Exception ex)
        {
            ReportFailure(ex);
        }
    }

    /// <summary>Both hooks share one kind, so an order that keeps failing is logged once, not once per hook.</summary>
    private static void ReportFailure(Exception ex) => _hooks?.Fail("handling an order", ex);
}
