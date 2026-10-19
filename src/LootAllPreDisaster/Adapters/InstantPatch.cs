using System;
using System.Collections.Generic;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using HarmonyLib;
using LootAllPreDisaster.Core;

namespace LootAllPreDisaster.Adapters;

/// <summary>
/// Ends the run's actions at once when ActionTime is Instant. Every action starts an effect that lasts the action's
/// length, and the effect manager's per-frame update hands one time step to all of it: the effect's remaining life,
/// its triggers and the timers that pay out its rewards. Stretching that one step runs the whole effect in a frame or
/// two, so the action still gives everything it gives. Only the leading role's actions that the current run lined up
/// are touched. Static because Harmony patches are.
/// </summary>
internal static class InstantPatch
{
    private static readonly HashSet<string> ReportedFailures = new();
    private static Action<string> _log = _ => { };

    /// <summary>The run in progress; set from the ticker's frame, read from the game's.</summary>
    public static volatile LootRun Run = LootRun.None;

    /// <summary>The leading role the run belongs to.</summary>
    public static long RoleId;

    /// <summary>Hooked whatever the setting, since the setting is read at each press.</summary>
    public static void Apply(Harmony harmony, Action<string> log)
    {
        _log = log;

        try
        {
            var target = AccessTools.Method(typeof(BTEffectManager), nameof(BTEffectManager.UpdateNode), new[] { typeof(BtConfig), typeof(EffectBtArgs), typeof(float), typeof(float) })
                         ?? throw new MissingMethodException("BTEffectManager.UpdateNode(BtConfig, EffectBtArgs, float, float)");

            harmony.Patch(target, prefix: new HarmonyMethod(typeof(InstantPatch), nameof(EndAtOnce)));
            log("action effects hooked for ActionTime Instant");
        }
        catch (Exception ex)
        {
            log($"could not hook action effects, ActionTime Instant takes normal time: {ex.Message}");
        }
    }

    /// <summary>Runs inside the game's own call, for every effect every frame; an escaping exception would reach native code.</summary>
    private static void EndAtOnce(EffectBtArgs args, ref float deltaTime)
    {
        try
        {
            var run = Run;

            if (!run.IsActive || run.Settings.Time != ActionTime.Instant || args == null) return;
            if (args._ArgsType_k__BackingField != EffectArgsType.Action || args._OwnerId_k__BackingField != RoleId) return;
            if (!IsRunsAction(run)) return;

            deltaTime = InstantClock.Step(deltaTime, args._MaxLifeDuration_k__BackingField, args._CurLifeDuration_k__BackingField, InstantClock.LongestStep(Toolset._TimeScale));
        }
        catch (Exception ex)
        {
            if (ReportedFailures.Add($"{ex.GetType().FullName}: {ex.Message}")) _log($"ending an action at once failed, it takes normal time: {ex}");
        }
    }

    /// <summary>The game sets a character's current action just before starting its effect, so the effect belongs to it.</summary>
    private static bool IsRunsAction(LootRun run)
    {
        var owner = GameSingleton.Existing<BattleLogicWorld>()?._AgentManager?.GetAgent(RoleId);
        var action = owner == null ? null : AgentTools.GetAgentComponent<ActionComponent>(owner);
        var actionId = action?.ActionConfig?.ID;

        if (action == null || actionId == null) return false;

        var target = action.GetTarget();
        var furniture = target.Item2 == InstanceIdType.Furniture ? target.Item1?.TryCast<Furniture>() : null;

        return furniture != null && run.Covers(actionId.Value, furniture.InstanceId);
    }
}
