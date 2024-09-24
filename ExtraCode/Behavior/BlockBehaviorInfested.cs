using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace ExtraCode.Behavior;

public class BlockBehaviorInfested : BlockBehaviorBreakSpawner
{
    private string triggerBlockSelector;
    private int breakDelay;
    public string TriggerBlockSelector => triggerBlockSelector;
    
    public BlockBehaviorInfested(Block block) : base(block)
    {
    }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        this.triggerBlockSelector = properties["triggerBlockSelector"].AsString(this.block.Code.ToString());
        this.breakDelay = properties["breakDelay"].AsInt(150);
    }

    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
    {
        base.OnBlockBroken(world, pos, byPlayer, ref handling);
        if (!HasRequiredTool(byPlayer, pos)) return;
        handling = EnumHandling.Handled;

        // Trigger neighboring blocks to break with a delay
        foreach (BlockFacing facing in BlockFacing.ALLFACES)
        {
            BlockPos neighborPos = pos.AddCopy(facing);
            world.RegisterCallbackUnique((worldAccessor, blockPos, dt) => BreakNeighborBlock(worldAccessor, blockPos, byPlayer), neighborPos, breakDelay);
        }
    }
    
    private void BreakNeighborBlock(IWorldAccessor world, BlockPos pos, IPlayer byPlayer)
    {
        Block neibBlock = world.BlockAccessor.GetBlock(pos);
        var bh = neibBlock.GetBehavior<BlockBehaviorInfested>();
        if (bh == null) return;
        //Debug wildcard match
        if (debugFlag) ExtraCore.Logger.Warning($"[BehaviorInfested][{neibBlock.Code}] TriggerSelector: {bh.TriggerBlockSelector} TriggerBlock: {neibBlock.Code}");
        if (neibBlock.Code == null || !WildcardUtil.Match(bh.TriggerBlockSelector, block.Code.ToString())) return;
        if (debugFlag) ExtraCore.Logger.Warning($"[BehaviorInfested][{neibBlock.Code}] Breaking neighbor block at {pos}");
        world.BlockAccessor.BreakBlock(pos, byPlayer);
    }
}