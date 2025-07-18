using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace ExtraCode.BlockBehaviors;

public class BlockBehaviorInfested(Block block) : BlockBehaviorBreakSpawner(block)
{
    protected int BreakDelay;
    private string TriggerBlockSelector { get; set; }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        TriggerBlockSelector = properties["triggerBlockSelector"].AsString(block.Code.ToString());
        BreakDelay = properties["breakDelay"].AsInt(150);
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
    {
        base.OnBlockBroken(world, pos, byPlayer, ref handling);
        if (!HasRequiredTool(byPlayer, pos)) return;
        handling = EnumHandling.Handled;

        // Trigger neighboring blocks to break with a delay
        foreach (var facing in BlockFacing.ALLFACES)
        {
            var neighborPos = pos.AddCopy(facing);
            world.RegisterCallbackUnique((worldAccessor, blockPos, dt) => BreakNeighborBlock(worldAccessor, blockPos, byPlayer), neighborPos, BreakDelay);
        }
    }
    
    protected virtual void BreakNeighborBlock(IWorldAccessor world, BlockPos pos, IPlayer byPlayer)
    {
        var neighborBlock = world.BlockAccessor.GetBlock(pos);
        if (neighborBlock.GetBehavior(typeof(BlockBehaviorInfested), withInheritance: true) is not BlockBehaviorInfested bh) return;
        //Debug wildcard match
        if (DebugFlag) ExtraCore.Logger?.Warning($"[BehaviorInfested][{neighborBlock.Code}] TriggerSelector: {bh.TriggerBlockSelector} TriggerBlock: {neighborBlock.Code}");
        if (neighborBlock.Code == null || !WildcardUtil.Match(bh.TriggerBlockSelector, block.Code.ToString())) return;
        if (DebugFlag) ExtraCore.Logger?.Warning($"[BehaviorInfested][{neighborBlock.Code}] Breaking neighbor block at {pos}");
        world.BlockAccessor.BreakBlock(pos, byPlayer);
    }
}