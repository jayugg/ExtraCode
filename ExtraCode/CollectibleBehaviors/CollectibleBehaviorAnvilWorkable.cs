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
    private byte[,,] Voxels => GenVoxelsFromJsonPattern(JsonPattern);
    private string[][] JsonPattern { get; set; } = [];
    
    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        try
        {
            JsonPattern = properties["voxels"].AsArray()
                .Select(s => 
                    s.AsArray()
                        .Select(t => t.AsString())
                        .ToArray()
                ).ToArray();
        }
        catch (Exception)
        {
            // ignored
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

    public int VoxelCountForHandbook(ItemStack stack) => MaterialCount(Voxels);
    
    /// <summary>
    /// Generates voxels from a JSON pattern.
    /// The pattern is expected to be a 3D array of strings,
    /// where each string represents a layer of the recipe.
    /// Each character in the string can be:
    /// '#' for a full voxel,
    /// 's' for a slag voxel,
    /// '_' or ' ' for an empty voxel.
    /// The generated voxels will be centered in a 16x6x16 array.
    /// </summary>
    public static byte[,,] GenVoxelsFromJsonPattern(string[][] pattern)
    {
        var voxels = new byte[16, 6, 16];
        var length = pattern[0][0].Length;
        var width = pattern[0].Length;
        var height = pattern.Length;
        // We'll center the recipe to the horizontal middle
        var startX = (16 - width) / 2;
        var startZ = (16 - length) / 2;
        for (var x = 0; x < Math.Min(width, 16); x++)
        {
            for (var y = 0; y < Math.Min(height, 6); y++)
            {
                for (var z = 0; z < Math.Min(length, 16); z++)
                {
                    voxels[z + startZ, y, x + startX] =
                        pattern[y][x][z] == '#'                 // full
                            ? (byte)1
                            : pattern[y][x][z] == '*'           // slag
                                ? (byte)2                     
                                : (byte)0;                      // empty (_ or space)
                }
            }
        }
        return voxels;
    }
    
    /// <summary>
    /// Counts the number of voxels with material type 1 (full voxels).
    /// </summary>
    public static int MaterialCount(byte[,,] voxels)
    {
        return voxels.Cast<byte>().Count(voxel => voxel == 1);
    }
}