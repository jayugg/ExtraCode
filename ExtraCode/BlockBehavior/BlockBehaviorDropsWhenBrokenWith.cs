using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExtraCode.Util;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace ExtraCode.Behavior;

public class BlockBehaviorDropsWhenBrokenWith : BlockBehavior
{
    private Dictionary<string, ItemStack[]> _resolvedDropMap = new();
    private Dictionary<string, JsonItemStack[]> _dropMap = new();
    public BlockBehaviorDropsWhenBrokenWith(Block block) : base(block)
    {
    }
    
    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        this._dropMap = properties["dropMap"].AsObject<Dictionary<string, JsonItemStack[]>>() ?? new();
    }

    public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref float dropChanceMultiplier,
        ref EnumHandling handling)
    {
        ExtraCore.Logger.Warning($"[BlockBehaviorDropsWhenBrokenWith][{block.Code}] GetDrops");
        if (this._dropMap == null) ExtraCore.Logger.Warning($"[BlockBehaviorDropsWhenBrokenWith][{block.Code}] dropMap is null");
        ExtraCore.Logger.Warning($"[BlockBehaviorDropsWhenBrokenWith][{block.Code}] dropMap: {this._dropMap.Count}");
        foreach (var entry in this._dropMap)
        {
            var itemStacks = string.Join(", ", entry.Value?.Select(stack1 => stack1.ToString()) ?? Array.Empty<string>());
            ExtraCore.Logger.Warning($"Key: {entry.Key}, Value: [{itemStacks}]");
        }
        ExtraCore.Logger.Warning($"[BlockBehaviorDropsWhenBrokenWith][{block.Code}] resolvedDropMap: {this._resolvedDropMap.Count}");
        foreach (var entry in this._resolvedDropMap)
        {
            var itemStacks = string.Join(", ", entry.Value?.Select(stack1 => stack1.ToString()) ?? Array.Empty<string>());
            ExtraCore.Logger.Warning($"Key: {entry.Key}, Value: [{itemStacks}]");
        }
        if (byPlayer?.Entity == null) return base.GetDrops(world, pos, byPlayer, ref dropChanceMultiplier, ref handling);
        var breakToolCode = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Code.ToString();
        var drops = GetDropsForTool(breakToolCode);
        if (drops.Length == 0) return base.GetDrops(world, pos, byPlayer, ref dropChanceMultiplier, ref handling);
        ExtraCore.Logger.Warning($"[BlockBehaviorDropsWhenBrokenWith][{block.Code}] GetDrops: {drops.Length} drops");
        handling = EnumHandling.PreventDefault;
        return drops;
    }
    
    private ItemStack[] GetDropsForTool(string toolCode)
    {
        var returnStacks = new List<ItemStack>();
        foreach (var entry in this._resolvedDropMap)
        {
            if (WildcardUtil.Match(entry.Key, toolCode))
            {
                if (entry.Value == null) continue;
                returnStacks.AddRange(entry.Value);
            }
        }
        return returnStacks.ToArray();
    }

    public void ResolveDrops(IWorldAccessor world)
    {
        foreach (var entry in this._dropMap)
        {
            var resolvedStacks = new List<ItemStack>();
            foreach (var stack in entry.Value)
            {
                ExtraCore.Logger.Warning($"[BlockBehaviorDropsWhenBrokenWith][{block.Code}] Resolving {stack.Code}");
                stack.Resolve(world, $"[{ExtraCore.ModId}]BlockBehaviorDropsWhenBrokenWith");
                if (stack?.ResolvedItemstack == null)
                {
                    continue;
                }
                resolvedStacks.Add(stack.ResolvedItemstack.Clone());
            }
            this._resolvedDropMap.Add(entry.Key, resolvedStacks.ToArray());
        }
    }
}