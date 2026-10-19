using System;
using System.Collections.Generic;
using System.Linq;
using ExiledToolkit;
using LootAllPreDisaster.Core;

namespace LootAllPreDisaster.Adapters;

/// <summary>
/// The Mods menu entry and the run it starts. A press plans the store and lines up the actions; the ticker then
/// empties each container its search opened (modes 2 and 3), closes the bag window the search opened for it, and ends
/// the run when the character has nothing of it left.
/// </summary>
internal sealed class LootController
{
    private const float TickSeconds = 0.25f;

    private readonly GameStore _store;
    private readonly Func<LootSettings> _settings;
    private readonly Action<string> _log;
    private readonly BagWindowWatch _window = new();
    private float _now;
    private float _nextTick;

    public LootController(GameStore store, Func<LootSettings> settings, Action<string> log)
    {
        _store = store;
        _settings = settings;
        _log = log;
    }

    private static LootRun Run
    {
        get => OrderPatch.Run;
        set => OrderPatch.Run = value;
    }

    public bool IsShown() => _store.IsBeforeDisaster();

    /// <summary>The settings are read at each press, so a change of mode applies to the next one.</summary>
    public void OnClick()
    {
        var settings = _settings();

        Run.End();

        switch (Press.Decide(_store.Locate(), () => RunPlanner.Plan(_store.ReadStore(), _store.RolePosition(), settings)))
        {
            case PressDecision.Refuse refuse:
                Toast(refuse.Message, ToastKind.Info);
                break;
            case PressDecision.Start start:
                Begin(start.Plan, settings);
                break;
            default:
                throw new InvalidOperationException("unknown press decision");
        }
    }

    /// <summary>Each frame: the run's checks a few times a second, the bag window every frame so it closes before it is seen.</summary>
    public void Tick(float now)
    {
        _now = now;

        if (Run.IsActive && now >= _nextTick) TickRun(now);

        CloseEmptiedWindow(now);
    }

    public void OnBagWindowOpened(long containerId) => _window.Opened(containerId, _now);

    private void TickRun(float now)
    {
        _nextTick = now + TickSeconds;

        if (_store.Locate() != Place.Store)
        {
            EndRun("the player left the store");

            return;
        }

        if (!TakeDue()) return;

        if (Run.IsOver(_store.IsTargetQueued, _store.IsSearched, now)) EndRun("done");
    }

    private void Begin(RunPlan plan, LootSettings settings)
    {
        var run = new LootRun(plan, settings);

        _log($"press: mode {(int)settings.Mode}; {plan.Queue.Count} actions, {plan.Searches.Count} searches, {plan.Rewards} interactions, {plan.Leftovers.Count} containers searched before");
        OrderPatch.RoleId = _store.RoleId();
        Run = run;

        if (!TakeDue()) return;

        if (plan.Queue.Count > 0 && !StartActions(plan.Queue))
        {
            EndRun("the actions could not be lined up");
            Toast(Texts.Failed, ToastKind.Error);

            return;
        }

        Toast(Texts.Started(plan.Searches.Count + plan.Leftovers.Count, plan.Rewards), ToastKind.Info);
    }

    /// <summary>The run's own start goes through the same call as the player's orders, so it is marked as the run's.</summary>
    private bool StartActions(IReadOnlyList<QueuedAction> queue)
    {
        OrderPatch.IsRunStarting = true;

        try
        {
            return _store.TryStart(queue);
        }
        finally
        {
            OrderPatch.IsRunStarting = false;
        }
    }

    /// <summary>Empties every opened container still due. False when one did not fit, which stops the run.</summary>
    private bool TakeDue()
    {
        foreach (var containerId in Run.DueForTake(_store.IsSearched))
        {
            var itemsLeft = _store.Take(containerId);

            if (!BagWindowPatch.IsHooked) _store.CloseBagScreen();

            if (Run.Took(containerId, itemsLeft) == TakeOutcome.Emptied) continue;

            _store.Stop();
            _log($"container {containerId} kept {itemsLeft} stacks: storage full, run stopped");
            Toast(_store.HasTrunk() ? Texts.TrunkAndBagFull : Texts.BagFull, ToastKind.Warning);

            return false;
        }

        return true;
    }

    private void CloseEmptiedWindow(float now)
    {
        var run = Run;

        if (_window.ShouldClose(_store.IsBagScreenOpen, run.WasTakenFrom, containerId => run.DueForTake(_store.IsSearched).Contains(containerId), now)) _store.CloseBagScreen();
    }

    private void EndRun(string why)
    {
        Run.End();
        _log("run ended: " + why);
    }

    private static void Toast(Text text, ToastKind kind) =>
        Toasts.Show(new ToastRequest(Texts.Source.Pick(GameLanguage.IsChinese()), text.Pick(GameLanguage.IsChinese())) { Kind = kind });
}
