namespace LootAllPreDisaster.Core;

public sealed record Text(string English, string Chinese)
{
    public string Pick(bool isChinese) => isChinese ? Chinese : English;
}

/// <summary>Everything the mod says to the player, in English and Simplified Chinese.</summary>
public static class Texts
{
    public static readonly Text Source = new("Loot All Pre-Disaster", "灾变前全部搜寻");

    public static readonly Text MenuLabel = new("Loot this store", "搜寻整家店");

    public static readonly Text MenuTitle = new(
        "Search every container and use every reward item in the store you are in",
        "搜寻你所在商店的所有容器，并使用所有能获得奖励的物品");

    public static readonly Text NoGame = new("Load a game first.", "请先载入存档。");

    public static readonly Text AfterDisaster = new("This only works before the disaster.", "仅在灾变前可用。");

    public static readonly Text AtHome = new("You are at home. Go to a store first.", "你在家里。请先前往商店。");

    public static readonly Text NothingLeft = new("Nothing left to loot in this store today.", "这家店今天没有可搜寻的东西了。");

    public static readonly Text Failed = new(
        "Could not start. See BepInEx\\loot-all-pre-disaster.log.",
        "无法开始。请查看 BepInEx\\loot-all-pre-disaster.log。");

    public static readonly Text TrunkAndBagFull = new(
        "Trunk and bag are full. Unload at home, then press again to carry on.",
        "后备箱和背包已满。回家卸货后再按一次即可继续。");

    public static readonly Text BagFull = new(
        "Your bag is full. Unload at home, then press again to carry on.",
        "背包已满。回家卸货后再按一次即可继续。");

    public static Text Started(int containers, int rewards) => new(
        $"Looting {containers} {Plural(containers, "container", "containers")} and {rewards} {Plural(rewards, "interaction", "interactions")}.",
        $"正在搜寻 {containers} 个容器，进行 {rewards} 项互动。");

    private static string Plural(int count, string one, string many) => count == 1 ? one : many;
}
