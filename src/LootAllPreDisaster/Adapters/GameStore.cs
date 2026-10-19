using System;
using System.Collections.Generic;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle.Logic;
using GameCore.HotUpdate.ReduxUI;
using Il2CppInterop.Runtime;
using LootAllPreDisaster.Core;
using UnityEngine;
using Il2CppInts = Il2CppSystem.Collections.Generic.List<int>;

namespace LootAllPreDisaster.Adapters;

/// <summary>
/// The game's side of a run: where the player is, the store's furniture, lining up actions the way the furniture's
/// own buttons do, and emptying an opened container with the game's own whole-container move. Every read goes through
/// singletons the game already made. Each call fails soft, logging once per kind of call.
/// </summary>
internal sealed class GameStore
{
    /// <summary>The game's effect that opens a container's bag when the action ends.</summary>
    private const int OpenBagEffect = 7;

    /// <summary>The chapter of the furniture buttons that only exist after the disaster.</summary>
    private const int AfterDisasterChapter = 2;

    private readonly Action<string> _log;
    private readonly HashSet<string> _reportedFailures = new();
    private readonly Dictionary<int, FuncKind> _funcKinds = new();

    public GameStore(Action<string> log)
    {
        _log = log;
    }

    private enum FuncKind
    {
        Other,
        Search,
        Reward,
    }

    /// <summary>Cheap enough for the Mods menu to ask a few times a second: two singleton checks and one field.</summary>
    public bool IsBeforeDisaster()
    {
        var world = GameSingleton.Existing<BattleLogicWorld>();

        return world?._AgentManager?.GetLeadingRole() != null && world._GameTimeManager?.WorldStateType == WorldStateType.PreDisaster;
    }

    public Place Locate()
    {
        try
        {
            var world = GameSingleton.Existing<BattleLogicWorld>();
            var agents = world?._AgentManager;

            if (world == null || agents?.GetLeadingRole() == null || GameSingleton.Existing<ConfigManager>() == null) return Place.NoGame;
            if (world._GameTimeManager?.WorldStateType != WorldStateType.PreDisaster) return Place.AfterDisaster;

            return agents.IsHomeMap(ExploreSearchStateManager.GetCurrentMapId()) ? Place.Home : Place.Store;
        }
        catch (Exception ex)
        {
            ReportOnce("place check", ex);

            return Place.NoGame;
        }
    }

    public long RoleId() => GameSingleton.Existing<BattleLogicWorld>()?._AgentManager?.GetLeadingRole()?.InstanceId ?? 0;

    public Spot RolePosition()
    {
        var role = GameSingleton.Existing<BattleLogicWorld>()?._AgentManager?.GetLeadingRole();

        return role == null ? default : PositionOf(role);
    }

    /// <summary>The shown, usable furniture on the floor the player stands on. One piece that cannot be read is left out.</summary>
    public List<StoreFurniture> ReadStore()
    {
        var store = new List<StoreFurniture>();
        var world = GameSingleton.Existing<BattleLogicWorld>();
        var config = GameSingleton.Existing<ConfigManager>();
        var found = world?._AgentManager?.GetAllFurnitures();
        var items = world?._ItemManager;

        if (config == null || found == null || items == null) return store;

        var mapId = ExploreSearchStateManager.GetCurrentMapId();

        for (var i = 0; i < found.Count; i++)
        {
            var furniture = found[i];

            if (furniture == null || !furniture.IsShow || !furniture.FuncOpen || furniture.IsDead) continue;

            try
            {
                if (config.IsSameFloorScope(mapId, furniture.MapConfigId) && ReadFurniture(furniture, config, items) is { } read) store.Add(read);
            }
            catch (Exception ex)
            {
                ReportOnce("furniture read", ex);
            }
        }

        return store;
    }

