using LethalHUD.Compats;
using System.Collections.Generic;
using UnityEngine;

namespace LethalHUD.Scan;

internal static class ScanNodeController
{
    internal static float lifetime = Plugins.ConfigEntries.ScanNodeLifetime.Value;
    internal static float fadeDuration = Plugins.ConfigEntries.ScanNodeFadeDuration.Value;

    private static readonly Dictionary<ScanNodeProperties, float> _nodeAppearTimes = [];
    private static readonly HashSet<ScanNodeProperties> _seenThisFrame = [];
    private static readonly List<ScanNodeProperties> _toRemove = [];
    private static readonly AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    internal static void UpdateTimers(Dictionary<RectTransform, ScanNodeProperties> scanNodes)
    {
        lifetime = Plugins.ConfigEntries.ScanNodeLifetime.Value;
        fadeDuration = Plugins.ConfigEntries.ScanNodeFadeDuration.Value;

        _seenThisFrame.Clear();

        IReadOnlyList<(RectTransform rect, ScanNodeProperties node)> nodesThisFrame = GoodItemScanProxy.EnumerateAllNodes(scanNodes);

        for (int i = 0; i < nodesThisFrame.Count; i++)
        {
            (RectTransform element, ScanNodeProperties node) = nodesThisFrame[i];

            if (element == null || node == null)
                continue;

            if (!element.gameObject.activeInHierarchy)
                continue;

            _seenThisFrame.Add(node);

            if (!_nodeAppearTimes.TryGetValue(node, out float appearedAt))
            {
                appearedAt = Time.time;
                _nodeAppearTimes[node] = appearedAt;
            }

            float elapsed = Time.time - appearedAt;

            if (GoodItemScanProxy.IsGoodItemScanNode(element))
            {
                if (elapsed >= lifetime)
                    RemoveNode(element, node, scanNodes);

                continue;
            }

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

            ScanNodeTextureManager.ApplyAlpha(element, node, alpha);
        }

        CleanInvalidNodes();
    }

    private static void RemoveNode(RectTransform element, ScanNodeProperties node, Dictionary<RectTransform, ScanNodeProperties> scanNodes)
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

    private static void CleanInvalidNodes()
    {
        _toRemove.Clear();

        foreach (KeyValuePair<ScanNodeProperties, float> kvp in _nodeAppearTimes)
        {
            if (kvp.Key == null || !_seenThisFrame.Contains(kvp.Key))
                _toRemove.Add(kvp.Key);
        }

        for (int i = 0; i < _toRemove.Count; i++)
            _nodeAppearTimes.Remove(_toRemove[i]);
    }

    internal static void Reset()
    {
        _nodeAppearTimes.Clear();
        _seenThisFrame.Clear();
        _toRemove.Clear();
    }
}