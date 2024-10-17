using System.Text.RegularExpressions;
using Vintagestory.API.Common;

namespace ExtraCode.Util;

public static class Extensions
{
    public static AssetLocation ReplaceVariants(this AssetLocation code, Block block)
    {
        var regex = new Regex(@"\{(\w+)\}");
        return new AssetLocation(code.Domain, regex.Replace(code.Path, match =>
        {
            var key = match.Groups[1].Value;
            return block.Variant.TryGetValue(key, out var value) ? value : match.Value;
        }));
    }
}