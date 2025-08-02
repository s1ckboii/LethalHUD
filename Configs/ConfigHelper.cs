using UnityEngine;

namespace LethalHUD.Configs;

public static class ConfigHelper
{
    public static Color GetScanColor()
    {
        var cfg = ConfigEntries.Instance;

        string hex = cfg._lastScanColorChange >= cfg._lastMainColorChange
            ? cfg.ScanColor.Value
            : cfg.MainColor.Value;

        return ParseHexColor(hex);
    }

    public static Color GetSlotColor()
    {
        var cfg = ConfigEntries.Instance;

        string hex = cfg._lastSlotColorChange >= cfg._lastMainColorChange
            ? cfg.SlotColor.Value
            : cfg.MainColor.Value;

        return ParseHexColor(hex);
    }

    public static Color ParseHexColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return new Color(color.r, color.g, color.b);
        }

        Plugins.mls.LogWarning($"Invalid HEX color: {hex}. Defaulting to original blue.");
        return new Color(0f, 0.047f, 1f);
    }
}
