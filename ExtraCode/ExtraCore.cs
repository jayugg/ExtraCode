using System.Linq;
using ExtraCode.BlockBehavior;
using ExtraCode.Items;
using JetBrains.Annotations;
using Vintagestory.API.Common;

#nullable enable

namespace ExtraCode;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class ExtraCore : ModSystem
{
    public static ILogger? Logger;
    public static string? ModId;
        
    public override void StartPre(ICoreAPI api)
    {
        Logger = Mod.Logger;
        ModId = Mod.Info.ModID;
    }
        
    public override void Start(ICoreAPI api)
    {
        api.RegisterBlockBehaviorClass("BreakSpawner", typeof(BlockBehaviorBreakSpawner));
        api.RegisterBlockBehaviorClass("InfestedBlock", typeof(BlockBehaviorInfested));
        api.RegisterBlockBehaviorClass("CustomDrinkSpeed", typeof(BlockBehaviorCustomDrinkSpeedContainer));
        api.RegisterBlockBehaviorClass("DropsWhenBrokenWith", typeof(BlockBehaviorDropsWhenBrokenWith));
        Logger?.Notification("Registered extra behaviors");
        api.RegisterItemClass("ItemMetalBloom", typeof(ItemMetalBloom));
        Logger?.Notification("Registered extra item classes");
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        base.AssetsFinalize(api);
        if (api.Side.IsClient()) return;
        var resolvedBlockCount = 0;
        foreach (var block in api.World.Blocks.Where(b => b?.Code != null))
        {
            if (!block.HasBehavior<BlockBehaviorDropsWhenBrokenWith>()) continue;
            block.GetBehavior<BlockBehaviorDropsWhenBrokenWith>().ResolveDrops(api.World);
            resolvedBlockCount++;
        }
        Logger?.Notification("Resolved drops for {0} blocks", resolvedBlockCount);
    }

    public override void Dispose()
    {
        Logger?.Notification("Disposing ExtraCore");
        Logger = null;
        ModId = null;
        base.Dispose();
    }
}