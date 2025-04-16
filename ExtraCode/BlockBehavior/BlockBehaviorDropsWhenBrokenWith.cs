using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace ExtraCode.BlockBehavior;

public class BlockBehaviorDropsWhenBrokenWith(Block block) : Vintagestory.API.Common.BlockBehavior(block)
{
    private Dictionary<string, ItemStack[]> _resolvedDropMap = new();
    private Dictionary<string, JsonItemStack[]> _dropMap = new();

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        _dropMap = properties["dropMap"].AsObject<Dictionary<string, JsonItemStack[]>>() ?? new Dictionary<string, JsonItemStack[]>();
    }

    public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref float dropChanceMultiplier,
        ref EnumHandling handling)
    {
        ExtraCore.Logger?.Warning($"[BlockBehaviorDropsWhenBrokenWith][{block.Code}] GetDrops");
        ExtraCore.Logger?.Warning($"[BlockBehaviorDropsWhenBrokenWith][{block.Code}] dropMap: {_dropMap.Count}");
        foreach (var entry in _dropMap)
        {
            var itemStacks = string.Join(", ", entry.Value?.Select(stack1 => stack1.ToString()) ?? Array.Empty<string>());
            ExtraCore.Logger?.Warning($"Key: {entry.Key}, Value: [{itemStacks}]");
        }
        ExtraCore.Logger?.Warning($"[BlockBehaviorDropsWhenBrokenWith][{block.Code}] resolvedDropMap: {_resolvedDropMap.Count}");
        foreach (var entry in _resolvedDropMap)
        {
            var itemStacks = string.Join(", ", entry.Value?.Select(stack1 => stack1.ToString()) ?? Array.Empty<string>());
            ExtraCore.Logger?.Warning($"Key: {entry.Key}, Value: [{itemStacks}]");
        }
        if (byPlayer?.Entity == null) return base.GetDrops(world, pos, byPlayer, ref dropChanceMultiplier, ref handling);
        var breakToolCode = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Code.ToString();
        var drops = GetDropsForTool(breakToolCode);
        if (drops.Length == 0) return base.GetDrops(world, pos, byPlayer, ref dropChanceMultiplier, ref handling);
        ExtraCore.Logger?.Warning($"[BlockBehaviorDropsWhenBrokenWith][{block.Code}] GetDrops: {drops.Length} drops");
        handling = EnumHandling.PreventDefault;
        return drops;
    }
    
    private ItemStack[] GetDropsForTool(string toolCode)
    {
        var returnStacks = new List<ItemStack>();
        foreach (var entry in _resolvedDropMap)
        {
            if (!WildcardUtil.Match(entry.Key, toolCode)) continue;
            if (entry.Value == null) continue;
            returnStacks.AddRange(entry.Value);
        }
        return returnStacks.ToArray();
    }

    public void ResolveDrops(IWorldAccessor world)
    {
        foreach (var entry in _dropMap)
        {
            var resolvedStacks = new List<ItemStack>();
            foreach (var stack in entry.Value)
            {
                //ExtraCore.Logger.Warning($"[BlockBehaviorDropsWhenBrokenWith][{block.Code}] Resolving {stack.Code}");
                if (stack.Resolve(world, $"[{ExtraCore.ModId}]BlockBehaviorDropsWhenBrokenWith") &&
                     stack.ResolvedItemstack == null)
                {
                    continue;
                }
                resolvedStacks.Add(stack.ResolvedItemstack.Clone());
            }
            _resolvedDropMap.Add(entry.Key, resolvedStacks.ToArray());
        }
    }
}