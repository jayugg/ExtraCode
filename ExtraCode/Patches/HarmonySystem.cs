using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using ExtraCode.BlockBehavior;
using HarmonyLib;
using JetBrains.Annotations;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ExtraCode.Patches;

#nullable enable
[HarmonyPatch]
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class HarmonySystem : ModSystem
{
    private static Harmony? _harmonyInstance;
    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        _harmonyInstance = new Harmony(Mod.Info.ModID);
        _harmonyInstance.PatchAll();
    }
    
    public override void Dispose()
    {
        _harmonyInstance?.UnpatchAll();
        _harmonyInstance = null;
        base.Dispose();
    }
    
    [HarmonyPostfix, HarmonyPatch(typeof(ItemIngot), "OnLoaded")]
    public static void ItemIngot_OnLoaded_Patch(ItemIngot __instance, ref bool ___isBlisterSteel, ICoreAPI api)
    {
        var blisterSteelLike = __instance.Attributes?["blisterSteelLike"]?.AsBool() ?? false;
        if (!blisterSteelLike)
            return;
        ___isBlisterSteel = blisterSteelLike;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(ItemWorkItem), "OnLoaded")]
    public static void ItemWorkItem_OnLoaded_Patch(ItemWorkItem __instance, ICoreAPI api)
    {
        var blisterSteelLike = __instance.Attributes?["blisterSteelLike"]?.AsBool() ?? false;
        var smeltedStack = __instance.CombustibleProps?.SmeltedStack?.ResolvedItemstack;
        var smeltedStackBlisterSteelLike = smeltedStack?.Attributes?["blisterSteelLike"]?.ToJsonToken()?.ToBool() ?? false;
        if (!(blisterSteelLike && smeltedStackBlisterSteelLike))
            return;
        __instance.isBlisterSteel = true;
    }
    
    [HarmonyTranspiler, HarmonyPatch(typeof(BlockLiquidContainerBase), "tryEatStop")]
    public static IEnumerable<CodeInstruction> BlockLiquidContainerBase_tryEatStop_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        ExtraCore.Logger?.Warning("Patching BlockLiquidContainerBase.tryEatStop");
        var codes = new List<CodeInstruction>(instructions);
        for (var i = 0; i < codes.Count; i++)
        {
            var codeInstruction = codes[i];
            // Look for the ldc.r4 opcode which loads a float32 (in this case, 1f) onto the evaluation stack
            if (codeInstruction.opcode != OpCodes.Ldc_R4 ||
                !(Math.Abs((float)codeInstruction.operand - 1f) < 0.0001)) continue;
            // Load the instance (this)
            codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
            i++;
            // Call the method to get the drink speed
            codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonySystem), nameof(GetDrinkSpeed))));
            // Replace the operand with the result of the method call
            codeInstruction.opcode = OpCodes.Nop; // No operation, as the value is now on the stack
            break; // Only one occurrence needs to be changed
        }
        return codes;
    }

    private static float GetDrinkSpeed(object instance)
    {
        if (instance is not BlockLiquidContainerBase container) return 1f;
        if (!container.HasBehavior<BlockBehaviorCustomDrinkSpeedContainer>()) return 1f;
        var drinkSpeed = container.GetBehavior<BlockBehaviorCustomDrinkSpeedContainer>().DrinkSpeed;
        if (drinkSpeed > container.CapacityLitres) drinkSpeed = container.CapacityLitres;
        return drinkSpeed;
    }
}