using ExiledToolkit.Kit;

namespace LootAllPreDisaster.Core;

/// <summary>Everything the mod says to the player, in English and Simplified Chinese.</summary>
public static class Texts
{
    public static readonly Phrase Source = new("Loot All Pre-Disaster", "灾变前全部搜寻");

    public static readonly Phrase MenuLabel = new("Loot this store", "搜寻整家店");

    public static readonly Phrase MenuTitle = new(
        "Search every container and use every daily reward in the store you're in",
        "搜寻你所在商店的所有容器，并使用所有还有每日奖励的互动");

    public static readonly Phrase NoGame = new("Load a game first.", "请先载入存档。");

    public static readonly Phrase AfterDisaster = new("This only works before the disaster.", "仅在灾变前可用。");

    public static readonly Phrase AtHome = new("You are at home. Go to a store first.", "你在家里。请先前往商店。");

    public static readonly Phrase NothingLeft = new("Nothing left to loot in this store today.", "这家店今天没有可搜寻的东西了。");

    public static readonly Phrase Failed = new(
        "The run couldn't start. Check BepInEx\\loot-all-pre-disaster.log.",
        "无法开始搜寻。请查看 BepInEx\\loot-all-pre-disaster.log。");

    public static readonly Phrase TrunkAndBagFull = new(
        "Your trunk and bag are full. Unload at home, then click Loot this store again to carry on.",
        "后备箱和背包都满了。回家卸货后，再点一次「搜寻整家店」即可继续。");

    public static readonly Phrase BagFull = new(
        "Your bag is full. Unload at home, then click Loot this store again to carry on.",
        "背包满了。回家卸货后，再点一次「搜寻整家店」即可继续。");

    public static Phrase Started(int containers, int rewards) => new(
        $"Looting {containers} {Plural(containers, "container", "containers")} and {rewards} {Plural(rewards, "interaction", "interactions")}.",
        $"正在搜寻 {containers} 个容器，进行 {rewards} 项互动。");

    private static string Plural(int count, string one, string many) => count == 1 ? one : many;
}
