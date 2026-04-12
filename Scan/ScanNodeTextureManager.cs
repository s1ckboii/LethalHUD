using LethalHUD.HUD;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static LethalHUD.Enums;
using LethalHUD.Compats;

namespace LethalHUD.Scan;
internal static class ScanNodeTextureManager
{
    internal static void Tick(Dictionary<RectTransform, ScanNodeProperties> scanNodes)
    {
        foreach (var (rect, node) in GoodItemScanProxy.EnumerateAllNodes(scanNodes))
        {
            if (rect == null || node == null) continue;
            if (IsHandledByDawn(rect)) continue;

            Apply(rect, node);
        }
    }

    private static void Apply(RectTransform rect, ScanNodeProperties node)
    {
        if (!rect.gameObject.activeInHierarchy) return;

        ScanNodeType type = ScanNodeClassifier.GetType(node);

        string shape = type switch
        {
            ScanNodeType.Scrap => Plugins.ConfigEntries.ScanNodeShape_Scrap.Value,
            ScanNodeType.Creature => Plugins.ConfigEntries.ScanNodeShape_Creature.Value,
            _ => Plugins.ConfigEntries.ScanNodeShape_Default.Value
        };

        string colorHex = type switch
        {
            ScanNodeType.Scrap => Plugins.ConfigEntries.ScanNodeColor_Scrap.Value,
            ScanNodeType.Creature => Plugins.ConfigEntries.ScanNodeColor_Creature.Value,
            _ => Plugins.ConfigEntries.ScanNodeColor_Default.Value
        };

        bool isDefault = IsDefault(type, colorHex);

        if (Plugins.ScanNodeSprites.TryGetValue(shape, out var entry))
        {
            var spritePair = entry.Asset;

            var inner = rect.transform.Find("Circle/Inner")?.GetComponent<Image>();
            var outer = rect.transform.Find("Circle/Outer")?.GetComponent<Image>();

            if (inner != null) inner.sprite = spritePair.Inner;
            if (outer != null) outer.sprite = spritePair.Outer;
        }

        if (isDefault) return;

        Color baseColor = HUDUtils.ParseHexColor(colorHex);

        var images = rect.GetComponentsInChildren<Image>(true);
        foreach (var img in images)
        {
            if (img == null) continue;
            img.color = new Color(baseColor.r, baseColor.g, baseColor.b, img.color.a);
        }

        float l = (0.299f * baseColor.r) + (0.587f * baseColor.g) + (0.114f * baseColor.b);
        Color readable = l > 0.5f ? Color.black : Color.white;

        var texts = rect.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var txt in texts)
        {
            if (txt == null) continue;
            txt.color = new Color(readable.r, readable.g, readable.b, txt.color.a);
        }
    }

    private static bool IsHandledByDawn(RectTransform rect)
    {
        return rect.GetComponent("ForceScanColorOnItem") != null;
    }

    private static bool IsDefault(ScanNodeType type, string hex)
    {
        return type switch
        {
            ScanNodeType.Scrap => hex.Equals("#38AB00", System.StringComparison.OrdinalIgnoreCase),
            ScanNodeType.Creature => hex.Equals("#FF0A00", System.StringComparison.OrdinalIgnoreCase),
            _ => hex.Equals("#0B00B2", System.StringComparison.OrdinalIgnoreCase)
        };
    }

    internal static void ForceRefresh() { }
}