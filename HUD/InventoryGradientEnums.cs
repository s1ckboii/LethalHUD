using LethalHUD.Configs;

namespace LethalHUD.HUD;

public class InventoryGradientEnums
{
    public enum SlotEnums
    {
        None,
        Rainbow,
        Summer,
        Winter,
        Vaporwave,
        Deepmint,
        Radioactive,
        TideEmber
    }

    public static bool HasCustomGradient()
    {
        string a = ConfigEntries.Instance.GradientColorA.Value;
        string b = ConfigEntries.Instance.GradientColorB.Value;

        return !string.IsNullOrWhiteSpace(a)
            && !string.IsNullOrWhiteSpace(b)
            && !string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
    }
}
