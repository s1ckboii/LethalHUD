using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LethalHUD.Compats;
internal class GoodItemScanProxy // Credits to Xu for the whole class
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetRectTransform(ScanNodeProperties scanNodeProperties, [NotNullWhen(true)] out RectTransform rectTransform)
    {
        rectTransform = null;
        if (GoodItemScan.GoodItemScan.scanner == null)
        {
            return false;
        }

        if (GoodItemScan.GoodItemScan.scanner._scanNodes.TryGetValue(scanNodeProperties, out int index))
        {
            rectTransform = GoodItemScan.GoodItemScan.scanner._scannedNodes[index].rectTransform;
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool TryGetScanNode(RectTransform rectTransform, [NotNullWhen(true)] out ScanNodeProperties scanNodeProperties)
    {
        scanNodeProperties = null;
        if (GoodItemScan.GoodItemScan.scanner == null)
        {
            return false;
        }

        foreach ((ScanNodeProperties scanNode, int index) in GoodItemScan.GoodItemScan.scanner._scanNodes)
        {
            GoodItemScan.ScannedNode scannedNode = GoodItemScan.GoodItemScan.scanner._scannedNodes[index];
            if (rectTransform != scannedNode.rectTransform)
            {
                continue;
            }

            scanNodeProperties = scanNode;
            return true;
        }

        return false;
    }
}
