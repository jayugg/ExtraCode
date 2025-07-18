using System.Reflection;
using ExtraCode.Util;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.ServerMods;

#nullable enable
namespace ExtraCode.Deposits;

public static class DepositExtensions
{
    private static MethodInfo? DepositBlockResolveMethod =>
        typeof(DepositBlock).GetMethod("Resolve", BindingFlags.NonPublic | BindingFlags.Instance);
    public static DepositVariant GetParentDeposit(this DepositVariant depositVariant) =>
        depositVariant.GetInternalField<DepositVariant>("parentDeposit");
    
    public static ResolvedDepositBlock? Resolve(
        this DepositBlock depositBlock,
        string fileForLogging,
        ICoreServerAPI api,
        Block inblock,
        string key,
        string value) =>
        DepositBlockResolveMethod?.Invoke(depositBlock, [fileForLogging, api, inblock, key, value]) as ResolvedDepositBlock;
}