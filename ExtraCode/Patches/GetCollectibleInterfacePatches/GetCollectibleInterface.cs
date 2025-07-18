using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using Vintagestory.API.Common;
using Harmony = HarmonyLib.Harmony;

namespace ExtraCode.Patches.GetCollectibleInterfacePatches;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class GetCollectibleInterface : ModSystem
{
    private static Harmony _harmony;
    
    public override void Start(ICoreAPI api)
    {
        base.Start(api);
        _harmony = new Harmony($"{Mod.Info.ModID}.GetCollectibleInterface");
        AnvilWorkablePatches.Patch(_harmony);
    }
    
    private static MethodInfo GetCollectibleInterfaceMethod<T>() where T : class
    {
        return AccessTools.Method(
            typeof(CollectibleObject),
            "GetCollectibleInterface"
        ).MakeGenericMethod(typeof(T));
    }
    
    public static IEnumerable<CodeInstruction> PatchIsinstCollectible<T>(IEnumerable<CodeInstruction> instructions) where T : class
    {
        foreach (var code in instructions)
        {
            // When encountering the isinst check for interface T,
            // insert a call to GetCollectibleInterface<T> before continuing.
            if (code.opcode == OpCodes.Isinst &&
                code.operand is Type typeOperand &&
                typeOperand == typeof(T))
                yield return new CodeInstruction(OpCodes.Call, GetCollectibleInterfaceMethod<T>());
            yield return code;
        }
    }
    
    public override void Dispose()
    {
        _harmony.UnpatchAll();
        _harmony = null;
    }
}