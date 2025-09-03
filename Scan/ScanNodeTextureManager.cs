using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.Scan;

internal static class ScanNodeTextureManager
{
    private static readonly Dictionary<GameObject, Color> nodeColors = [];

    internal static void Tick()
    {
        GameObject scanner = GameObject.Find("UI/Canvas/ObjectScanner");
        if (scanner == null) return;

        var chosenShape = Plugins.ConfigEntries.ScanNodeShapeChoice.Value;
        if (!Plugins.ScanNodeSprites.TryGetValue(chosenShape, out var spritePair))
            return;

        Sprite innerSprite = spritePair.Inner;
        Sprite outerSprite = spritePair.Outer;

        foreach (Transform child in scanner.transform)
        {
            if (!child.name.StartsWith("ScanObject")) continue;
            ApplySpritesToScanObject(child.gameObject, innerSprite, outerSprite);
        }
    }

    private static void ApplySpritesToScanObject(GameObject scanObject, Sprite innerSprite, Sprite outerSprite)
    {
        if (scanObject == null || !scanObject.activeInHierarchy) return;

        Image inner = scanObject.transform.Find("Circle/Inner")?.GetComponent<Image>();
        Image outer = scanObject.transform.Find("Circle/Outer")?.GetComponent<Image>();

        if (inner == null || outer == null) return;

        if (!nodeColors.ContainsKey(inner.gameObject))
            nodeColors[inner.gameObject] = inner.color;
        if (!nodeColors.ContainsKey(outer.gameObject))
            nodeColors[outer.gameObject] = outer.color;

        inner.sprite = innerSprite;
        outer.sprite = outerSprite;

        inner.color = nodeColors[inner.gameObject];
        outer.color = nodeColors[outer.gameObject];
    }

    internal static void ForceRefresh()
    {
        Tick();
    }

    internal static void ClearDestroyedObjects()
    {
        List<GameObject> keysToRemove = [];
        foreach (var kvp in nodeColors)
            if (kvp.Key == null)
                keysToRemove.Add(kvp.Key);

        foreach (var key in keysToRemove)
            nodeColors.Remove(key);
    }
}