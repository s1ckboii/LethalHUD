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
        string a = Plugins.ConfigEntries.GradientColorA.Value;
        string b = Plugins.ConfigEntries.GradientColorB.Value;

        return !string.IsNullOrWhiteSpace(a)
            && !string.IsNullOrWhiteSpace(b)
            && !string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
    }
}
