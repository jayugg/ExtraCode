using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;

namespace ExtraCode.Deposits;

public class SphericalChildDepositGenerator(
    ICoreServerAPI api,
    DepositVariant variant,
    LCGRandom depositRand,
    NormalizedSimplexNoise noiseGen) : DiscDepositGenerator(api, variant, depositRand, noiseGen)
{
    [JsonProperty]
    public NatFloat RandomTries;

    public override void Init() {}

    public override void GetYMinMax(BlockPos pos, out double miny, out double maxy)
    {
        variant.GetParentDeposit().GeneratorInst.GetYMinMax(pos, out miny, out maxy);
    }

    public void ResolveAdd(Block inBlock, string key, string value)
    {
        placeBlockByInBlockId[inBlock.BlockId] = PlaceBlock.Resolve(variant.fromFile, Api, inBlock, key, value);
        if (SurfaceBlock != null)
        {
            surfaceBlockByInBlockId[inBlock.BlockId] = SurfaceBlock.Resolve(variant.fromFile, Api, inBlock, key, value);
        }
    }

    public override void GenDeposit(
    IBlockAccessor blockAccessor,
    IServerChunk[] chunks,
    int originChunkX,
    int originChunkZ,
    BlockPos pos,
    ref Dictionary<BlockPos, DepositVariant> subDepositsToPlace)
    {
        var currentMapChunk = chunks[0].MapChunk;
        var sampleRadius = Math.Min(64, (int)Radius.nextFloat(1f, DepositRand));
        if (sampleRadius <= 0)
            return;
        
        // Increase radius by one to get proper bounds for the deposit
        var depositBoundary = sampleRadius + 1;
        var blockVariantIndex = PlaceBlock.AllowedVariants != null ? DepositRand.NextInt(PlaceBlock.AllowedVariants.Length) : 0;
        var enableSurfacePlacement = DepositRand.NextFloat() > 0.35 && SurfaceBlock != null;
        var depositIterations = RandomTries.nextFloat(1f, DepositRand);

        for (var i = 0; i < (double)depositIterations; i++)
        {
            // Select a random center within the deposit boundary
            targetPos.Set(
                pos.X + DepositRand.NextInt(2 * depositBoundary + 1) - depositBoundary,
                pos.Y + DepositRand.NextInt(2 * depositBoundary + 1) - depositBoundary,
                pos.Z + DepositRand.NextInt(2 * depositBoundary + 1) - depositBoundary
            );
            
            // Iterate through every point in a cube that encloses the sphere
            for (var dx = -sampleRadius; dx <= sampleRadius; dx++)
            {
                for (var dy = -sampleRadius; dy <= sampleRadius; dy++)
                {
                    for (var dz = -sampleRadius; dz <= sampleRadius; dz++)
                    {
                        // Check if point lies within the sphere volume
                        if (dx * dx + dy * dy + dz * dz > sampleRadius * sampleRadius)
                            continue;
                        
                        var blockX = targetPos.X + dx;
                        var blockY = targetPos.Y + dy;
                        var blockZ = targetPos.Z + dz;
                        
                        // Validate Y bounds and chunk-local X/Z bounds
                        if (blockY <= 1 || blockY >= worldheight)
                            continue;
                        
                        var localX = blockX % 32;
                        var localZ = blockZ % 32;
                        if (localX < 0 || localZ < 0 || localX >= 32 || localZ >= 32)
                            continue;
                        
                        // Calculate the index in the chunk's block container
                        var blockIndex = (blockY % 32 * 32 + localZ) * 32 + localX;
                        var blockId = chunks[blockY / 32].Data.GetBlockIdUnsafe(blockIndex);
                        
                        if (!placeBlockByInBlockId.TryGetValue(blockId, out var resolvedBlock))
                            continue;
                        
                        var selectedBlock = resolvedBlock.Blocks[blockVariantIndex];
                        if (variant.WithBlockCallback)
                            selectedBlock.TryPlaceBlockForWorldGen(blockAccessor, new BlockPos(blockX, blockY, blockZ), BlockFacing.UP, DepositRand);
                        else
                            chunks[blockY / 32].Data[blockIndex] = selectedBlock.BlockId;
                        
                        // Optional surface placement
                        if (!enableSurfacePlacement) continue;
                        var rainHeight = Math.Min(currentMapChunk.RainHeightMap[localZ * 32 + localX], Api.World.BlockAccessor.MapSizeY - 2);
                        var surfaceChance = SurfaceBlockChance * Math.Max(0f, 1f - (rainHeight - blockY) / 8f);
                        if (rainHeight >= worldheight ||
                            !(DepositRand.NextFloat() < surfaceChance) ||
                            !Api.World.Blocks[chunks[rainHeight / 32].Data
                                .GetBlockIdUnsafe((rainHeight % 32 * 32 + localZ) * 32 + localX)]
                                .SideSolid[BlockFacing.UP.Index])
                            continue;
                        var surfaceIndex = ((rainHeight + 1) % 32 * 32 + localZ) * 32 + localX;
                        var surfaceData = chunks[(rainHeight + 1) / 32].Data;
                        if (surfaceData.GetBlockIdUnsafe(surfaceIndex) == 0)
                            surfaceData[surfaceIndex] = surfaceBlockByInBlockId[blockId].Blocks[0].BlockId;
                    }
                }
            }
        }
    }

    protected override void beforeGenDeposit(IMapChunk heremapchunk, BlockPos pos) {}
    protected override void loadYPosAndThickness(IMapChunk heremapchunk, int lx, int lz, BlockPos targetBlockPos, double distanceToEdge) {}
}
