using System;
using System.Linq;
using ExtraCode.Util;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace ExtraCode.Behavior;

public class BlockBehaviorBreakSpawner : BlockBehavior
{

    protected string[] entityCodes;
    protected int[] entityWeights;
    protected bool requiresTool = false;
    protected bool spawnAll = false;
    protected string toolCode;
    protected Vec3d spawnOffset;
    protected bool debugFlag;
    protected bool doDrops;
    protected bool hasSpawned;

    public BlockBehaviorBreakSpawner(Block block) : base(block)
    {
    }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        this.entityCodes = properties["entityCodes"].AsArray<string>(Array.Empty<string>());
        this.entityWeights = properties["entityWeights"].AsArray<int>(entityCodes.Length > 0 ? entityCodes.Select(_ => 1).ToArray() : Array.Empty<int>());
        this.requiresTool = properties["requiresTool"].AsBool();
        this.spawnAll = properties["spawnAll"].AsBool();
        this.toolCode = properties["toolCode"].AsString("");
        this.spawnOffset = properties["spawnOffset"].AsObject<Vec3d>(new Vec3d(0, 0, 0));
        this.debugFlag = properties["debugFlag"].AsBool(false);
        this.doDrops = properties["doDrops"].AsBool();
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
    {
        base.OnBlockBroken(world, pos, byPlayer, ref handling);
        if (byPlayer.WorldData.CurrentGameMode == EnumGameMode.Creative && !debugFlag) return;
        handling = EnumHandling.Handled;
        if (debugFlag) ExtraCore.Logger.Warning($"[BehaviorBreakSpawner][{block.Code}] Block broken at {pos}");
        if (!HasRequiredTool(byPlayer, pos)) return;
        world.RegisterCallback((worldAccessor, blockPos, dt) => SpawnEntities(worldAccessor, blockPos), pos, 50);
    }

    public override bool OnBlockBrokenWith(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel,
        float dropQuantityMultiplier, ref EnumHandling bhHandling)
    {
        if (!doDrops || byEntity is EntityPlayer entityPlayer && !HasRequiredTool(entityPlayer.Player, blockSel?.Position)) dropQuantityMultiplier = 0;
        return base.OnBlockBrokenWith(world, byEntity, itemslot, blockSel, dropQuantityMultiplier, ref bhHandling);
    }

    protected virtual void SpawnEntities(IWorldAccessor world, BlockPos pos)
    {
        if (spawnAll)
        {
            if (debugFlag) ExtraCore.Logger.Warning($"[BehaviorBreakSpawner][{block.Code}] Spawning all entities at {pos}");
            for (var i = 0; i < entityCodes.Length; i++)
            {
                if (world.Rand.NextDouble() < entityWeights[i]) SpawnEntity(world, pos, entityCodes[i]);
            }
            return;
        }
        SpawnEntity(world, pos, entityCodes.RandomElementByWeight(code => entityWeights[Array.IndexOf(entityCodes, code)]));
    }

    protected virtual void SpawnEntity(IWorldAccessor world, BlockPos pos, string entityCode)
    {
        EntityProperties entityType = world.GetEntityType(new AssetLocation(entityCode));
        if (entityType == null)
        {
            if (debugFlag) ExtraCore.Logger.Warning($"[BehaviorBreakSpawner][{block.Code}] Entity {entityCode} not found");
            return;
        }
        var entity1 = world.ClassRegistry.CreateEntity(entityType);
        entity1.ServerPos.SetPos(pos.X + 0.5 + spawnOffset.X, pos.Y + spawnOffset.Y, pos.Z + 0.5 + spawnOffset.X);
        world.SpawnEntity(entity1);
        if (debugFlag) ExtraCore.Logger.Warning($"[BehaviorBreakSpawner][{block.Code}] Spawned entity {entityCode} at {pos}");
    }
    
    protected bool HasRequiredTool(IPlayer byPlayer, BlockPos pos)
    {
        if (!requiresTool) return true;
        var activeToolCode = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack.Collectible.Code.ToString();
        if (WildcardUtil.Match(toolCode, activeToolCode)) return true;
        if (debugFlag) ExtraCore.Logger.Warning($"[BehaviorBreakSpawner][{block.Code}] Tool {toolCode} required to break block at {pos}");
        return false;
    }
}