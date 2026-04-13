using static LethalHUD.Enums;

namespace LethalHUD.Scan;
internal static class ScanNodeClassifier
{
    internal static ScanNodeType GetType(ScanNodeProperties node)
    {
        if (node == null) return ScanNodeType.Default;

        return node.nodeType switch
        {
            2 => ScanNodeType.Scrap,
            1 => ScanNodeType.Creature,
            _ => ScanNodeType.Default
        };
    }
}