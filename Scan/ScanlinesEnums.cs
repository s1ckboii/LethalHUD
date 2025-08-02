using LethalHUD.Configs;
using UnityEngine;

namespace LethalHUD.Scan;

public class ScanlinesEnums
{
    public enum ScanLines
    {
        Default,
        Circles,
        Hexagons,
        Circlegons
    }
    private static readonly float baseIntensity = 352.08f;
    private static readonly float customBaseIntensity = 100f;
    public static void DirtIntensityHandlerByScanLine()
    {
        var scanBloom = ScanController.ScanBloom;
        if (scanBloom == null) return;

        var selected = ConfigEntries.Instance.SelectedScanlineMode.Value;

        if (selected == ScanLines.Default)
            scanBloom.dirtIntensity.Override(Mathf.Max(0f, baseIntensity + ConfigEntries.Instance.DirtIntensity.Value));
        else
            scanBloom.dirtIntensity.Override(Mathf.Max(0f, customBaseIntensity + ConfigEntries.Instance.DirtIntensity.Value));
    }
}