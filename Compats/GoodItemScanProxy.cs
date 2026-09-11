using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LethalHUD.Compats;

internal static class GoodItemScanProxy
{
    private const string PluginTypeName = "GoodItemScan.GoodItemScan";
    private const string ScannedNodeTypeName = "GoodItemScan.ScannedNode";

    private static bool _initialized;
    private static bool _failed;

    private static FieldInfo _scannerField;
    private static FieldInfo _activeNodesField;

    private static MethodInfo _scanMethod;
    private static MethodInfo _disableScanNodeMethod;
    private static MethodInfo _disableScanNodeWithAnimationMethod;

    private static PropertyInfo _rectTransformProperty;
    private static PropertyInfo _scanNodePropertiesProperty;
    private static PropertyInfo _hasScanNodeProperty;

    private static readonly HashSet<RectTransform> _snapshotSeenRects = [];
    private static readonly HashSet<RectTransform> _goodItemScanRects = [];
    private static readonly List<(RectTransform rect, ScanNodeProperties node)> _snapshot = [];
    private static Dictionary<RectTransform, ScanNodeProperties> _snapshotSource;
    private static int _snapshotFrame = -1;

    private const BindingFlags StaticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
    private const BindingFlags InstanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    internal static IReadOnlyList<(RectTransform rect, ScanNodeProperties node)> EnumerateAllNodes(Dictionary<RectTransform, ScanNodeProperties> vanillaNodes)
    {
        if (_snapshotFrame == Time.frameCount && ReferenceEquals(_snapshotSource, vanillaNodes))
            return _snapshot;

        _snapshotFrame = Time.frameCount;
        _snapshotSource = vanillaNodes;
        _snapshot.Clear();
        _snapshotSeenRects.Clear();
        _goodItemScanRects.Clear();

        if (vanillaNodes != null)
        {
            try
            {
                foreach (KeyValuePair<RectTransform, ScanNodeProperties> kvp in vanillaNodes)
                {
                    RectTransform rect = kvp.Key;
                    ScanNodeProperties node = kvp.Value;

                    if (rect == null || node == null)
                        continue;

                    if (_snapshotSeenRects.Add(rect))
                        _snapshot.Add((rect, node));
                }
            }
            catch
            {
                // scannodes may be modified by the game or another mod.
            }
        }

        if (ModCompats.IsGoodItemScanPresent)
            AppendGoodItemScanNodes();

        return _snapshot;
    }

    internal static bool TryScan()
    {
        if (!ModCompats.IsGoodItemScanPresent)
            return false;

        if (!TryInit())
            return false;

        try
        {
            object scanner = GetScanner();
            if (scanner == null || _scanMethod == null)
                return false;

            _scanMethod.Invoke(scanner, null);
            return true;
        }
        catch
        {
            return false;
        }
    }

    internal static bool IsGoodItemScanNode(RectTransform rect)
    {
        return rect != null && _goodItemScanRects.Contains(rect);
    }

    internal static bool TryRemoveNode(RectTransform rect, ScanNodeProperties node)
    {
        if (!ModCompats.IsGoodItemScanPresent)
            return false;

        if (!TryInit())
            return false;

        object scanner = GetScanner();
        if (scanner == null)
            return false;

        object scannedNode = FindScannedNode(rect, node);
        if (scannedNode == null)
            return false;

        if (_disableScanNodeWithAnimationMethod != null)
        {
            try
            {
                object result = _disableScanNodeWithAnimationMethod.Invoke(scanner, [scannedNode]);
                if (result is IEnumerator routine && HUDManager.Instance != null)
                {
                    HUDManager.Instance.StartCoroutine(routine);
                    return true;
                }
            }
            catch
            {
                // Fall back to the non-animated removal path.
            }
        }

        if (_disableScanNodeMethod == null)
            return false;

        try
        {
            _disableScanNodeMethod.Invoke(scanner, [scannedNode]);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryInit()
    {
        if (_initialized)
            return true;

        if (_failed)
            return false;

        if (!ModCompats.IsGoodItemScanPresent)
            return false;

        try
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name != ModCompats.GoodItemScan_PLUGIN_GUID)
                    continue;

                Type pluginType = assembly.GetType(PluginTypeName, false);
                Type scannedNodeType = assembly.GetType(ScannedNodeTypeName, false);

                if (pluginType == null || scannedNodeType == null)
                    continue;

                _scannerField = pluginType.GetField("scanner", StaticFlags);

                Type scannerType = _scannerField?.FieldType;
                if (scannerType == null)
                    continue;

                _activeNodesField = scannerType.GetField("activeNodes", InstanceFlags);
                _scanMethod = scannerType.GetMethod("Scan", InstanceFlags);
                _disableScanNodeMethod = scannerType.GetMethod("DisableScanNode", InstanceFlags);
                _disableScanNodeWithAnimationMethod = scannerType.GetMethod("DisableScanNodeWithAnimation", InstanceFlags);

                _rectTransformProperty = scannedNodeType.GetProperty("RectTransform", InstanceFlags);
                _scanNodePropertiesProperty = scannedNodeType.GetProperty("ScanNodeProperties", InstanceFlags);
                _hasScanNodeProperty = scannedNodeType.GetProperty("HasScanNode", InstanceFlags);

                _initialized =
                    _scannerField != null &&
                    _activeNodesField != null &&
                    _rectTransformProperty != null &&
                    _scanNodePropertiesProperty != null;

                if (_initialized)
                    return true;
            }
        }
        catch
        {
            ResetReflection();
        }

