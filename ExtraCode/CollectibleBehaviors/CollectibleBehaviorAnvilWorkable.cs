using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

#nullable enable

namespace ExtraCode.CollectibleBehaviors;

public class CollectibleBehaviorAnvilWorkable(CollectibleObject collObj) : CollectibleBehavior(collObj), IAnvilWorkable
{
    private ICoreAPI? Api { get; set; }
    private byte[,,] Voxels => HasExtraVoxels ? 
        GenVoxelsFromJsonPatternWithExtra(Pattern, Api?.World.Rand, ExtraVoxelChance) :
        GenVoxelsFromJsonPattern(Pattern);
    private string[][] Pattern { get; set; } = [];
    private bool HasExtraVoxels { get; set; }
    private float ExtraVoxelChance { get; set; } = 0.5f;
    
    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        HasExtraVoxels = properties["extraVoxels"].Exists && properties["extraVoxels"].AsBool();
        ExtraVoxelChance = properties["extraVoxelChance"].AsFloat(0.5f);
        var jsonPattern = properties["voxels"].AsArray();
        if (jsonPattern is { Length: > 0 })
        {
            Pattern = jsonPattern
                .Select(s => 
                    s.AsArray()
                        .Select(t => t.AsString())
                        .ToArray()
                ).ToArray();
        }
    }

    public override void OnLoaded(ICoreAPI api)
    {
        base.OnLoaded(api);
        Api = api;
        if (MaterialCount(Voxels) == 0)
        {
            Api.Logger.Error("CollectibleBehaviorAnvilWorkable for {0} has no voxels defined. Please check the 'voxels' attribute in the item JSON.", collObj.Code);
        }
    }

    public int GetRequiredAnvilTier(ItemStack stack) => 
        stack.ItemAttributes?["requiredAnvilTier"].AsInt() ?? 0;

    public List<SmithingRecipe> GetMatchingRecipes(ItemStack stack)
    {
        return Api.GetSmithingRecipes()
                .Where(r => r.Ingredient.SatisfiesAsIngredient(stack))
                .OrderBy(r => r.Output.ResolvedItemstack.Collectible.Code)
                .ToList()
            ;
    }

    public bool CanWork(ItemStack stack)
    {
        var temperature = stack.Collectible.GetTemperature(Api.World, stack);
        var meltingPoint = stack.Collectible.GetMeltingPoint(Api.World, null, new DummySlot(stack));
        if (stack.ItemAttributes?["workableTemperature"].Exists == true)
        {
            return stack.ItemAttributes["workableTemperature"].AsFloat(meltingPoint / 2) <= temperature;
        }
        return temperature >= meltingPoint / 2;
    }

    public ItemStack? TryPlaceOn(ItemStack stack, BlockEntityAnvil beAnvil)
    {
        // Already occupied anvil
        if (beAnvil.WorkItemStack != null) return null;
        if (stack.Attributes.HasAttribute("voxels"))
        {
            try
            {
                beAnvil.Voxels = BlockEntityAnvil.deserializeVoxels(stack.Attributes.GetBytes("voxels"));
                beAnvil.SelectedRecipeId = stack.Attributes.GetInt("selectedRecipeId");
            }
            catch (Exception)
            {
                beAnvil.Voxels = Voxels;
            }
        }
        else
        {
            beAnvil.Voxels = Voxels;
        }
        
        var workItemStack = stack.Clone();
        workItemStack.StackSize = 1;
        workItemStack.Collectible.SetTemperature(Api.World, workItemStack, stack.Collectible.GetTemperature(Api.World, stack));
        return workItemStack.Clone();
    }

    public ItemStack GetBaseMaterial(ItemStack stack) => stack;

    public EnumHelveWorkableMode GetHelveWorkableMode(ItemStack stack, BlockEntityAnvil beAnvil) =>
        EnumHelveWorkableMode.NotWorkable;

    // Only use always present voxels for handbook
    public int VoxelCountForHandbook(ItemStack stack) => MaterialCount(GenVoxelsFromJsonPattern(Pattern));
    
    /// <summary>
    /// Counts the number of voxels with material type 1 (full voxels).
    /// </summary>
    /// <param name="voxels">The 3D array of voxels.</param>
    public static int MaterialCount(byte[,,] voxels)
    {
        return voxels.Cast<byte>().Count(voxel => voxel == 1);
    }

    /// <summary>
    /// Generates voxels from a JSON pattern.
    /// The pattern is expected to be a 3D array of strings,
    /// where each string represents a layer of the recipe.
    /// Each character in the string can be:
    /// '#' for a full voxel,
    /// '*' for a slag voxel,
    /// '_' or any character for an empty voxel.
    /// The generated voxels will be centered in a 16x6x16 array.
    /// </summary>
    /// <param name="pattern">The JSON pattern to generate voxels from.</param>
    public static byte[,,] GenVoxelsFromJsonPattern(string[][] pattern)
        => GenVoxelsFromJsonPatternWithExtra(pattern, null, 0f);
    
    /// <summary>
    /// Generates voxels from a JSON pattern with extra voxel chance.
    /// The pattern is expected to be a 3D array of strings,
    /// where each string represents a layer of the recipe.
    /// Each character in the string can be:
    /// '#' for a full voxel,
    /// '*' for a slag voxel,
    /// 'o' for a random full voxel (with a chance defined by extraVoxelChance),
    /// 'x' for a random slag voxel (with a chance defined by extraVoxelChance),
    /// '_' or any character for an empty voxel.
    /// The generated voxels will be centered in a 16x6x16 array.
    /// </summary>
    /// <param name="pattern">The JSON pattern to generate voxels from.</param>
    /// <param name="rand">An optional random number generator. If null, a default one will be used.</param>
    /// <param name="extraVoxelChance">The chance of generating extra voxels (for 'o' and 'x' characters).</param>
    public static byte[,,] GenVoxelsFromJsonPatternWithExtra(string[][] pattern, Random? rand, float extraVoxelChance = 0.5f)
    {
        // Fallback if api is not available
        if (rand == null)
            extraVoxelChance = 0f;
        var voxels = new byte[16, 6, 16];
        var length = pattern[0][0].Length;
        var width = pattern[0].Length;
        var height = pattern.Length;
        // Center the recipe to the horizontal middle
        var startX = (16 - width) / 2;
        var startZ = (16 - length) / 2;
        for (var x = 0; x < Math.Min(width, 16); x++)
        {
            for (var y = 0; y < Math.Min(height, 6); y++)
            {
                for (var z = 0; z < Math.Min(length, 16); z++)
                {
                    var c = pattern[y][x][z];
                    byte b = c switch
                    {
                        '#' => 1,  // always full
                        '*' => 2,  // always slag
                        'o' => rand?.NextDouble() < extraVoxelChance ? (byte)1 : (byte)0,  // random full
                        'x' => rand?.NextDouble() < extraVoxelChance ? (byte)2 : (byte)0,  // random slag
                         _  => 0 // empty (_ or space or anything else)
                    };
                    voxels[z + startZ, y, x + startX] = b;
                }
            }
        }
        return voxels;
    }
}