using System.Globalization;
using LethalHUD.CustomHUD;
using TMPro;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;

internal static class WeightController
{
    private static readonly int WeightAnimatorHash = Animator.StringToHash("weight");

    private static TextMeshProUGUI _lastTarget;
    private static Material _styledMaterial;
    private static string _cachedDisplayText;
    private static float _lastWeightInLbs = float.NaN;
    private static WeightUnit _lastUnit;
    private static WeightUnitDisplay _lastUnitDisplay;
    private static WeightDecimalFormat _lastDecimalFormat;
    private static WeightDisplayLayout _lastLayout;
    private static bool _hasCachedText;

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
            return $"{FormatWeight(convertedWeight)} manuls\n{GetManulAsciiByWeight(convertedWeight)}";

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

    private static void EnsureMaterialStyle(TextMeshProUGUI weightText)
    {
        if (_lastTarget == weightText && _styledMaterial != null)
            return;

        _lastTarget = weightText;
        _styledMaterial = weightText.fontMaterial;

        if (_styledMaterial == null)
            return;

        _styledMaterial.EnableKeyword("UNDERLAY_ON");
        _styledMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, Color.black);
        _styledMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.5f);
        _styledMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.5f);
    }

    private static bool DisplayTextNeedsRefresh(float weightInLbs)
    {
        WeightUnit unit = Plugins.ConfigEntries.WeightUnitConfig.Value;
        WeightUnitDisplay display = Plugins.ConfigEntries.WeightUnitDisplayConfig.Value;
        WeightDecimalFormat decimalFormat = Plugins.ConfigEntries.WeightDecimalFormatConfig.Value;
        WeightDisplayLayout layout = CustomHealthBar.ActiveWeightLayout;

        bool changed = !_hasCachedText ||
            !Mathf.Approximately(_lastWeightInLbs, weightInLbs) ||
            _lastUnit != unit ||
            _lastUnitDisplay != display ||
            _lastDecimalFormat != decimalFormat ||
            _lastLayout != layout;

        if (!changed)
            return false;

        _lastWeightInLbs = weightInLbs;
        _lastUnit = unit;
        _lastUnitDisplay = display;
        _lastDecimalFormat = decimalFormat;
        _lastLayout = layout;
        _hasCachedText = true;
        return true;
    }

    internal static void UpdateWeightDisplay()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null || hud.weightCounter == null || hud.weightCounterAnimator == null)
            return;

        if (GameNetworkManager.Instance?.localPlayerController == null)
            return;

        TextMeshProUGUI weightText = GetActiveWeightText(hud);
        if (weightText == null)
            return;

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

        float normalizedWeight = Mathf.Clamp01(weightInLbs / 130f);
        hud.weightCounterAnimator.SetFloat(WeightAnimatorHash, normalizedWeight * scaleReduction);

        if (DisplayTextNeedsRefresh(weightInLbs))
            _cachedDisplayText = GetWeightText(weightInLbs);

        if (_cachedDisplayText != null && weightText.text != _cachedDisplayText)
            weightText.text = _cachedDisplayText;

        if (weightText.enableVertexGradient)
            weightText.enableVertexGradient = false;

        Color weightColor = HUDUtils.GetWeightColor(normalizedWeight);
        if (weightText.color != weightColor)
            weightText.color = weightColor;

        if (!weightText.extraPadding)
            weightText.extraPadding = true;

        EnsureMaterialStyle(weightText);
    }

    internal static void ResetCache()
    {
        _lastTarget = null;
        _styledMaterial = null;
        _cachedDisplayText = null;
        _lastWeightInLbs = float.NaN;
        _hasCachedText = false;
    }
}