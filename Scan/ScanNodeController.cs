using LethalHUD.Compats;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.Scan;

internal static class ScanNodeController
{
    internal static float lifetime = Plugins.ConfigEntries.ScanNodeLifetime.Value;
    internal static float fadeDuration = Plugins.ConfigEntries.ScanNodeFadeDuration.Value;

    private static readonly Dictionary<ScanNodeProperties, float> _nodeAppearTimes = [];
    private static readonly AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    internal static void UpdateTimers(Dictionary<RectTransform, ScanNodeProperties> scanNodes)
    {
        lifetime = Plugins.ConfigEntries.ScanNodeLifetime.Value;
        fadeDuration = Plugins.ConfigEntries.ScanNodeFadeDuration.Value;

        HashSet<ScanNodeProperties> seenThisFrame = [];

        // Snapshot before processing, because RemoveNode can modify scanNodes.
        List<(RectTransform element, ScanNodeProperties node)> nodesThisFrame = [];

        foreach (var (element, node) in GoodItemScanProxy.EnumerateAllNodes(scanNodes))
        {
            if (element == null || node == null)
                continue;

            nodesThisFrame.Add((element, node));
        }

        foreach (var (element, node) in nodesThisFrame)
        {
            if (element == null || node == null)
                continue;

            if (!element.gameObject.activeInHierarchy)
                continue;

            seenThisFrame.Add(node);

            if (!_nodeAppearTimes.ContainsKey(node))
                _nodeAppearTimes[node] = Time.time;

            float elapsed = Time.time - _nodeAppearTimes[node];
            float removeAt = lifetime + Mathf.Max(0f, fadeDuration);

            if (elapsed >= removeAt)
            {
                RemoveNode(element, node, scanNodes);
                continue;
            }

            float alpha = 1f;

            if (elapsed > lifetime)
            {
                alpha = fadeDuration <= 0f
                    ? 0f
                    : _fadeCurve.Evaluate(Mathf.Clamp01((elapsed - lifetime) / fadeDuration));
            }

            ApplyAlpha(element, alpha);
        }

        CleanInvalidNodes(seenThisFrame);
    }

    private static void ApplyAlpha(RectTransform element, float alpha)
    {
        Image[] images = element.GetComponentsInChildren<Image>(true);

        foreach (Image img in images)
        {
            if (img == null)
                continue;

            Color color = img.color;
            color.a = alpha;
            img.color = color;
        }

        TextMeshProUGUI[] texts = element.GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI txt in texts)
        {
            if (txt == null)
                continue;

            Color color = txt.color;
            color.a = alpha;
            txt.color = color;
        }
    }

    private static void RemoveNode(
        RectTransform element,
        ScanNodeProperties node,
        Dictionary<RectTransform, ScanNodeProperties> scanNodes)
    {
        if (ModCompats.IsGoodItemScanPresent && GoodItemScanProxy.TryRemoveNode(element, node))
        {
            _nodeAppearTimes.Remove(node);
            return;
        }

        if (element != null)
            element.gameObject.SetActive(false);

        scanNodes?.Remove(element);
        _nodeAppearTimes.Remove(node);
    }

    private static void CleanInvalidNodes(HashSet<ScanNodeProperties> seenThisFrame)
    {
        List<ScanNodeProperties> toRemove = [];

        foreach (KeyValuePair<ScanNodeProperties, float> kvp in _nodeAppearTimes)
        {
            if (kvp.Key == null || !seenThisFrame.Contains(kvp.Key))
                toRemove.Add(kvp.Key);
        }

        foreach (ScanNodeProperties key in toRemove)
            _nodeAppearTimes.Remove(key);
    }

    internal static void Reset()
    {
        _nodeAppearTimes.Clear();
    }
}