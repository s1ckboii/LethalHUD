using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class WeightController
{
    internal static float ConvertWeight(float weightInLbs)
    {
        return Plugins.ConfigEntries.WeightUnitConfig.Value switch
        {
            WeightUnit.Pounds => weightInLbs,
            WeightUnit.Kilograms => weightInLbs * 0.453592f,
            WeightUnit.Manuls => weightInLbs / 9.9f,
            _ => weightInLbs,
        };
    }

    internal static string GetUnitString()
    {
        return Plugins.ConfigEntries.WeightUnitConfig.Value switch
        {
            WeightUnit.Pounds => "lb",
            WeightUnit.Kilograms => "kg",
            WeightUnit.Manuls => "manuls",
            _ => "lb"
        };
    }
    internal static void RecolorWeightText()
    {
        var hud = HUDManager.Instance;
        if (hud == null || hud.weightCounter == null)
            return;

        string text = hud.weightCounter.text;
        string[] parts = text.Split(' ');
        if (parts.Length < 2) return;

        if (!float.TryParse(parts[0], out float weightNum)) return;

        string unit = parts[1].Split('\n')[0].ToLower().Trim();

        float maxWeight = unit switch
        {
            "manuls" => 130f / 9.9f,
            "kg" => 130f * 0.453592f,
            _ => 130f
        };

        float normalizedWeight = Mathf.Clamp01(weightNum / maxWeight);

        Color color = HUDUtils.GetWeightColor(normalizedWeight);
        hud.weightCounter.color = color;
    }


    private static string GetManulAsciiTired()
    {
        return " /\\_/\\  \n( -.- )\n z  z  z";
    }
    private static string GetManulAsciiLight()
    {
        return " /\\_/\\ \n( o.o )\n > ^ < ";
    }

    private static string GetManulAsciiOverloaded()
    {
        return " /\\_/\\  \n( x_x )\n  ~~~  ";
    }
    private static string GetManulAsciiByWeight(float manulsCount)
    {
        if (manulsCount < 2)
            return GetManulAsciiTired();
        else if (manulsCount < 9)
            return GetManulAsciiLight();
        else
            return GetManulAsciiOverloaded();
    }
    internal static void UpdateWeightDisplay()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null || hud.weightCounter == null || hud.weightCounterAnimator == null)
            return;

        float carryWeight = GameNetworkManager.Instance.localPlayerController.carryWeight;
        int num2 = Mathf.RoundToInt(Mathf.Clamp(carryWeight - 1f, 0f, 100f) * 105f);

        hud.weightCounterAnimator.SetFloat("weight", num2 / 130f);

        float convertedWeight = ConvertWeight(num2);
        string unitString = GetUnitString();


        float maxWeight = Plugins.ConfigEntries.WeightUnitConfig.Value switch
        {
            WeightUnit.Pounds => 130f,
            WeightUnit.Kilograms => 130f * 0.453592f,
            WeightUnit.Manuls => 130f / 9.9f,
            _ => 130f
        };

        float normalizedWeight = Mathf.Clamp01(convertedWeight / maxWeight);
        Color color = HUDUtils.GetWeightColor(normalizedWeight);

        if (Plugins.ConfigEntries.WeightUnitConfig.Value == WeightUnit.Manuls)
        {
            hud.weightCounter.text = $"{convertedWeight:F0} manuls\n{GetManulAsciiByWeight(convertedWeight)}";
            hud.weightCounter.color = color;
        }
        else
        {
            hud.weightCounter.text = $"{convertedWeight:F0} {unitString}";
            hud.weightCounter.color = color;
        }
    }
}