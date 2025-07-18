using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using ProtoBuf;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace ExtraCode.ModData;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class ChunkDataSystem : ModSystem
{
    private const string ModDataKey = "ec:chunkdata";
    private readonly ConditionalWeakTable<IServerChunk, InfestationData> _chunkInfestedDataCache = new();
    
    public void SetInfested(ICoreServerAPI api, BlockPos pos, bool infested)
    {
        var data = GetOrCreateInfestationData(api, pos);
        data.SetInfested(pos, infested);
    }
    
    public InfestationData GetOrCreateInfestationData(ICoreServerAPI api, BlockPos pos)
    {
        var chunk = api.WorldManager.GetChunk(pos);
        return chunk == null ? null : GetOrCreateInfestationData(chunk);
    }

    private InfestationData GetOrCreateInfestationData(IServerChunk chunk, bool create = true)
    {
        if (_chunkInfestedDataCache.TryGetValue(chunk, out var cachedData))
            return cachedData;
        var serializedData = chunk.GetServerModdata(ModDataKey);
        var prospectingData = serializedData != null 
            ? SerializerUtil.Deserialize<InfestationData>(serializedData) 
            : create ? new InfestationData() : null;
        if (prospectingData == null) return null;
        _chunkInfestedDataCache.Add(chunk, prospectingData);
        if (chunk.LiveModData.TryGetValue(ModDataKey, out var serializerObj) && serializerObj is SerializationCallback serializer)
            serializer.OnSerialization += c => c.SetServerModdata(ModDataKey, SerializerUtil.Serialize(prospectingData));
        else
            chunk.LiveModData[ModDataKey] = new SerializationCallback(chunk)
            {
                OnSerialization = c => c.SetServerModdata(ModDataKey, SerializerUtil.Serialize(prospectingData))
            };
        return prospectingData;
    }

    public bool IsInfested(ICoreServerAPI api, BlockPos pos)
    {
        var chunk = api.WorldManager.GetChunk(pos);
        var data = chunk == null ? null : GetOrCreateInfestationData(chunk, false);
        return data?.IsInfested(pos) ?? false;
    }
    
    [ProtoContract]
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SerializationCallback(IServerChunk chunk)
    {
        public delegate void OnSerializationDelegate(IServerChunk chunk);

        public OnSerializationDelegate OnSerialization;

        [ProtoBeforeSerialization]
        private void BeforeSerialization()
        {
            OnSerialization(chunk);
        }
    }
}