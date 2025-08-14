using System.Collections.Generic;
using UnityEngine;

namespace LethalHUD.Scan;
internal static class Scannodes
{

    // GoodItemScan is incompatible rn

    private static readonly Dictionary<ScanNodeProperties, float> nodeAppearTimes = [];
    private static readonly float lifetime = 3f;
    private static readonly float fadeDuration = 1f;
    private static readonly AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    internal static void UpdateTimers(RectTransform[] scanElements, Dictionary<RectTransform, ScanNodeProperties> scanNodes)
    {
        foreach (var element in scanElements)
        {
            if (element == null) continue;
            if (!scanNodes.TryGetValue(element, out var node) || node == null)
            {
                CleanInvalidNodes();
                continue;
            }

            if (!nodeAppearTimes.ContainsKey(node))
                nodeAppearTimes[node] = Time.time;

            float elapsed = Time.time - nodeAppearTimes[node];

            if (elapsed >= lifetime + fadeDuration)
            {
                RemoveNode(element, node, scanNodes);
                continue;
            }

            float alpha = elapsed > lifetime
                ? fadeCurve.Evaluate((elapsed - lifetime) / fadeDuration)
                : 1f;

            var canvasGroup = element.GetComponent<CanvasGroup>() ?? element.gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = alpha;
        }
    }

    private static void RemoveNode(RectTransform element, ScanNodeProperties node, Dictionary<RectTransform, ScanNodeProperties> scanNodes)
    {
        element.gameObject.SetActive(false);
        scanNodes.Remove(element);
        nodeAppearTimes.Remove(node);
    }

    private static void CleanInvalidNodes()
    {
        var toRemove = new List<ScanNodeProperties>();
        foreach (var kvp in nodeAppearTimes)
            if (kvp.Key == null)
                toRemove.Add(kvp.Key);

        foreach (var key in toRemove)
            nodeAppearTimes.Remove(key);
    }
}