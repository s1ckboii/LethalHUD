using LethalHUD.Scan;
using UnityEngine;

namespace LethalHUD;
public static class Enums
{
    public enum SlotEnums
    {
        None,
        Rainbow,
        Summer,
        Winter,
        Vaporwave,
        Deepmint,
        Radioactive,
        TideEmber
    }
    public enum ScanLines
    {
        Default,
        Circles,
        Hexagons,
        Circuit,
        Noisy,
        Scifi
    }
    public enum WeightUnit
    {
        Pounds,
        Kilograms,
        Manuls
    }
    public enum FPSPingLayout
    {
        Vertical,
        Horizontal
    }

    internal static Color solarFlare = new(1f, 0.8f, 0.3f);
    internal static Color moltenCore = new(1f, 0.2f, 0f);
    internal static Color skyWave = new(0.4f, 0.65f, 1f);
    internal static Color moonlitMist = new(0.8f, 0.8f, 1f);
    internal static Color pinkPrism = new(1f, 0.4f, 1f);
    internal static Color aquaPulse = new(0.4f, 1f, 1f);
    internal static Color mintWave = new(0.5647f, 0.8706f, 0.7843f);
    internal static Color deepTeal = new(0.1608f, 0.2863f, 0.2549f);
    internal static Color neonLime = new(0.443f, 0.780f, 0f);
    internal static Color lemonGlow = new(0.941f, 1f, 0.471f);
    internal static Color crimsonSpark = new(1f, 0.27f, 0f);
    internal static Color deepOcean = new(0f, 0.48f, 0.54f);

    private static readonly float baseIntensity = 352.08f;
    private static readonly float customBaseIntensity = 100f;
    internal static void DirtIntensityHandlerByScanLine()
    {
        var scanBloom = ScanController.ScanBloom;
        if (scanBloom == null) return;

        var selected = Plugins.ConfigEntries.SelectedScanlineMode.Value;

        if (selected == ScanLines.Default)
            scanBloom.dirtIntensity.Override(Mathf.Max(0f, baseIntensity + Plugins.ConfigEntries.DirtIntensity.Value));
        else
            scanBloom.dirtIntensity.Override(Mathf.Max(0f, customBaseIntensity + Plugins.ConfigEntries.DirtIntensity.Value));
    }
}
