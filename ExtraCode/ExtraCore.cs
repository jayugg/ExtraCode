using System.Linq;
using ExtraCode.Behavior;
using ExtraCode.Items;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace ExtraCode
{
    public class ExtraCore : ModSystem
    {

        public static ILogger Logger;
        public static string ModId;
        
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
            Logger.Notification("Registered extra behaviors");
            api.RegisterItemClass("ItemMetalBloom", typeof(ItemMetalBloom));
            Logger.Notification("Registered extra item classes");
        }

        public override void AssetsLoaded(ICoreAPI api)
        {
            var scSystem = api.ModLoader.GetModSystem<SurvivalCoreSystem>();
            var propertyAssets = api.Assets.GetMany("worldproperties/block/metal.json");
            var metalProperties = propertyAssets.Select(asset => asset.ToObject<MetalProperty>()).ToList();
            foreach (var metalProperty in metalProperties)
            {
                for (int index = 0; index < metalProperty.Variants.Length; ++index)
                {
                    MetalPropertyVariant variant = metalProperty.Variants[index];
                    scSystem.metalsByCode[variant.Code.Path] = variant;
                }
            }
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
            Logger.Notification("Resolved drops for {0} blocks", resolvedBlockCount);
        }
    }
}