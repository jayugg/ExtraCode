using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace ExtraCode.BlockBehavior;

public class BlockBehaviorInfested(Block block) : BlockBehaviorBreakSpawner(block)
{
    private int _breakDelay;
    public string TriggerBlockSelector { get; private set; }

    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        TriggerBlockSelector = properties["triggerBlockSelector"].AsString(block.Code.ToString());
        _breakDelay = properties["breakDelay"].AsInt(150);
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
            world.RegisterCallbackUnique((worldAccessor, blockPos, dt) => BreakNeighborBlock(worldAccessor, blockPos, byPlayer), neighborPos, _breakDelay);
        }
    }
    
    private void BreakNeighborBlock(IWorldAccessor world, BlockPos pos, IPlayer byPlayer)
    {
        var neibBlock = world.BlockAccessor.GetBlock(pos);
        var bh = neibBlock.GetBehavior<BlockBehaviorInfested>();
        if (bh == null) return;
        //Debug wildcard match
        if (DebugFlag) ExtraCore.Logger?.Warning($"[BehaviorInfested][{neibBlock.Code}] TriggerSelector: {bh.TriggerBlockSelector} TriggerBlock: {neibBlock.Code}");
        if (neibBlock.Code == null || !WildcardUtil.Match(bh.TriggerBlockSelector, block.Code.ToString())) return;
        if (DebugFlag) ExtraCore.Logger?.Warning($"[BehaviorInfested][{neibBlock.Code}] Breaking neighbor block at {pos}");
        world.BlockAccessor.BreakBlock(pos, byPlayer);
    }
}