using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace ExtraCode.Items;

public class ItemMetalBloom : Item, IAnvilWorkable
{
  public override string GetHeldItemName(ItemStack itemStack)
    {
      return itemStack.Attributes.HasAttribute("voxels") ? Lang.Get($"Partially worked {base.GetHeldItemName(itemStack)}", Array.Empty<object>()) : base.GetHeldItemName(itemStack);
    }

    public int GetRequiredAnvilTier(ItemStack stack)
    {
      var defaultValue = 0;
      var attributes = stack.Collectible.Attributes;
      if ((attributes != null ? (attributes["requiresAnvilTier"].Exists ? 1 : 0) : 0) != 0)
        defaultValue = stack.Collectible.Attributes["requiresAnvilTier"]?.AsInt(defaultValue) ?? defaultValue;
      return defaultValue;
    }

    public List<SmithingRecipe> GetMatchingRecipes(ItemStack stack)
    {
      return api.GetSmithingRecipes().Where((System.Func<SmithingRecipe, bool>) (r => r.Ingredient.SatisfiesAsIngredient(stack))).OrderBy((System.Func<SmithingRecipe, AssetLocation>) (r => r.Output.ResolvedItemstack.Collectible.Code)).ToList();
    }

    public bool CanWork(ItemStack stack)
    {
      var temperature = stack.Collectible.GetTemperature(api.World, stack);
      var meltingPoint = stack.Collectible.GetMeltingPoint(api.World, null, new DummySlot(stack));
      var attributes = stack.Collectible.Attributes;
      return (attributes != null ? (attributes["workableTemperature"].Exists ? 1 : 0) : 0) != 0 ? stack.Collectible.Attributes["workableTemperature"].AsFloat(meltingPoint / 2f) <= (double) temperature : temperature >= meltingPoint / 2.0;
    }

    public ItemStack TryPlaceOn(ItemStack stack, BlockEntityAnvil beAnvil)
    {
      if (beAnvil.WorkItemStack != null)
        return null;
      if (stack.Attributes.HasAttribute("voxels"))
      {
        try
        {
          beAnvil.Voxels = BlockEntityAnvil.deserializeVoxels(stack.Attributes.GetBytes("voxels"));
          beAnvil.SelectedRecipeId = stack.Attributes.GetInt("selectedRecipeId");
        }
        catch (Exception ex)
        {
          CreateVoxelsFromBloom(ref beAnvil.Voxels);
        }
      }
      else
        CreateVoxelsFromBloom(ref beAnvil.Voxels);
      var itemstack = stack.Clone();
      itemstack.StackSize = 1;
      itemstack.Collectible.SetTemperature(api.World, itemstack, stack.Collectible.GetTemperature(api.World, stack));
      return itemstack.Clone();
    }

    private void CreateVoxelsFromBloom(ref byte[,,] voxels)
    {
      ItemIngot.CreateVoxelsFromIngot(api, ref voxels);
      var rand = api.World.Rand;
      for (var index1 = -1; index1 < 8; ++index1)
      {
        for (var index2 = 0; index2 < 5; ++index2)
        {
          for (var index3 = -1; index3 < 5; ++index3)
          {
            var index4 = 4 + index1;
            var index5 = 6 + index3;
            if (index2 != 0 || voxels[index4, index2, index5] != 1)
            {
              var num = Math.Max(0, Math.Abs(index4 - 7) - 1) + Math.Max(0, Math.Abs(index5 - 8) - 1) + Math.Max(0.0f, index2 - 1f);
              if (rand.NextDouble() >= num / 3.0 - 0.4000000059604645 + (index2 - 1.5) / 4.0)
                voxels[index4, index2, index5] = rand.NextDouble() <= num / 2.0 ? (byte) 2 : (byte) 1;
            }
          }
        }
      }
    }

    public ItemStack GetBaseMaterial(ItemStack stack) => stack;

    public EnumHelveWorkableMode GetHelveWorkableMode(ItemStack stack, BlockEntityAnvil beAnvil)
    {
      return (EnumHelveWorkableMode) 1;
    }
  }