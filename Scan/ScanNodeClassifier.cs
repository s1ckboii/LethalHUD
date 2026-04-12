using static LethalHUD.Enums;

namespace LethalHUD.Scan;
internal static class ScanNodeClassifier
{
    internal static ScanNodeType GetType(ScanNodeProperties node)
    {
        if (node == null) return ScanNodeType.Default;

        if (node.scrapValue > 0)
            return ScanNodeType.Scrap;

        if (node.creatureScanID != -1)
            return ScanNodeType.Creature;

        return ScanNodeType.Default;
    }
}