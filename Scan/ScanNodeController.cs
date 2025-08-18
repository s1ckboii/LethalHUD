using LethalHUD.Compats;
using LethalHUD.Configs;
using System.Collections.Generic;
using UnityEngine;

namespace LethalHUD.Scan;
internal static class ScanNodeController
{
    private static readonly Dictionary<ScanNodeProperties, float> nodeAppearTimes = [];
    internal static float lifetime = Plugins.ConfigEntries.ScanNodeLifetime.Value;
    internal static float fadeDuration = Plugins.ConfigEntries.ScanNodeFadeDuration.Value;
    private static readonly AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    internal static void UpdateTimers(RectTransform[] scanElements, Dictionary<RectTransform, ScanNodeProperties> scanNodes)
    {
        foreach (RectTransform element in scanElements)
        {
            if (element == null) continue;
            if (!scanNodes.TryGetValue(element, out ScanNodeProperties node) || node == null)
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

            CanvasGroup canvasGroup = element.GetComponent<CanvasGroup>();
            canvasGroup.alpha = alpha;
        }
    }
    internal static void UpdateGoodItemScanNodes()
    {
        if (!ModCompats.IsGoodItemScanPresent) return;

        var scanner = GoodItemScan.GoodItemScan.scanner;
        if (scanner == null) return;

        foreach (var kvp in scanner._scanNodes)
        {
            if (!GoodItemScanProxy.TryGetRectTransform(kvp.Key, out RectTransform rect)) continue;
            if (rect == null) continue;

            if (!nodeAppearTimes.ContainsKey(kvp.Key))
                nodeAppearTimes[kvp.Key] = Time.time;

            float elapsed = Time.time - nodeAppearTimes[kvp.Key];

            float alpha = elapsed > lifetime
                ? fadeCurve.Evaluate((elapsed - lifetime) / fadeDuration)
                : 1f;

            CanvasGroup canvasGroup = rect.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.alpha = alpha;

            rect.gameObject.SetActive(alpha > 0f);
        }
    }
    internal static void ResetGoodItemScanNodes()
    {
        if (!ModCompats.IsGoodItemScanPresent) return;

        var scanner = GoodItemScan.GoodItemScan.scanner;
        if (scanner == null) return;

        foreach (var kvp in scanner._scanNodes)
        {
            if (!GoodItemScanProxy.TryGetRectTransform(kvp.Key, out RectTransform rect)) continue;
            if (rect == null) continue;

            nodeAppearTimes[kvp.Key] = Time.time;
            rect.gameObject.SetActive(true);

            var canvasGroup = rect.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
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
        List<ScanNodeProperties> toRemove = [];
        foreach (var kvp in nodeAppearTimes)
            if (kvp.Key == null)
                toRemove.Add(kvp.Key);

        foreach (var key in toRemove)
            nodeAppearTimes.Remove(key);
    }
}