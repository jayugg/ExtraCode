using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ExtraCode.Behavior;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace ExtraCode;

[HarmonyPatch]
public class HarmonySystem : ModSystem
{
    public Harmony HarmonyInstance;
    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        HarmonyInstance = new Harmony(Mod.Info.ModID);
        HarmonyInstance.PatchAll();
    }
    
    public override void Dispose()
    {
        HarmonyInstance.UnpatchAll();
        base.Dispose();
    }
    
    [HarmonyTranspiler]
    [HarmonyPatch(typeof(BlockLiquidContainerBase), "tryEatStop")]
    public static IEnumerable<CodeInstruction> BlockLiquidContainerBase_tryEatStop_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        ExtraCore.Logger.Warning("Patching BlockLiquidContainerBase.tryEatStop");
        var codes = new List<CodeInstruction>(instructions);
        for (int i = 0; i < codes.Count; i++)
        {
            var codeInstruction = codes[i];

            // Look for the ldc.r4 opcode which loads a float32 (in this case, 1f) onto the evaluation stack
            if (codeInstruction.opcode == OpCodes.Ldc_R4 && Math.Abs((float)codeInstruction.operand - 1f) < 0.0001)
            {
                // Load the instance (this)
                codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                i++;

                // Call the method to get the drink speed
                codes.Insert(i, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonySystem), nameof(GetDrinkSpeed))));

                // Replace the operand with the result of the method call
                codeInstruction.opcode = OpCodes.Nop; // No operation, as the value is now on the stack
                break; // Only one occurrence needs to be changed
            }
        }
        return codes;
    }

    private static float GetDrinkSpeed(object instance)
    {
        // Replace with actual logic to determine drink speed based on the instance
        if (instance is not BlockLiquidContainerBase container) return 1f;
        if (!container.HasBehavior<BlockBehaviorCustomDrinkSpeedContainer>()) return 1f;
        var drinkSpeed = container.GetBehavior<BlockBehaviorCustomDrinkSpeedContainer>().DrinkSpeed;
        if (drinkSpeed > container.CapacityLitres) drinkSpeed = container.CapacityLitres;
        return drinkSpeed;
    }
}