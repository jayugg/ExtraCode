using System.Linq;
using ExtraCode.Behavior;
using Vintagestory.API.Common;

[assembly: ModInfo(name: "ExtraCode", modID: "extracode", Side = "Universal", Version = "1.1.0", Authors = new[] { "jayugg" },
    Description = "Extra class and behaviors for content modders ")]

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
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            base.AssetsFinalize(api);
            if (api.Side.IsClient()) return;
            foreach (var block in api.World.Blocks.Where(b => b?.Code != null))
            {
                if (!block.HasBehavior<BlockBehaviorDropsWhenBrokenWith>()) continue;
                block.GetBehavior<BlockBehaviorDropsWhenBrokenWith>().ResolveDrops(api.World);
            }
        }
    }
}