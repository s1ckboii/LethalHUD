using TMPro;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class WeightController
{
    private static bool _shadowApplied = false;
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

    internal static string FormatWeight(float weight)
    {
        return Plugins.ConfigEntries.WeightDecimalFormatConfig.Value switch
        {
            WeightDecimalFormat.Rounded => $"{weight:F0}",
            WeightDecimalFormat.TwoDecimalsDot => $"{weight:F2}".Replace(',', '.'),
            WeightDecimalFormat.TwoDecimalsComma => $"{weight:F2}",
            _ => $"{weight:F0}"
        };
    }

    internal static string GetUnitString(float weightInLbs, bool useInline = false)
    {
        WeightUnit unit = Plugins.ConfigEntries.WeightUnitConfig.Value;
        WeightUnitDisplay display = Plugins.ConfigEntries.WeightUnitDisplayConfig.Value;

        float kg = weightInLbs * 0.453592f;
        float manuls = weightInLbs / 9.9f;

        string separator = useInline ? " | " : "\n";

        return display switch
        {
            WeightUnitDisplay.KgAndPounds => $"{FormatWeight(kg)} kg{separator}{FormatWeight(weightInLbs)} lb",
            WeightUnitDisplay.PoundsAndManuls => $"{FormatWeight(weightInLbs)} lb{separator}{FormatWeight(manuls)} manuls",
            WeightUnitDisplay.KgAndManuls => $"{FormatWeight(kg)} kg{separator}{FormatWeight(manuls)} manuls",
            WeightUnitDisplay.KgPoundsAndManuls => $"{FormatWeight(kg)} kg{separator}{FormatWeight(weightInLbs)} lb{separator}{FormatWeight(manuls)} manuls",
            _ => $"{FormatWeight(ConvertWeight(weightInLbs))} {GetUnitSingle(unit)}"
        };
    }


    private static string GetUnitSingle(WeightUnit unit)
    {
        return unit switch
        {
            WeightUnit.Pounds => "lb",
            WeightUnit.Kilograms => "kg",
            WeightUnit.Manuls => "manuls",
            _ => "lb"
        };
    }

    internal static void RecolorWeightText()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null || hud.weightCounter == null) return;

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

        hud.weightCounter.color = Color.white;

        hud.weightCounter.colorGradient = HUDUtils.GetWeightGradient(normalizedWeight);
        hud.weightCounter.enableVertexGradient = true;
    }

    private static string GetManulAsciiTired() => " /\\_/\\  \n( -.- )\n z  z  z";
    private static string GetManulAsciiLight() => " /\\_/\\ \n( o.o )\n > ^ < ";
    private static string GetManulAsciiOverloaded() => " /\\_/\\  \n( x_x )\n  ~~~  ";

    internal static string GetManulAsciiByWeight(float manulsCount)
    {
        if (manulsCount < 2) return GetManulAsciiTired();
        if (manulsCount < 9) return GetManulAsciiLight();
        return GetManulAsciiOverloaded();
    }


    internal static void UpdateWeightDisplay()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null || hud.weightCounter == null || hud.weightCounterAnimator == null) return;
        if (GameNetworkManager.Instance?.localPlayerController == null) return;

        float carryWeight = GameNetworkManager.Instance.localPlayerController.carryWeight;
        float num2 = Mathf.Clamp(carryWeight - 1f, 0f, 100f) * 105f;

        float maxWeight = Plugins.ConfigEntries.WeightUnitConfig.Value switch
        {
            WeightUnit.Pounds => 130f,
            WeightUnit.Kilograms => 130f * 0.453592f,
            WeightUnit.Manuls => 130f / 9.9f,
            _ => 130f
        };

        int unitsCount = 1;
        if (Plugins.ConfigEntries.WeightUnitConfig.Value == WeightUnit.Manuls) unitsCount = 2;
        else
        {
            switch (Plugins.ConfigEntries.WeightUnitDisplayConfig.Value)
            {
                case WeightUnitDisplay.KgAndPounds:
                case WeightUnitDisplay.PoundsAndManuls:
                case WeightUnitDisplay.KgAndManuls: unitsCount = 2; break;
                case WeightUnitDisplay.KgPoundsAndManuls: unitsCount = 3; break;
            }
        }

        float scaleReduction = unitsCount switch
        {
            1 => 0.8f,
            2 => 0.6f,
            3 => 0.4f,
            _ => 1f
        };

        float animatorWeight = Mathf.Clamp(num2 / maxWeight, 0f, 1f);
        hud.weightCounterAnimator.SetFloat("weight", animatorWeight * scaleReduction);

        float convertedWeight = ConvertWeight(num2);

        if (Plugins.ConfigEntries.WeightUnitConfig.Value == WeightUnit.Manuls)
        {
            hud.weightCounter.text = $"{FormatWeight(convertedWeight)} manuls\n{GetManulAsciiByWeight(convertedWeight)}";
        }
        else
        {
            hud.weightCounter.text = GetUnitString(num2);
        }

        hud.weightCounter.color = Color.white;
        hud.weightCounter.enableVertexGradient = true;
        hud.weightCounter.extraPadding = true;

        if (!_shadowApplied)
        {
            Material mat = hud.weightCounter.fontMaterial;
            mat.EnableKeyword("UNDERLAY_ON");
            mat.SetColor(ShaderUtilities.ID_UnderlayColor, Color.black);
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.5f);
            mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.5f);
            _shadowApplied = true;
        }

        hud.weightCounter.colorGradient = HUDUtils.GetWeightGradient(animatorWeight);
    }
}