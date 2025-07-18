using System;
using System.Reflection;
using HarmonyLib;
using Vintagestory.GameContent;

namespace ExtraCode.Patches.GetCollectibleInterfacePatches;

public static class AnvilWorkablePatches
{
    public static void Patch(Harmony harmony)
    {
        var patchIsinstIAnvilWorkable = 
            AccessTools.Method(typeof(GetCollectibleInterface), nameof(GetCollectibleInterface.PatchIsinstCollectible))
                .MakeGenericMethod(typeof(IAnvilWorkable));
        
        ManualTranspiler(typeof(BlockEntityAnvil), "get_CanWorkCurrent");
        ManualTranspiler(typeof(BlockEntityAnvil), "TryPut");
        ManualTranspiler(typeof(BlockEntityAnvil), "PrintDebugText");
        ManualTranspiler(typeof(BlockEntityAnvil), nameof(BlockEntityAnvil.OnHelveHammerHit));
        ManualTranspiler(typeof(BlockEntityAnvil), nameof(BlockEntityAnvil.ditchWorkItemStack));
        ManualTranspiler(typeof(BlockEntityAnvil), "OpenDialog");
        ManualTranspiler(
            typeof(BlockAnvil).GetNestedType("<>c__DisplayClass1_0", BindingFlags.NonPublic),
            "<OnLoaded>b__6"
            );
        return;

        void ManualTranspiler(Type type, string methodName)
        {
            var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (method != null)
                harmony.Patch(method, transpiler: new HarmonyMethod(patchIsinstIAnvilWorkable));
        }
    }
}