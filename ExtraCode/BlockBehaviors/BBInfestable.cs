using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace ExtraCode.BlockBehaviors;

public class BlockBehaviorInfestable(Block block) : BlockBehaviorInfested(block)
{
    private float InfestedChance { get; set; }
    
    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        InfestedChance = properties["infestedChance"].AsFloat(1);
    }
    
    public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, ref EnumHandling handling)
    {
        if (!(world.Rand.NextSingle() < InfestedChance))
        {
            handling = EnumHandling.PassThrough;
            return;
        }
        
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
}