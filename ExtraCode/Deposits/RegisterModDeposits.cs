using JetBrains.Annotations;
using Vintagestory.API.Common;
using Vintagestory.ServerMods;

namespace ExtraCode.Deposits;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
public class RegisterModDeposits : ModSystem
{
    public override double ExecuteOrder() => 0.19;

    public override void Start(ICoreAPI api)
    {
        DepositGeneratorRegistry.RegisterDepositGenerator<SphericalChildDepositGenerator>("ec:childdeposit-spherical");
    }
}