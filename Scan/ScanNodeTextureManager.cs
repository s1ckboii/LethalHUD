using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.Scan;
internal static class ScanNodeTextureManager
{
    private static readonly Dictionary<GameObject, Color> _nodeColors = [];

    internal static void Tick()
    {
        GameObject scanner = GameObject.Find("UI/Canvas/ObjectScanner");
        if (scanner == null) return;

        ScanNodeShape chosenShape = Plugins.ConfigEntries.ScanNodeShapeChoice.Value;
        if (!Plugins.ScanNodeSprites.TryGetValue(chosenShape, out Plugins.ScanNodeCircleTextures spritePair))
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

        if (!_nodeColors.ContainsKey(inner.gameObject))
            _nodeColors[inner.gameObject] = inner.color;
        if (!_nodeColors.ContainsKey(outer.gameObject))
            _nodeColors[outer.gameObject] = outer.color;

        inner.sprite = innerSprite;
        outer.sprite = outerSprite;

        inner.color = _nodeColors[inner.gameObject];
        outer.color = _nodeColors[outer.gameObject];
    }

    internal static void ForceRefresh()
    {
        Tick();
    }

    internal static void ClearDestroyedObjects()
    {
        List<GameObject> keysToRemove = [];
        foreach (KeyValuePair<GameObject, Color> kvp in _nodeColors)
            if (kvp.Key == null)
                keysToRemove.Add(kvp.Key);

        foreach (GameObject key in keysToRemove)
            _nodeColors.Remove(key);
    }
}