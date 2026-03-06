using System.Collections.Generic;

namespace LethalHUD.API;
internal static class HUDStyleRegistry
{
    internal static string[] GetHealthBarStyles()
    {
        List<string> styles = ["Default"];

        foreach (var key in Plugins.HealthBarPrefabs.Keys)
            if (!styles.Contains(key))
                styles.Add(key);

        return [.. styles];
    }

    internal static string[] GetStaminaBarStyles()
    {
        List<string> styles = ["Default"];

        foreach (var key in Plugins.StaminaBarPrefabs.Keys)
            if (!styles.Contains(key))
                styles.Add(key);

        return [.. styles];
    }

    internal static string[] GetInventoryFrameStyles()
    {
        List<string> styles = ["Default"];

        foreach (var key in Plugins.SlotPrefabs.Keys)
            if (!styles.Contains(key))
                styles.Add(key);

        return [.. styles];
    }

    internal static string[] GetScanlines()
    {
        List<string> styles = ["Default"];

        foreach (var key in Plugins.ScanlineTextures.Keys)
            if (!styles.Contains(key))
                styles.Add(key);

        return [.. styles];
    }

    internal static string[] GetScannodes()
    {
        List<string> styles = ["Default"];

        foreach (var key in Plugins.ScanNodeSprites.Keys)
            if (!styles.Contains(key))
                styles.Add(key);

        return [.. styles];
    }
}