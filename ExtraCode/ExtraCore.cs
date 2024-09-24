using ExtraCode.Behavior;
using Vintagestory.API.Common;

[assembly: ModInfo(name: "ExtraCode", modID: "extracode", Side = "Universal", Version = "1.0.0", Authors = new string[] { "jayugg" },
    Description = "Extra class and behaviors for content modders ")]

namespace ExtraCode
{
    public class ExtraCore : ModSystem
    {

        public static ILogger Logger;
        
        public override void StartPre(ICoreAPI api)
        {
            Logger = Mod.Logger;
        }
        
        public override void Start(ICoreAPI api)
        {
            api.RegisterBlockBehaviorClass("BreakSpawner", typeof(BlockBehaviorBreakSpawner));
            api.RegisterBlockBehaviorClass("InfestedBlock", typeof(BlockBehaviorInfested));
            Logger.Notification("Registered extra behaviors");
        }
    }
}