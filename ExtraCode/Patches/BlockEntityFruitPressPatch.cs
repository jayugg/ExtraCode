using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Vintagestory.GameContent;

namespace ExtraCode.Patches;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[HarmonyPatch(typeof(BlockEntityFruitPress))]
public class BlockEntityFruitPressPatch
{
    private static MethodInfo CapacityLitresGetter =>
        AccessTools.PropertyGetter(typeof(BlockLiquidContainerBase), nameof(BlockLiquidContainerBase.CapacityLitres));
    private static MethodInfo MaxContainerCapacityLitresGetter =>
        AccessTools.Method(typeof(BlockEntityFruitPressPatch), nameof(GetMaxContainerCapacityLitres));
    public static float GetMaxContainerCapacityLitres(BlockEntityFruitPress beFruitPress)
    {
        return beFruitPress.Block.Attributes["maxContainerCapacityLitres"]?.AsFloat(20f) ?? 20f;
    }
    
    [HarmonyTranspiler, HarmonyPatch("InteractGround")]
    public static IEnumerable<CodeInstruction> InteractGround_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // Look for the IL code that compares capacity litres to 20 and replace it with a call to GetMaxContainerCapacityLitres
        var codes = instructions.ToList();
        for (var i = 0; i < codes.Count; i++)
        {
            var prevCode = i > 0 ? codes[i - 1] : null;
            var code = codes[i];
            if (prevCode == null)
                continue;
            if (prevCode.opcode == OpCodes.Callvirt &&
                prevCode.operand is MethodInfo methodInfo &&
                methodInfo == CapacityLitresGetter &&
                code.opcode == OpCodes.Ldc_R4 &&
                code.operand is float maxContainerCapacityLitres &&
                Math.Abs(maxContainerCapacityLitres - 20f) < 1e-5)
            {
                // Replace Ldc_R4 20 with a call to GetMaxContainerCapacityLitres
                yield return new CodeInstruction(OpCodes.Call, MaxContainerCapacityLitresGetter);
            }
            yield return code;
        }
    }
}