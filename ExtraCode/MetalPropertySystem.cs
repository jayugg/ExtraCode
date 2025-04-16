using System.Linq;
using JetBrains.Annotations;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace ExtraCode;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class MetalPropertySystem : ModSystem
{
    // Since 1.20, mods can add metal properties in their own domains,
    // but the metal properties in the game domain still cannot be json patched because they normally load after json patches.
    public override void AssetsLoaded(ICoreAPI api)
    {
        var scSystem = api.ModLoader.GetModSystem<SurvivalCoreSystem>();
        var propertyAssets = api.Assets.GetMany("worldproperties/block/metal.json");
        var metalProperties = propertyAssets.Select(asset => asset.ToObject<MetalProperty>()).ToList();
        foreach (var variant in metalProperties.SelectMany(metalProperty => metalProperty.Variants))
        {
            scSystem.metalsByCode[variant.Code.Path] = variant;
        }
    }
}