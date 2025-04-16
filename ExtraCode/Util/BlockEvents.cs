using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace ExtraCode.Util;

public static class BlockEvents
{
    private static readonly Dictionary<BlockPos, (IPlayer player, string blockCode)> BlockBreakingInfo = new();

    public static void RaiseBlockBreaking(BlockPos pos, IPlayer player, string blockCode)
    {
        BlockBreakingInfo[pos] = (player, blockCode);
    }

    public static (IPlayer player, string blockCode)? GetBlockBreakingInfo(BlockPos pos)
    {
        return BlockBreakingInfo.TryGetValue(pos, out var info) ? info : null;
    }

    public static void ClearBlockBreakingInfo(BlockPos pos)
    {
        BlockBreakingInfo.Remove(pos);
    }
}