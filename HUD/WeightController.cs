using System.Globalization;
using LethalHUD.CustomHUD;
using TMPro;
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

    internal static string FormatWeight(float weight)
    {
        return Plugins.ConfigEntries.WeightDecimalFormatConfig.Value switch
        {
            WeightDecimalFormat.Rounded => weight.ToString("F0"),
            WeightDecimalFormat.TwoDecimalsDot => weight.ToString("F2", CultureInfo.InvariantCulture),
            WeightDecimalFormat.TwoDecimalsComma => weight.ToString("F2"),
            _ => weight.ToString("F0")
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

    private static TextMeshProUGUI GetActiveWeightText(HUDManager hud)
    {
        if (CustomHealthBar.UsingCustom && CustomHealthBar.CustomWeightNumber != null)
            return CustomHealthBar.CustomWeightNumber;

        return hud?.weightCounter;
    }

    internal static void RecolorWeightText()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null) return;

        TextMeshProUGUI weightText = GetActiveWeightText(hud);
        if (weightText == null) return;

        string text = weightText.text;
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

        weightText.color = Color.white;
        weightText.colorGradient = HUDUtils.GetWeightGradient(normalizedWeight);
        weightText.enableVertexGradient = true;
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

    private static string GetConfigWeightText(float weightInLbs)
    {
        float convertedWeight = ConvertWeight(weightInLbs);

        if (Plugins.ConfigEntries.WeightUnitConfig.Value == WeightUnit.Manuls)
        {
            return $"{FormatWeight(convertedWeight)} manuls\n{GetManulAsciiByWeight(convertedWeight)}";
        }

        return GetUnitString(weightInLbs);
    }

    private static string GetWeightText(float weightInLbs)
    {
        WeightDisplayLayout layout = CustomHealthBar.ActiveWeightLayout;

        return layout switch
        {
            WeightDisplayLayout.Horizontal => GetUnitString(weightInLbs, true),

            WeightDisplayLayout.Vertical => GetUnitString(weightInLbs, false),

            WeightDisplayLayout.ASCII =>
                $"{FormatWeight(weightInLbs / 9.9f)} manuls\n{GetManulAsciiByWeight(weightInLbs / 9.9f)}",

            _ => GetConfigWeightText(weightInLbs)
        };
    }

    private static int GetUnitsCount()
    {
        WeightDisplayLayout layout = CustomHealthBar.ActiveWeightLayout;

        if (layout == WeightDisplayLayout.ASCII)
            return 2;

        if (layout == WeightDisplayLayout.Horizontal)
            return 1;

        if (Plugins.ConfigEntries.WeightUnitConfig.Value == WeightUnit.Manuls)
            return 2;

        return Plugins.ConfigEntries.WeightUnitDisplayConfig.Value switch
        {
            WeightUnitDisplay.KgAndPounds => 2,
            WeightUnitDisplay.PoundsAndManuls => 2,
            WeightUnitDisplay.KgAndManuls => 2,
            WeightUnitDisplay.KgPoundsAndManuls => 3,
            _ => 1
        };
    }

    private static float GetMaxWeight()
    {
        return Plugins.ConfigEntries.WeightUnitConfig.Value switch
        {
            WeightUnit.Pounds => 130f,
            WeightUnit.Kilograms => 130f * 0.453592f,
            WeightUnit.Manuls => 130f / 9.9f,
            _ => 130f
        };
    }

    private static void ApplyWeightTextStyle(TextMeshProUGUI weightText, float animatorWeight)
    {
        weightText.color = Color.white;
        weightText.enableVertexGradient = true;
        weightText.extraPadding = true;
        weightText.colorGradient = HUDUtils.GetWeightGradient(animatorWeight);

        Material mat = weightText.fontMaterial;
        mat.EnableKeyword("UNDERLAY_ON");
        mat.SetColor(ShaderUtilities.ID_UnderlayColor, Color.black);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.5f);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.5f);
    }

    internal static void UpdateWeightDisplay()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null || hud.weightCounter == null || hud.weightCounterAnimator == null) return;
        if (GameNetworkManager.Instance?.localPlayerController == null) return;

        TextMeshProUGUI weightText = GetActiveWeightText(hud);
        if (weightText == null) return;

        float carryWeight = GameNetworkManager.Instance.localPlayerController.carryWeight;
        float weightInLbs = Mathf.Clamp(carryWeight - 1f, 0f, 100f) * 105f;

        int unitsCount = GetUnitsCount();

        float scaleReduction = unitsCount switch
        {
            1 => 0.8f,
            2 => 0.6f,
            3 => 0.4f,
            _ => 1f
        };

        float maxWeight = GetMaxWeight();
        float animatorWeight = Mathf.Clamp(weightInLbs / maxWeight, 0f, 1f);

        hud.weightCounterAnimator.SetFloat("weight", animatorWeight * scaleReduction);

        weightText.text = GetWeightText(weightInLbs);

        ApplyWeightTextStyle(weightText, animatorWeight);
    }
}