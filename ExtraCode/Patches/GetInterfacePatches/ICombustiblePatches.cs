using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace ExtraCode.Patches.GetInterfacePatches;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[HarmonyPatch]
public class CombustiblePatches
{
    /*
    [HarmonyTranspiler, HarmonyPatch(typeof(BEBehaviorBurning), "getBurnDuration")]
    public static IEnumerable<CodeInstruction> getBurnDuration_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return GetInterface.PatchIsinstFromBlockEntityBehavior<ICombustible>(instructions);
    }
    
    [HarmonyTranspiler, HarmonyPatch(typeof(BlockEntityCharcoalPit), "IsCombustible")]
    public static IEnumerable<CodeInstruction> IsCombustible_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return GetInterface.PatchIsinstFromBlockEntity<ICombustible>(instructions);
    }
    */
}