        _failed = true;
        return false;
    }

    private static object GetScanner()
    {
        try
        {
            return _scannerField?.GetValue(null);
        }
        catch
        {
            return null;
        }
    }

    private static void AppendGoodItemScanNodes()
    {
        if (!TryInit())
            return;

        try
        {
            object scanner = GetScanner();
            if (scanner == null)
                return;

            if (_activeNodesField.GetValue(scanner) is not IEnumerable activeNodes)
                return;

            foreach (object scanned in activeNodes)
            {
                if (scanned == null || !HasScanNode(scanned))
                    continue;

                RectTransform rect = GetRectTransform(scanned);
                ScanNodeProperties node = GetScanNodeProperties(scanned);

                if (rect == null || node == null)
                    continue;

                _goodItemScanRects.Add(rect);

                if (_snapshotSeenRects.Add(rect))
                    _snapshot.Add((rect, node));
            }
        }
        catch
        {
            // Compatibility should never break vanilla scan nodes.
        }
    }

    private static object FindScannedNode(RectTransform rect, ScanNodeProperties node)
    {
        try
        {
            object scanner = GetScanner();
            if (scanner == null)
                return null;

            if (_activeNodesField.GetValue(scanner) is not IEnumerable activeNodes)
                return null;

            foreach (object scanned in activeNodes)
            {
                if (scanned == null || !HasScanNode(scanned))
                    continue;

                RectTransform scannedRect = GetRectTransform(scanned);
                ScanNodeProperties scannedNode = GetScanNodeProperties(scanned);

                if (rect != null && scannedRect == rect)
                    return scanned;

                if (node != null && scannedNode == node)
                    return scanned;
            }
        }
        catch
        {
            return null;
        }

        return null;
    }

    private static bool HasScanNode(object scanned)
    {
        try
        {
            if (_hasScanNodeProperty == null)
                return true;

            return _hasScanNodeProperty.GetValue(scanned) is true;
        }
        catch
        {
            return true;
        }
    }

    private static RectTransform GetRectTransform(object scanned)
    {
        try
        {
            return _rectTransformProperty?.GetValue(scanned) as RectTransform;
        }
        catch
        {
            return null;
        }
    }

    private static ScanNodeProperties GetScanNodeProperties(object scanned)
    {
        try
        {
            return _scanNodePropertiesProperty?.GetValue(scanned) as ScanNodeProperties;
        }
        catch
        {
            return null;
        }
    }

    private static void ResetReflection()
    {
        _initialized = false;
        _failed = false;

        _scannerField = null;
        _activeNodesField = null;

        _scanMethod = null;
        _disableScanNodeMethod = null;
        _disableScanNodeWithAnimationMethod = null;

        _rectTransformProperty = null;
        _scanNodePropertiesProperty = null;
        _hasScanNodeProperty = null;
    }

    internal static void Reset()
    {
        ResetReflection();

        _snapshot.Clear();
        _snapshotSeenRects.Clear();
        _goodItemScanRects.Clear();
        _snapshotSource = null;
        _snapshotFrame = -1;
    }
}