    /// <summary>
    /// Lines up the plan and starts it as one batch, the way the game starts a furniture button's actions. Before the
    /// disaster that replaces whatever the character was doing. When it throws, the line is cleared so nothing half
    /// lined up is left behind.
    /// </summary>
    public bool TryStart(IReadOnlyList<QueuedAction> queue)
    {
        LeadingRole? role = null;

        try
        {
            role = GameSingleton.Existing<BattleLogicWorld>()?._AgentManager?.GetLeadingRole();

            if (role == null) return false;

            foreach (var action in queue) role.AddLeadingRoleAction(action.ActionId, action.FurnitureId, Vector3.zero, Quaternion.identity);

            role.DoLeadingRoleActionCache();

            return true;
        }
        catch (Exception ex)
        {
            ReportOnce("start", ex);

            if (role != null) ClearLine(role);

            return false;
        }
    }

    public bool IsSearched(long furnitureId)
    {
        try
        {
            return FurnitureById(furnitureId)?.HasSearched == true;
        }
        catch (Exception ex)
        {
            ReportOnce("searched check", ex);

            return false;
        }
    }

    /// <summary>A check that fails counts as not queued, so a broken read ends the run rather than holding it open.</summary>
    public bool IsTargetQueued(long furnitureId)
    {
        try
        {
            var world = GameSingleton.Existing<BattleLogicWorld>();
            var role = world?._AgentManager?.GetLeadingRole();

            return role != null && world!._ActionManager?.HasActionWithTarget(role.InstanceId, furnitureId) == true;
        }
        catch (Exception ex)
        {
            ReportOnce("queue check", ex);

            return false;
        }
    }

    public bool HasTrunk()
    {
        try
        {
            return VehicleConfig.HasVehicle();
        }
        catch (Exception ex)
        {
            ReportOnce("trunk check", ex);

            return false;
        }
    }

    /// <summary>
    /// Empties a container with the move the game's Take All button ends in: into the trunk first when the player has
    /// one, then into the bag. Each move takes what fits, and the game's own lock and weight checks apply. Answers how
    /// many stacks are left in the container; one that cannot be read counts as left behind, which stops the run.
    /// </summary>
    public int Take(long containerId)
    {
        try
        {
            var world = GameSingleton.Existing<BattleLogicWorld>();
            var items = world?._ItemManager;
            var role = world?._AgentManager?.GetLeadingRole();

            if (items == null || role == null) return 1;

            if (HasTrunk()) items.OnMoveBag(containerId, VehicleConfig.TrunkOwnerId);

            if (StacksIn(items, containerId) > 0) items.OnMoveBag(containerId, role.InstanceId);

            return StacksIn(items, containerId);
        }
        catch (Exception ex)
        {
            ReportOnce("take", ex);

            return 1;
        }
    }

    public bool IsBagScreenOpen()
    {
        try
        {
            return GameSingleton.Existing<ReduxUISystem>()?.IsUIActive(Il2CppType.Of<WebUI_BackpackUI>()) == true;
        }
        catch (Exception ex)
        {
            ReportOnce("bag screen check", ex);

            return false;
        }
    }

    /// <summary>The search opens the bag screen when it ends; the modes that take contents close it again.</summary>
    public void CloseBagScreen()
    {
        try
        {
            if (IsBagScreenOpen()) Ac_BackpackUI_Close.SendAction();
        }
        catch (Exception ex)
        {
            ReportOnce("bag screen close", ex);
        }
    }

    /// <summary>Stops the character: the lined-up batch and every action in its queue.</summary>
    public void Stop()
    {
        try
        {
            var world = GameSingleton.Existing<BattleLogicWorld>();
            var role = world?._AgentManager?.GetLeadingRole();

            if (role == null) return;

            ClearLine(role);
            world!._ActionManager?.ClearAgentAction(role.InstanceId);
        }
        catch (Exception ex)
        {
            ReportOnce("stop", ex);
        }
    }

