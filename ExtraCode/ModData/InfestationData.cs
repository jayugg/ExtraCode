using System.Collections.Generic;
using JetBrains.Annotations;
using ProtoBuf;
using Vintagestory.API.MathTools;

namespace ExtraCode.ModData;

[ProtoContract]
public class InfestationData
{
    [ProtoMember(1)]
    private readonly Dictionary<BlockPos, bool> _data = new();
    
    public bool IsInfested(BlockPos pos)
    {
        _data.TryGetValue(pos, out var infested);
        return infested;
    }
    
    public void SetInfested(BlockPos pos, bool infested)
    {
        if (infested)
            _data[pos] = true;
        else
            _data.Remove(pos);
    }
}