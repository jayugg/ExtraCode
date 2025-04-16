using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace ExtraCode.BlockBehavior;

public class BlockBehaviorCustomDrinkSpeedContainer : Vintagestory.API.Common.BlockBehavior
{
    public float DrinkSpeed { get; private set; } = 1f;

    public BlockLiquidContainerBase Container { get; }

    public BlockBehaviorCustomDrinkSpeedContainer(Block block) : base(block)
    {
        if (block is BlockLiquidContainerBase container)
        {
            Container = container;
            DrinkSpeed = container.Attributes["drinkSpeed"].AsFloat();
        }
        else
        {
            ExtraCore.Logger?.Warning($"[BlockBehaviorCustomDrinkSpeedContainer][{block.Code}] Block is not a liquid container");
        }
    }
    
    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        DrinkSpeed = properties["drinkSpeed"].AsFloat(DrinkSpeed);
        if (!(DrinkSpeed <= 0)) return;
        ExtraCore.Logger?.Warning($"[BlockBehaviorCustomDrinkSpeedContainer][{block.Code}] Invalid drink speed value: {DrinkSpeed}");
        DrinkSpeed = 1f;
    }
}