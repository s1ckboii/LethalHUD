/*using System.Collections.Generic;
using UnityEngine;
using LethalHUD.Compats;
using UnityEngine.UI;

namespace LethalHUD.Scan;
internal static class ScanNodeTextureManager
{
    private static readonly HashSet<ScanNodeProperties> updatedNodes = [];

    internal static void ApplySpritesToAll()
    {
        var chosenShape = Plugins.ConfigEntries.ScanNodeShapeChoice.Value;

        if (!Plugins.ScanNodeSprites.TryGetValue(chosenShape, out var spritePair))
        {
            Plugins.Logger.LogWarning($"No sprites found for shape {chosenShape}");
            return;
        }

        Sprite innerSprite = spritePair.Inner;
        Sprite outerSprite = spritePair.Outer;

        if (ModCompats.IsGoodItemScanPresent && GoodItemScan.GoodItemScan.scanner != null)
        {
            foreach (ScanNodeProperties node in GoodItemScan.GoodItemScan.scanner._scanNodes.Keys)
            {
                ApplySpritesToNode(node, innerSprite, outerSprite);
            }
        }

        foreach (ScanNodeProperties node in Object.FindObjectsOfType<ScanNodeProperties>())
        {
            ApplySpritesToNode(node, innerSprite, outerSprite);
        }
    }

    private static void ApplySpritesToNode(ScanNodeProperties node, Sprite innerSprite, Sprite outerSprite)
    {
        if (node == null || !node.gameObject.activeInHierarchy) return;

        if (updatedNodes.Contains(node)) return;

        Image innerRenderer = node.transform.Find("Inner")?.GetComponent<Image>();
        Image outerRenderer = node.transform.Find("Outer")?.GetComponent<Image>();

        if (innerRenderer != null)
            innerRenderer.sprite = innerSprite;

        if (outerRenderer != null)
            outerRenderer.sprite = outerSprite;

        updatedNodes.Add(node);
    }
}*/