    private StoreFurniture? ReadFurniture(Furniture furniture, ConfigManager config, ItemManager items)
    {
        var row = config.Get_Config_Furniture(furniture.AgentConfigId);
        var funcIds = row?.FurnitureFunc;

        if (row == null || funcIds == null || funcIds.Count == 0) return null;

        var usage = AgentTools.GetAgentComponent<FurnitureFuncCDComponent>(furniture)?.GetDailyUsageDict();
        var funcs = new List<FurnitureFunc>();

        for (var i = 0; i < funcIds.Count; i++)
        {
            var funcId = funcIds[i];
            var func = config.Get_Config_FurnitureFunc(funcId);

            if (func == null) continue;

            switch (KindOf(config, func))
            {
                case FuncKind.Search:
                    funcs.Add(new FurnitureFunc.Search(funcId, ToList(func.ActionIds)));
                    break;
                case FuncKind.Reward:
                    var used = usage != null && usage.ContainsKey(funcId) ? usage[funcId] : 0;

                    funcs.Add(new FurnitureFunc.Reward(funcId, ToList(func.ActionIds), func.DailyMax, used));
                    break;
                case FuncKind.Other:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(func), func.ID, "unknown kind of furniture button");
            }
        }

        if (funcs.Count == 0) return null;

        var id = furniture.InstanceId;

        return new StoreFurniture(id, row.ActionId, PositionOf(furniture), furniture.HasSearched, StacksIn(items, id), funcs);
    }

    /// <summary>
    /// A search is a timed action that opens the bag when it ends; tidying a storage box opens it at once and is left
    /// out. A reward button gives a daily reward set, shows without conditions, exists before the disaster, and every
    /// action of it has a length (trying the stove only opens the cooking panel). Each button is looked up once.
    /// </summary>
    private FuncKind KindOf(ConfigManager config, Config_FurnitureFunc func)
    {
        if (_funcKinds.TryGetValue(func.ID, out var known)) return known;

        var actions = func.ActionIds;
        var isSearch = false;
        var isTimed = actions != null && actions.Count > 0;

        for (var i = 0; actions != null && i < actions.Count; i++)
        {
            var action = config.Get_Config_Action(actions[i]);

            isTimed &= action != null && action.During > 0f;
            isSearch |= action != null && action.EffectConfigID == OpenBagEffect && action.During > 0f;
        }

        var isReward = isTimed && func.RewardSetId > 0 && func.DailyMax > 0 && func.VisibilityConditionSetId == 0 && func.Chapter != AfterDisasterChapter;
        var kind = isSearch ? FuncKind.Search : isReward ? FuncKind.Reward : FuncKind.Other;

        _funcKinds[func.ID] = kind;

        return kind;
    }

    private static Furniture? FurnitureById(long furnitureId) =>
        GameSingleton.Existing<BattleLogicWorld>()?._AgentManager?.GetAgent(furnitureId)?.TryCast<Furniture>();

    private static int StacksIn(ItemManager items, long ownerId) => items.GetItemDataList(ownerId)?.Count ?? 0;

    private static Spot PositionOf(BaseAgent agent)
    {
        var transform = AgentTools.GetAgentComponent<TransformComponent>(agent);

        if (transform == null) return default;

        var position = transform.Position;

        return new Spot(position.x, position.y, position.z);
    }

    private static List<int> ToList(Il2CppInts? ids)
    {
        var list = new List<int>();

        for (var i = 0; ids != null && i < ids.Count; i++) list.Add(ids[i]);

        return list;
    }

    private void ClearLine(LeadingRole role)
    {
        try
        {
            role.ClearLeadingRoleActionCache();
        }
        catch (Exception ex)
        {
            ReportOnce("clear line", ex);
        }
    }

    private void ReportOnce(string operation, Exception ex)
    {
        if (_reportedFailures.Add(operation)) _log($"{operation} failed (each kind reported once): {ex}");
    }
}
