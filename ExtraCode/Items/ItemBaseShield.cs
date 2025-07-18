using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

#nullable enable

namespace ExtraCode.Items;
public class ModSystemStopRaiseShieldAnim : ModSystem
{
    ICoreClientAPI? _capi;
    public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

    public override void StartClientSide(ICoreClientAPI? api)
    {
        _capi = api;
        if (_capi == null) return;
        _capi.Event.AfterActiveSlotChanged += _ => MaybeStopRaiseShield();
    }

    public override void AssetsFinalize(ICoreAPI api)
    {
        if (_capi == null) return;
        _capi.World.Player.InventoryManager.GetHotbarInventory().SlotModified += _ => MaybeStopRaiseShield();
    }

    private void MaybeStopRaiseShield()
    {
        if (_capi?.World?.Player == null) return;
        var entityPlayer = _capi.World.Player.Entity;
        if (entityPlayer.RightHandItemSlot.Itemstack?.Item is not ItemBaseShield && entityPlayer.AnimManager.IsAnimationActive(ItemBaseShield.RaiseShieldRightAnim))
        {
            entityPlayer.AnimManager.StopAnimation(ItemBaseShield.RaiseShieldRightAnim);
        }
    }
}

public class ItemBaseShield : Item // 1.21.x IAttachableToEntity
{
    public const string RaiseShieldLeftAnim  = "raiseshield-left";
    public const string RaiseShieldRightAnim = "raiseshield-right";

    /* 1.21.x stuff
    IAttachableToEntity? _attachableToEntity;
    #region IAttachableToEntity
    public int RequiresBehindSlots { get; set; } = 0;
    string? IAttachableToEntity.GetCategoryCode(ItemStack stack) => _attachableToEntity?.GetCategoryCode(stack);
    CompositeShape? IAttachableToEntity.GetAttachedShape(ItemStack stack, string slotCode) => _attachableToEntity?.GetAttachedShape(stack, slotCode);
    string[]? IAttachableToEntity.GetDisableElements(ItemStack stack) => _attachableToEntity?.GetDisableElements(stack);
    string[]? IAttachableToEntity.GetKeepElements(ItemStack stack) => _attachableToEntity?.GetKeepElements(stack);
    string? IAttachableToEntity.GetTexturePrefixCode(ItemStack stack) => _attachableToEntity?.GetTexturePrefixCode(stack);
    void IAttachableToEntity.CollectTextures(ItemStack itemstack, Shape intoShape, string texturePrefixCode, Dictionary<string, CompositeTexture> intoDict)
    => _attachableToEntity?.CollectTextures(itemstack, intoShape, texturePrefixCode, intoDict);
    public bool IsAttachable(Entity toEntity, ItemStack itemStack) => Attributes["isAttachable"].AsBool(true);
    #endregion
    public override void OnLoaded(ICoreAPI coreApi)
    {
        base.OnLoaded(coreApi);
        _attachableToEntity = IAttachableToEntity.FromAttributes(this);
    }
    */
    
    private static void SetAnimActive(IAnimationManager anim, string name, bool active)
    {
        var isActive = anim.IsAnimationActive(name);
        if (active && !isActive) anim.StartAnimation(name);
        else if (!active && isActive) anim.StopAnimation(name);
    }



    public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
    {
        var inLeftHand = (byEntity.LeftHandItemSlot == slot);
        var animThisHand  = inLeftHand ? RaiseShieldLeftAnim  : RaiseShieldRightAnim;
        var animOtherHand = inLeftHand ? RaiseShieldRightAnim : RaiseShieldLeftAnim;
        var shouldRaise = byEntity.Controls.Sneak && !byEntity.Controls.RightMouseDown;
        SetAnimActive(byEntity.AnimManager, animThisHand,  shouldRaise);
        SetAnimActive(byEntity.AnimManager, animOtherHand, false);
        base.OnHeldIdle(slot, byEntity);
    }
    
    public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
    {
        base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        var shieldAttributes = inSlot.Itemstack?.ItemAttributes?["shield"];
        if (shieldAttributes is not { Exists: true }) return;

        if (shieldAttributes["protectionChance"]["active-projectile"].Exists)
        {
            var activeProjectileProtChance = shieldAttributes["protectionChance"]["active-projectile"].AsFloat();
            var passiveProjectileProtChance = shieldAttributes["protectionChance"]["passive-projectile"].AsFloat();
            var projectileDamageAbsorption = shieldAttributes["projectileDamageAbsorption"].AsFloat();
            dsc.AppendLine("<strong>" + Lang.Get("Projectile protection") + "</strong>");
            dsc.AppendLine(Lang.Get("shield-stats", (int)(100 * activeProjectileProtChance), (int)(100 * passiveProjectileProtChance), projectileDamageAbsorption));
            dsc.AppendLine();
        }

        var damageAbsorption = shieldAttributes["damageAbsorption"].AsFloat();
        var activeProtChance = shieldAttributes["protectionChance"]["active"].AsFloat();
        var passiveProtChance = shieldAttributes["protectionChance"]["passive"].AsFloat();

        dsc.AppendLine("<strong>" + Lang.Get("Melee attack protection") + "</strong>");
        dsc.AppendLine(Lang.Get("shield-stats", (int)(100 * activeProtChance), (int)(100 * passiveProtChance), damageAbsorption));
        dsc.AppendLine();
    }
}