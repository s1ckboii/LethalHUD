using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LethalHUD.Compats;
internal static class GoodItemScanProxy
{
    private static bool _initialized;
    private static FieldInfo _scannerField;
    private static FieldInfo _activeNodesField;

    internal static IEnumerable<(RectTransform rect, ScanNodeProperties node)> EnumerateAllNodes(Dictionary<RectTransform, ScanNodeProperties> vanillaNodes)
    {
        foreach (var kvp in vanillaNodes)
            yield return (kvp.Key, kvp.Value);

        if (!ModCompats.IsGoodItemScanPresent)
            yield break;

        TryInit();
        if (_scannerField == null || _activeNodesField == null)
            yield break;

        List<(RectTransform, ScanNodeProperties)> extraNodes = null;

        try
        {
            var scanner = _scannerField.GetValue(null);
            if (scanner == null)
                yield break;

            if (_activeNodesField.GetValue(scanner) is not IEnumerable activeNodes)
                yield break;

            extraNodes = [];

            foreach (var scanned in activeNodes)
            {
                var rect = scanned.GetType().GetField("rectTransform")?.GetValue(scanned) as RectTransform;
                var node = scanned.GetType().GetField("ScanNodeProperties")?.GetValue(scanned) as ScanNodeProperties;

                if (rect == null || node == null)
                    continue;

                extraNodes.Add((rect, node));
            }
        }
        catch
        {
            yield break;
        }

        if (extraNodes == null)
            yield break;

        foreach (var n in extraNodes)
            yield return n;
    }

    private static void TryInit()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var a in assemblies)
            {
                if (a.GetName().Name == "TestAccount666.GoodItemScan")
                {
                    var type = a.GetType("GoodItemScan.GoodItemScan");
                    _scannerField = type?.GetField("scanner", BindingFlags.Public | BindingFlags.Static);

                    var scannerType = _scannerField?.FieldType;
                    _activeNodesField = scannerType?.GetField("activeNodes", BindingFlags.Public | BindingFlags.Instance);
                    break;
                }
            }
        }
        catch { }
    }
}