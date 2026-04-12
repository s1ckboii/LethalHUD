using System.Collections.Generic;
using UnityEngine;

namespace LethalHUD.Compats;
internal static class GoodItemScanProxy
{
    internal static IEnumerable<(RectTransform rect, ScanNodeProperties node)> EnumerateAllNodes(Dictionary<RectTransform, ScanNodeProperties> vanillaNodes)
    {
        foreach (var kvp in vanillaNodes)
            yield return (kvp.Key, kvp.Value);

        if (!ModCompats.IsGoodItemScanPresent)
            yield break;

        var scanner = GoodItemScan.GoodItemScan.scanner;
        if (scanner == null)
            yield break;

        foreach (var scanned in scanner.activeNodes)
        {
            if (scanned.rectTransform == null || scanned.ScanNodeProperties == null)
                continue;

            yield return (scanned.rectTransform, scanned.ScanNodeProperties);
        }
    }
}