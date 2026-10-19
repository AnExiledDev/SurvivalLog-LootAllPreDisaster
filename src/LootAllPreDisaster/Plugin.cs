using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using ExiledToolkit;
using HarmonyLib;
using LootAllPreDisaster.Adapters;
using LootAllPreDisaster.Core;

namespace LootAllPreDisaster;

[BepInPlugin(Guid, "Loot All Pre-Disaster", Version)]
[BepInDependency(ExiledToolkit.Plugin.Guid, ">=0.5.0")]
public sealed class Plugin : BasePlugin
{
    private const string Guid = "com.dev.survivallog.lootallpredisaster";
    private const string Version = "0.1.0";

    /// <summary>A trash can with a down arrow: emptying something into your own storage.</summary>
    private const string Icon = "M5 4h14v4H5zM6 9h12l-1 11H7zM12 11v6M9.5 14.5 12 17l2.5-2.5";

    public override void Load()
    {
        var log = new LootLog(Log, Path.Combine(Paths.BepInExRootPath, "loot-all-pre-disaster.log"));

        log.TrimIfLarge();
        log.Write($"===== Loot All Pre-Disaster {Version} started");

        var mode = Config.Bind("Loot", "Mode", 1, new ConfigDescription(
            "How a press loots the store. 1: walk to each container and interaction; each container's bag window opens and you take what you want. 2: walk to each one and take everything at once with no window, into the trunk first if you have a car or shopping cart, then the bag. 3: take everything at once with no window and no walking.\n" +
            "按下后如何搜寻商店。1：走到每个容器和互动处；每个容器的背包窗口会打开，由你自己拿取想要的物品。2：走到每一处，不打开窗口直接拿走全部物品，有汽车或购物车时先放进后备箱，再放进背包。3：不打开窗口、不走动，直接拿走全部物品。",
            new AcceptableValueRange<int>(1, 3)));
        var settings = WatchSettings(mode, log.Write);
        var store = new GameStore(log.Write);
        var controller = new LootController(store, settings, log.Write);

        OrderPatch.Apply(new Harmony(Guid), log.Write);
        ModsMenu.Add(new ModsMenuEntry("lootallpredisaster.loot", Texts.MenuLabel.English, controller.OnClick)
        {
            ChineseLabel = Texts.MenuLabel.Chinese,
            Title = Texts.MenuTitle.English,
            ChineseTitle = Texts.MenuTitle.Chinese,
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
