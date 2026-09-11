using LethalHUD.Compats;
using LethalHUD.HUD;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.Scan;

internal static class ScanNodeTextureManager
{
    private sealed class VisualCache
    {
        internal ScanNodeProperties Node;
        internal Image Inner;
        internal Image Outer;
        internal Image[] Images;
        internal ScanNodeColorMeshEffect[] ColorEffects;
        internal TextMeshProUGUI[] Texts;
        internal bool HandledByDawn;
        internal int LastSeenFrame;
        internal int NextDiscoveryFrame;
    }

    private static readonly Dictionary<RectTransform, VisualCache> _visualCache = [];
    private static readonly List<RectTransform> _deadCacheKeys = [];

    private static bool _settingsDirty = true;
    private static string _shapeDefault;
    private static string _shapeScrap;
    private static string _shapeCreature;
    private static string _hexDefault;
    private static string _hexScrap;
    private static string _hexCreature;
    private static Color _colorDefault;
    private static Color _colorScrap;
    private static Color _colorCreature;

    internal static void Tick(Dictionary<RectTransform, ScanNodeProperties> scanNodes)
    {
        RefreshSettingsIfNeeded();

        IReadOnlyList<(RectTransform rect, ScanNodeProperties node)> nodes =
            GoodItemScanProxy.EnumerateAllNodes(scanNodes);

        for (int i = 0; i < nodes.Count; i++)
        {
            (RectTransform rect, ScanNodeProperties node) = nodes[i];

            if (rect == null || node == null)
                continue;

            VisualCache cache = GetOrRefreshCache(rect, node);
            cache.LastSeenFrame = Time.frameCount;

            if (cache.HandledByDawn)
            {
                ApplyImageColorOverride(cache, Color.white, false);
                continue;
            }

            Apply(rect, node, cache);
        }

        if ((Time.frameCount % 300) == 0)
            PruneCache();
    }

    private static VisualCache GetOrRefreshCache(RectTransform rect, ScanNodeProperties node)
    {
        bool shouldDiscover = false;

        if (!_visualCache.TryGetValue(rect, out VisualCache cache))
        {
            cache = new VisualCache();
            _visualCache[rect] = cache;
            shouldDiscover = true;
        }
        else if (cache.Node != node)
        {
            shouldDiscover = true;
        }
        else if (Time.frameCount >= cache.NextDiscoveryFrame)
        {
            shouldDiscover = true;
        }

        if (shouldDiscover)
            DiscoverVisuals(rect, node, cache);

        return cache;
    }

    private static void DiscoverVisuals(RectTransform rect, ScanNodeProperties node, VisualCache cache)
    {
        cache.Node = node;
        cache.Inner = rect.transform.Find("Circle/Inner")?.GetComponent<Image>();
        cache.Outer = rect.transform.Find("Circle/Outer")?.GetComponent<Image>();
        cache.Images = rect.GetComponentsInChildren<Image>(true);
        cache.ColorEffects = null;
        cache.Texts = rect.GetComponentsInChildren<TextMeshProUGUI>(true);
        cache.HandledByDawn = rect.GetComponent("ForceScanColorOnItem") != null;

        int stagger = Math.Abs(rect.GetInstanceID() % 60);
        cache.NextDiscoveryFrame = Time.frameCount + 180 + stagger;
    }

