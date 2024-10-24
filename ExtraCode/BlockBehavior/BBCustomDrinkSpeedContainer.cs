using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace ExtraCode.Behavior;

public class BlockBehaviorCustomDrinkSpeedContainer : BlockBehavior
{
    public float DrinkSpeed => _drinkSpeed;
    private float _drinkSpeed = 1f;
    public BlockLiquidContainerBase Container => _container;
    private BlockLiquidContainerBase _container;
    public BlockBehaviorCustomDrinkSpeedContainer(Block block) : base(block)
    {
        if (block is BlockLiquidContainerBase container)
        {
            this._container = container;
            this._drinkSpeed = container.Attributes["drinkSpeed"].AsFloat();
        }
        else
        {
            ExtraCore.Logger.Warning($"[BlockBehaviorCustomDrinkSpeedContainer][{block.Code}] Block is not a liquid container");
        }
    }
    
    public override void Initialize(JsonObject properties)
    {
        base.Initialize(properties);
        this._drinkSpeed = properties["drinkSpeed"].AsFloat(_drinkSpeed);
        if (!(this._drinkSpeed <= 0)) return;
        ExtraCore.Logger.Warning($"[BlockBehaviorCustomDrinkSpeedContainer][{block.Code}] Invalid drink speed value: {this._drinkSpeed}");
        this._drinkSpeed = 1f;
    }
}