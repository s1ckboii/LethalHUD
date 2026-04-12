using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.Scan;
internal static class ScanNodeController
{
    internal static float lifetime = Plugins.ConfigEntries.ScanNodeLifetime.Value;
    internal static float fadeDuration = Plugins.ConfigEntries.ScanNodeFadeDuration.Value;

    private static readonly Dictionary<ScanNodeProperties, float> _nodeAppearTimes = [];
    private static readonly AnimationCurve _fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

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

            if (!_nodeAppearTimes.ContainsKey(node))
                _nodeAppearTimes[node] = Time.time;

            float elapsed = Time.time - _nodeAppearTimes[node];

            if (elapsed >= lifetime + fadeDuration)
            {
                RemoveNode(element, node, scanNodes);
                continue;
            }

            float alpha = elapsed > lifetime
                ? _fadeCurve.Evaluate((elapsed - lifetime) / fadeDuration)
                : 1f;

            ApplyAlphaToImages(element, alpha);
        }
    }
    private static void ApplyAlphaToImages(RectTransform element, float alpha)
    {
        Image[] images = element.GetComponentsInChildren<Image>(true);

        foreach (var img in images)
        {
            Color color = img.color;
            color.a = alpha;
            img.color = color;
        }
    }
    private static void RemoveNode(RectTransform element, ScanNodeProperties node, Dictionary<RectTransform, ScanNodeProperties> scanNodes)
    {
        element.gameObject.SetActive(false);
        scanNodes.Remove(element);
        _nodeAppearTimes.Remove(node);
    }

    private static void CleanInvalidNodes()
    {
        List<ScanNodeProperties> toRemove = [];
        foreach (KeyValuePair<ScanNodeProperties, float> kvp in _nodeAppearTimes)
            if (kvp.Key == null)
                toRemove.Add(kvp.Key);

        foreach (ScanNodeProperties key in toRemove)
            _nodeAppearTimes.Remove(key);
    }
}