    private static void Apply(RectTransform rect, ScanNodeProperties node, VisualCache cache)
    {
        if (!rect.gameObject.activeInHierarchy)
            return;

        ScanNodeType type = ScanNodeClassifier.GetType(node);

        string shape;
        Color baseColor;
        bool isDefault;

        switch (type)
        {
            case ScanNodeType.Scrap:
                shape = _shapeScrap;
                baseColor = _colorScrap;
                isDefault = IsDefault(type, _hexScrap);
                break;

            case ScanNodeType.Creature:
                shape = _shapeCreature;
                baseColor = _colorCreature;
                isDefault = IsDefault(type, _hexCreature);
                break;

            default:
                shape = _shapeDefault;
                baseColor = _colorDefault;
                isDefault = IsDefault(type, _hexDefault);
                break;
        }

        if (Plugins.ScanNodeSprites.TryGetValue(shape, out var entry))
        {
            var spritePair = entry.Asset;

            if (cache.Inner != null && cache.Inner.sprite != spritePair.Inner)
                cache.Inner.sprite = spritePair.Inner;

            if (cache.Outer != null && cache.Outer.sprite != spritePair.Outer)
                cache.Outer.sprite = spritePair.Outer;
        }

        ApplyImageColorOverride(cache, baseColor, !isDefault);

        if (isDefault)
            return;

        float l = (0.299f * baseColor.r) + (0.587f * baseColor.g) + (0.114f * baseColor.b);
        Color readable = l > 0.5f ? Color.black : Color.white;

        TextMeshProUGUI[] texts = cache.Texts;
        if (texts != null)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                TextMeshProUGUI txt = texts[i];
                if (txt == null)
                    continue;

                Color current = txt.color;
                Color target = new(readable.r, readable.g, readable.b, current.a);
                if (current != target)
                    txt.color = target;
            }
        }
    }

    private static void ApplyImageColorOverride(VisualCache cache, Color color, bool enabled)
    {
        Image[] images = cache.Images;
        if (images == null || images.Length == 0)
            return;

        if (cache.ColorEffects == null || cache.ColorEffects.Length != images.Length)
            cache.ColorEffects = new ScanNodeColorMeshEffect[images.Length];

        for (int i = 0; i < images.Length; i++)
        {
            Image img = images[i];
            if (img == null)
                continue;

            ScanNodeColorMeshEffect effect = cache.ColorEffects[i];
            if (effect == null)
            {
                effect = img.GetComponent<ScanNodeColorMeshEffect>();
                
                if (effect == null && !enabled)
                    continue;

                if (effect == null)
                    effect = img.gameObject.AddComponent<ScanNodeColorMeshEffect>();

                cache.ColorEffects[i] = effect;
            }

            if (enabled)
                effect.SetColor(color);
            else
                effect.ClearColor();
        }
    }

    internal static void ApplyAlpha(RectTransform rect, ScanNodeProperties node, float alpha)
    {
        if (rect == null)
            return;

        VisualCache cache = GetOrRefreshCache(rect, node);
        cache.LastSeenFrame = Time.frameCount;

        Image[] images = cache.Images;
        if (images != null)
        {
            for (int i = 0; i < images.Length; i++)
            {
                Image img = images[i];
                if (img == null)
                    continue;

                Color color = img.color;
                if (Mathf.Approximately(color.a, alpha))
                    continue;

                color.a = alpha;
                img.color = color;
            }
        }

        TextMeshProUGUI[] texts = cache.Texts;
        if (texts != null)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                TextMeshProUGUI txt = texts[i];
                if (txt == null)
                    continue;

                Color color = txt.color;
                if (Mathf.Approximately(color.a, alpha))
                    continue;

                color.a = alpha;
                txt.color = color;
            }
        }
    }

    private static void RefreshSettingsIfNeeded()
    {
        string shapeDefault = Plugins.ConfigEntries.ScanNodeShape_Default.Value;
        string shapeScrap = Plugins.ConfigEntries.ScanNodeShape_Scrap.Value;
        string shapeCreature = Plugins.ConfigEntries.ScanNodeShape_Creature.Value;
        string hexDefault = Plugins.ConfigEntries.ScanNodeColor_Default.Value;
        string hexScrap = Plugins.ConfigEntries.ScanNodeColor_Scrap.Value;
        string hexCreature = Plugins.ConfigEntries.ScanNodeColor_Creature.Value;

        if (!_settingsDirty &&
            shapeDefault == _shapeDefault &&
            shapeScrap == _shapeScrap &&
            shapeCreature == _shapeCreature &&
            hexDefault == _hexDefault &&
            hexScrap == _hexScrap &&
            hexCreature == _hexCreature)
        {
            return;
        }

        _settingsDirty = false;
        _shapeDefault = shapeDefault;
        _shapeScrap = shapeScrap;
        _shapeCreature = shapeCreature;
        _hexDefault = hexDefault;
        _hexScrap = hexScrap;
        _hexCreature = hexCreature;

        _colorDefault = HUDUtils.ParseHexColor(_hexDefault);
        _colorScrap = HUDUtils.ParseHexColor(_hexScrap);
        _colorCreature = HUDUtils.ParseHexColor(_hexCreature);
    }

    private static bool IsDefault(ScanNodeType type, string hex)
    {
        return type switch
        {
            ScanNodeType.Scrap => hex.Equals("#38AB00", StringComparison.OrdinalIgnoreCase),
            ScanNodeType.Creature => hex.Equals("#FF0A00", StringComparison.OrdinalIgnoreCase),
            _ => hex.Equals("#0B00B2", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static void PruneCache()
    {
        _deadCacheKeys.Clear();

        foreach (KeyValuePair<RectTransform, VisualCache> pair in _visualCache)
        {
            if (pair.Key == null || Time.frameCount - pair.Value.LastSeenFrame > 600)
                _deadCacheKeys.Add(pair.Key);
        }

        for (int i = 0; i < _deadCacheKeys.Count; i++)
            _visualCache.Remove(_deadCacheKeys[i]);
    }

    internal static void ForceRefresh()
    {
        _settingsDirty = true;
    }

    internal static void Reset()
    {
        foreach (VisualCache cache in _visualCache.Values)
            ApplyImageColorOverride(cache, Color.white, false);

        _visualCache.Clear();
        _deadCacheKeys.Clear();
        _settingsDirty = true;
    }
}

internal sealed class ScanNodeColorMeshEffect : BaseMeshEffect
{
    private Color32 _targetColor = new(255, 255, 255, 255);

    internal void SetColor(Color color)
    {
        Color32 next = color;
        bool changed = _targetColor.r != next.r ||
                       _targetColor.g != next.g ||
                       _targetColor.b != next.b;

        _targetColor = next;

        if (!enabled)
        {
            enabled = true;
            return;
        }

        if (changed && graphic != null)
            graphic.SetVerticesDirty();
    }

    internal void ClearColor()
    {
        if (enabled)
            enabled = false;
    }

    public override void ModifyMesh(VertexHelper vertexHelper)
    {
        if (!IsActive() || vertexHelper == null)
            return;

        UIVertex vertex = default;
        int count = vertexHelper.currentVertCount;

        for (int i = 0; i < count; i++)
        {
            vertexHelper.PopulateUIVertex(ref vertex, i);

            vertex.color = new Color32(
                _targetColor.r,
                _targetColor.g,
                _targetColor.b,
                vertex.color.a);

            vertexHelper.SetUIVertex(vertex, i);
        }
    }
}