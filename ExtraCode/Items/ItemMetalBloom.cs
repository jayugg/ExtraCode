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
      int defaultValue = 0;
      JsonObject attributes = stack.Collectible.Attributes;
      if ((attributes != null ? (attributes["requiresAnvilTier"].Exists ? 1 : 0) : 0) != 0)
        defaultValue = stack.Collectible.Attributes["requiresAnvilTier"]?.AsInt(defaultValue) ?? defaultValue;
      return defaultValue;
    }

    public List<SmithingRecipe> GetMatchingRecipes(ItemStack stack)
    {
      return this.api.GetSmithingRecipes().Where((System.Func<SmithingRecipe, bool>) (r => r.Ingredient.SatisfiesAsIngredient(stack))).OrderBy((System.Func<SmithingRecipe, AssetLocation>) (r => r.Output.ResolvedItemstack.Collectible.Code)).ToList();
    }

    public bool CanWork(ItemStack stack)
    {
      float temperature = stack.Collectible.GetTemperature(this.api.World, stack);
      float meltingPoint = stack.Collectible.GetMeltingPoint(this.api.World, null, new DummySlot(stack));
      JsonObject attributes = stack.Collectible.Attributes;
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
          this.CreateVoxelsFromBloom(ref beAnvil.Voxels);
        }
      }
      else
        this.CreateVoxelsFromBloom(ref beAnvil.Voxels);
      ItemStack itemstack = stack.Clone();
      itemstack.StackSize = 1;
      itemstack.Collectible.SetTemperature(this.api.World, itemstack, stack.Collectible.GetTemperature(this.api.World, stack));
      return itemstack.Clone();
    }

    private void CreateVoxelsFromBloom(ref byte[,,] voxels)
    {
      ItemIngot.CreateVoxelsFromIngot(this.api, ref voxels);
      Random rand = this.api.World.Rand;
      for (int index1 = -1; index1 < 8; ++index1)
      {
        for (int index2 = 0; index2 < 5; ++index2)
        {
          for (int index3 = -1; index3 < 5; ++index3)
          {
            int index4 = 4 + index1;
            int index5 = 6 + index3;
            if (index2 != 0 || voxels[index4, index2, index5] != 1)
            {
              float num = Math.Max(0, Math.Abs(index4 - 7) - 1) + Math.Max(0, Math.Abs(index5 - 8) - 1) + Math.Max(0.0f, index2 - 1f);
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