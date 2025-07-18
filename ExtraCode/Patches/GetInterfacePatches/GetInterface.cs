using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Vintagestory.API.Common;

namespace ExtraCode.Patches.GetInterfacePatches;

public static class GetInterface
{
    private static MethodInfo BlockEntityPosGetterMethod => AccessTools.PropertyGetter(typeof(BlockEntity), "Pos");
    // ReSharper disable once InconsistentNaming
    private static MethodInfo ICoreApiWorldGetterMethod => AccessTools.PropertyGetter(typeof(ICoreAPI), "World");
    private static MethodInfo BlockEntityApiGetterMethod => AccessTools.PropertyGetter(typeof(BlockEntity), "Api");
    private static FieldInfo BeBehaviourBlockEntityField => AccessTools.Field(typeof(BlockEntityBehavior), "Blockentity");
    private static FieldInfo BeBehaviourApiField => AccessTools.Field(typeof(BlockEntityBehavior), "Api");
    
    private static MethodInfo GetInterfaceMethod<T>() where T : class
    {
        return AccessTools.Method(
            typeof(Block),
            "GetInterface"
        ).MakeGenericMethod(typeof(T));
    }
    
    public static IEnumerable<CodeInstruction> PatchIsinstFromBlockEntityBehavior<T>(IEnumerable<CodeInstruction> instructions) where T : class
    {
        foreach (var code in instructions)
        {
            if (code.opcode == OpCodes.Isinst &&
                code.operand is System.Type typeOperand &&
                typeOperand == typeof(T))
            {
                // Load this.Api.World
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Castclass, typeof(BlockEntityBehavior));
                yield return new CodeInstruction(OpCodes.Ldfld, BeBehaviourApiField);
                yield return new CodeInstruction(OpCodes.Callvirt, ICoreApiWorldGetterMethod);
                yield return new CodeInstruction(OpCodes.Ldarg_1); // load pos
                // Call the method: GetInterface<T>(IWorldAccessor world, BlockPos pos)
                yield return new CodeInstruction(OpCodes.Call, GetInterfaceMethod<T>());
            }
            yield return code;
        }
    }
    
    public static IEnumerable<CodeInstruction> PatchIsinstFromBlockEntity<T>(IEnumerable<CodeInstruction> instructions) where T : class
    {
        foreach (var code in instructions)
        {
            if (code.opcode == OpCodes.Isinst &&
                code.operand is System.Type typeOperand &&
                typeOperand == typeof(T))
            {
                // Load blockentity.Api.World
                yield return new CodeInstruction(OpCodes.Ldarg_0); // load blockentity
                yield return new CodeInstruction(OpCodes.Callvirt,
                    BlockEntityApiGetterMethod);
                yield return new CodeInstruction(OpCodes.Callvirt,
                    ICoreApiWorldGetterMethod);
                // Load blockentity.Pos
                yield return new CodeInstruction(OpCodes.Ldarg_0); // load blockentity
                yield return new CodeInstruction(OpCodes.Callvirt,
                    BlockEntityPosGetterMethod);
                // Call the method: GetInterface<T>(IWorldAccessor world, BlockPos pos)
                yield return new CodeInstruction(OpCodes.Call, GetInterfaceMethod<T>());
            }
            yield return code;
        }
    }
}