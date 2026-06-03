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

    private static PropertyInfo _rectTransformProperty;
    private static PropertyInfo _scanNodePropertiesProperty;
    private static PropertyInfo _hasScanNodeProperty;

    private const BindingFlags StaticFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

    private const BindingFlags InstanceFlags =
        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    internal static IEnumerable<(RectTransform rect, ScanNodeProperties node)> EnumerateAllNodes(
        Dictionary<RectTransform, ScanNodeProperties> vanillaNodes)
    {
        HashSet<RectTransform> seenRects = [];
        List<(RectTransform rect, ScanNodeProperties node)> snapshot = [];

        if (vanillaNodes != null)
        {
            try
            {
                foreach (var kvp in vanillaNodes)
                {
                    if (kvp.Key == null || kvp.Value == null)
                        continue;

                    if (seenRects.Add(kvp.Key))
                        snapshot.Add((kvp.Key, kvp.Value));
                }
            }
            catch
            {
                // scanNodes can be modified by the game or another mod.
                // Keep whatever was already captured this frame.
            }
        }

        if (ModCompats.IsGoodItemScanPresent)
        {
            List<(RectTransform rect, ScanNodeProperties node)> extraNodes = GetGoodItemScanSnapshot();

            foreach (var pair in extraNodes)
            {
                if (pair.rect == null || pair.node == null)
                    continue;

                if (seenRects.Add(pair.rect))
                    snapshot.Add(pair);
            }
        }

        foreach (var pair in snapshot)
            yield return pair;
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

    internal static bool TryRemoveNode(RectTransform rect, ScanNodeProperties node)
    {
        if (!ModCompats.IsGoodItemScanPresent)
            return false;

        if (!TryInit())
            return false;

        try
        {
            object scanner = GetScanner();
            if (scanner == null || _disableScanNodeMethod == null)
                return false;

            object scannedNode = FindScannedNode(rect, node);
            if (scannedNode == null)
                return false;

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
            Reset();
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

    private static List<(RectTransform rect, ScanNodeProperties node)> GetGoodItemScanSnapshot()
    {
        List<(RectTransform rect, ScanNodeProperties node)> result = [];

        if (!TryInit())
            return result;

        try
        {
            object scanner = GetScanner();
            if (scanner == null)
                return result;

            if (_activeNodesField.GetValue(scanner) is not IEnumerable activeNodes)
                return result;

            List<object> snapshot = [];

            foreach (object scanned in activeNodes)
            {
                if (scanned != null)
                    snapshot.Add(scanned);
            }

            foreach (object scanned in snapshot)
            {
                if (!HasScanNode(scanned))
                    continue;

                RectTransform rect = GetRectTransform(scanned);
                ScanNodeProperties node = GetScanNodeProperties(scanned);

                if (rect == null || node == null)
                    continue;

                result.Add((rect, node));
            }
        }
        catch
        {
            // Compatibility should never break vanilla scan nodes.
        }

        return result;
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

            List<object> snapshot = [];

            foreach (object scanned in activeNodes)
            {
                if (scanned != null)
                    snapshot.Add(scanned);
            }

            foreach (object scanned in snapshot)
            {
                if (!HasScanNode(scanned))
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

    internal static void Reset()
    {
        _initialized = false;
        _failed = false;

        _scannerField = null;
        _activeNodesField = null;

        _scanMethod = null;
        _disableScanNodeMethod = null;

        _rectTransformProperty = null;
        _scanNodePropertiesProperty = null;
        _hasScanNodeProperty = null;
    }
}