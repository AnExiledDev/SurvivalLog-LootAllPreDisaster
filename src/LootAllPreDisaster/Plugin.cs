using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using ExiledToolkit;
using ExiledToolkit.Game;
using ExiledToolkit.Kit;
using HarmonyLib;
using LootAllPreDisaster.Adapters;
using LootAllPreDisaster.Core;

namespace LootAllPreDisaster;

[BepInPlugin(Guid, "Loot All Pre-Disaster", Version)]
[BepInDependency(ExiledToolkit.Plugin.Guid, ">=0.19.0")]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.lootallpredisaster";
    private const string Version = "0.2.2";

    /// <summary>Lines add up over many sessions, so past 1 MB the file is moved aside and started again.</summary>
    private const long MaxLogBytes = 1024 * 1024;

    /// <summary>A trash can with a down arrow: emptying something into your own storage.</summary>
    private const string Icon = "M5 4h14v4H5zM6 9h12l-1 11H7zM12 11v6M9.5 14.5 12 17l2.5-2.5";

    public override void Load()
    {
        var log = new ModLog(Log, "loot-all-pre-disaster.log") { MaxFileBytes = MaxLogBytes };

        log.Write($"===== Loot All Pre-Disaster {Version} started");

        var mode = Config.Bind("Loot", "Mode", 1, new ConfigDescription(
            "What happens when you click Loot this store. 1: you walk to each container and interaction, and each container's bag window opens so you can take what you want. 2: you walk to each one and take everything with no window. Items go into your trunk first if you have a car or shopping cart, then into your bag. 3: you take everything with no window and without walking. Each action still takes its normal time.\n" +
            "点击「搜寻整家店」后怎么搜。1：走到每个容器和互动处，每个容器都会打开背包窗口，你自己拿想要的东西。2：走到每一处，不打开窗口，直接拿走全部物品。有汽车或购物车时先放进后备箱，再放进背包。3：不打开窗口，也不走动，直接拿走全部物品。每个动作仍然花正常的时间。",
            new AcceptableValueRange<int>(1, 3), WorldRules.Tag));
        var settings = WatchSettings(mode, log.Write);
        var store = new GameStore(log.Write);
        var controller = new LootController(store, settings, log.Write);

        var hooks = new PatchKit(new Harmony(Guid), log.Write);

        OrderPatch.Apply(hooks, log.Write);
        BagWindowPatch.Apply(hooks);
        BagWindowPatch.Opened = controller.OnBagWindowOpened;
        ModsMenu.Add(new ModsMenuEntry("lootallpredisaster.loot", Texts.MenuLabel.En, controller.OnClick)
        {
            ChineseLabel = Texts.MenuLabel.Zh,
            Title = Texts.MenuTitle.En,
            ChineseTitle = Texts.MenuTitle.Zh,
            Icon = Icon,
            Order = 300,
            IsShown = controller.IsShown,
        });

        LootTicker.Log = log.Write;
        LootTicker.Checks.Add(controller.Tick);
        AddComponent<LootTicker>();
    }

    /// <summary>The mode changes in game without a restart; it is read at each click, since a run's plan is made then.</summary>
    private static Func<LootSettings> WatchSettings(ConfigEntry<int> mode, Action<string> log)
    {
        var current = new LootSettings(LootMode.WalkAndOpen);

        LiveSettings.Watch(() =>
        {
            current = new LootSettings(LootSettings.ModeFrom(mode.Value));
            log($"settings: mode {(int)current.Mode}");
        }, mode);

        return () => current;
    }
}
