using System;
using System.Linq;
using ExtraCode.Util;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace ExtraCode.BlockBehavior;

public class BlockBehaviorBreakSpawner(Block block) : Vintagestory.API.Common.BlockBehavior(block)
{
    protected string[] EntityCodes;
    protected int[] EntityWeights;
    protected bool RequiresTool = false;
    protected bool SpawnAll = false;
    protected string ToolCode;
    protected Vec3d SpawnOffset;
    protected bool DebugFlag;
    protected bool DoDrops;
    protected bool HasSpawned;

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        EntityCodes = properties["entityCodes"].AsArray<string>([]);
        EntityWeights = properties["entityWeights"].AsArray<int>(EntityCodes.Length > 0 ? EntityCodes.Select(_ => 1).ToArray() : []);
        RequiresTool = properties["requiresTool"].AsBool();
        SpawnAll = properties["spawnAll"].AsBool();
        ToolCode = properties["toolCode"].AsString("");
        SpawnOffset = properties["spawnOffset"].AsObject<Vec3d>(new Vec3d(0, 0, 0));
        DebugFlag = properties["debugFlag"].AsBool(false);
        DoDrops = properties["doDrops"].AsBool();
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
    {
        base.OnBlockBroken(world, pos, byPlayer, ref handling);
        if (byPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative && !DebugFlag) return;
        handling = EnumHandling.Handled;
        if (DebugFlag) ExtraCore.Logger?.Warning($"[BehaviorBreakSpawner][{block.Code}] Block broken at {pos}");
        if (!HasRequiredTool(byPlayer, pos)) return;
        world.RegisterCallback((worldAccessor, blockPos, _) => SpawnEntities(worldAccessor, blockPos), pos, 50);
    }

    public override bool OnBlockBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel,
        float dropQuantityMultiplier, ref EnumHandling bhHandling)
    {
        if (!DoDrops || byEntity is EntityPlayer entityPlayer && !HasRequiredTool(entityPlayer.Player, blockSel?.Position)) dropQuantityMultiplier = 0;
        return base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel, dropQuantityMultiplier, ref bhHandling);
    }

    protected virtual void SpawnEntities(IWorldAccessor world, BlockPos pos)
    {
        if (SpawnAll)
        {
            if (DebugFlag) ExtraCore.Logger?.Warning($"[BehaviorBreakSpawner][{block.Code}] Spawning all entities at {pos}");
            for (var i = 0; i < EntityCodes.Length; i++)
            {
                if (world.Rand.NextDouble() < EntityWeights[i]) SpawnEntity(world, pos, EntityCodes[i]);
            }
            return;
        }
        SpawnEntity(world, pos, EntityCodes.RandomElementByWeight(code => EntityWeights[Array.IndexOf(EntityCodes, code)]));
    }

    protected virtual void SpawnEntity(IWorldAccessor world, BlockPos pos, string entityCode)
    {
        var entityType = world.GetEntityType(new AssetLocation(entityCode));
        if (entityType == null)
        {
            if (DebugFlag) ExtraCore.Logger?.Warning($"[BehaviorBreakSpawner][{block.Code}] Entity {entityCode} not found");
            return;
        }
        var entity1 = world.ClassRegistry.CreateEntity(entityType);
        entity1.ServerPos.SetPos(pos.X + 0.5 + SpawnOffset.X, pos.Y + SpawnOffset.Y, pos.Z + 0.5 + SpawnOffset.X);
        world.SpawnEntity(entity1);
        if (DebugFlag) ExtraCore.Logger?.Warning($"[BehaviorBreakSpawner][{block.Code}] Spawned entity {entityCode} at {pos}");
    }
    
    protected bool HasRequiredTool(IPlayer byPlayer, BlockPos pos)
    {
        if (!RequiresTool) return true;
        var activeToolCode = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Code.ToString();
        if (WildcardUtil.Match(ToolCode, activeToolCode)) return true;
        if (DebugFlag) ExtraCore.Logger?.Warning($"[BehaviorBreakSpawner][{block.Code}] Tool {ToolCode} required to break block at {pos}");
        return false;
    }
}