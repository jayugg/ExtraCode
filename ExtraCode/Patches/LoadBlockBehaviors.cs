using HarmonyLib;
using JetBrains.Annotations;
using Vintagestory.API.Common;

namespace ExtraCode.Patches;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[HarmonyPatch(typeof(Block), nameof(Block.OnLoaded))]
public class LoadBlockBehaviors
{
    [HarmonyPostfix]
    public static void OnLoaded_Postfix(Block __instance, ICoreAPI api)
    {
        foreach (var blockBehavior in __instance.BlockBehaviors)
            blockBehavior.OnLoaded(api);
    }
}