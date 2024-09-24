using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace ExtraCode.Util;

public static class BlockEvents
{
    private static readonly Dictionary<BlockPos, (IPlayer player, string blockCode)> blockBreakingInfo = new();

    public static void RaiseBlockBreaking(BlockPos pos, IPlayer player, string blockCode)
    {
        blockBreakingInfo[pos] = (player, blockCode);
    }

    public static (IPlayer player, string blockCode)? GetBlockBreakingInfo(BlockPos pos)
    {
        return blockBreakingInfo.TryGetValue(pos, out var info) ? info : null;
    }

    public static void ClearBlockBreakingInfo(BlockPos pos)
    {
        blockBreakingInfo.Remove(pos);
    }
}