using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LethalHUD.HUD;
using LethalHUD.Configs;
using GameNetcodeStuff;

namespace LethalHUD.Compats;
internal static class EladsHUDProxy
{
    private static Type _customHudType;
    private static FieldInfo _instanceField;
    private static FieldInfo _healthBarField;
    private static FieldInfo _staminaBarField;
    private static FieldInfo _staminaTextField;
    private static FieldInfo _carryTextField;

    private static object _hudInstance;
    private static bool _triedFindingType = false;

    private static Image _cachedStaminaBar;
    private static TextMeshProUGUI _cachedStaminaText;
    private static Image _cachedHealthBar;
    private static TextMeshProUGUI _cachedCarryText;

    private static float _lastWeightValue = -1f;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void OverrideHUD()
    {
        if (!ModCompats.IsEladsHUDPresent) return;

        if (_customHudType == null)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    _customHudType = asm.GetTypes().FirstOrDefault(x => x.Name == "CustomHUD_Mono");
                    if (_customHudType != null)
                    {
                        Plugins.Logger.LogInfo($"[CustomHUDProxy] Found CustomHUD_Mono in {asm.GetName().Name}");
                        break;
                    }
                }
                catch (ReflectionTypeLoadException) { continue; }
            }

            if (_customHudType == null)
            {
                if (!_triedFindingType)
                {
                    Plugins.Logger.LogWarning("[CustomHUDProxy] Failed to find CustomHUD_Mono type.");
                    _triedFindingType = true;
                }
                return;
            }

            _instanceField = _customHudType.GetField("instance", BindingFlags.Static | BindingFlags.Public);
            _healthBarField = _customHudType.GetField("healthBar", BindingFlags.Instance | BindingFlags.Public);
            _staminaBarField = _customHudType.GetField("staminaBar", BindingFlags.Instance | BindingFlags.Public);
            _staminaTextField = _customHudType.GetField("staminaText", BindingFlags.Instance | BindingFlags.Public);
            _carryTextField = _customHudType.GetField("carryText", BindingFlags.Instance | BindingFlags.Public);
        }

        if (_instanceField == null) return;

        _hudInstance ??= _instanceField.GetValue(null);
        if (_hudInstance == null) return;

        PlayerControllerB player = StartOfRound.Instance?.localPlayerController;
        if (player == null) return;

        if (_cachedStaminaBar == null)
            _cachedStaminaBar = _staminaBarField?.GetValue(_hudInstance) as Image;

        if (_cachedStaminaText == null)
            _cachedStaminaText = _staminaTextField?.GetValue(_hudInstance) as TextMeshProUGUI;

        if (_cachedHealthBar == null)
            _cachedHealthBar = _healthBarField?.GetValue(_hudInstance) as Image;

        if (_cachedCarryText == null)
            _cachedCarryText = _carryTextField?.GetValue(_hudInstance) as TextMeshProUGUI;

        if (_cachedStaminaBar != null || _cachedStaminaText != null)
            ApplyStaminaColors(_cachedStaminaBar, _cachedStaminaText);

        if (_cachedHealthBar != null && Plugins.ConfigEntries.HealthStarterColor.Value)
        {
            _cachedHealthBar.color = ConfigHelper.GetSlotColor();
            Transform parent = _cachedHealthBar.transform.parent;
            if (parent != null)
            {
                Image bg = parent.Find("Healthbar BG")?.GetComponent<Image>();
                if (bg != null)
                    bg.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.UnifyMostColors.Value);
            }
        }

        if (_cachedCarryText != null)
        {
            float carryWeight = player.carryWeight;
            if (!Mathf.Approximately(carryWeight, _lastWeightValue))
            {
                UpdateWeightDisplay(_cachedCarryText, carryWeight);
                _lastWeightValue = carryWeight;
            }
        }
    }

    private static void ApplyStaminaColors(Image staminaBar, TextMeshProUGUI staminaText)
    {
        string lastMode = PlayerPrefs.GetString(SprintMeter.PlayerPrefsKey, "Solid");
        Color finalColor;

        switch (lastMode)
        {
            case "Solid":
                finalColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SprintMeterColorSolid.Value);
                break;

            case "Shades":
                {
                    Color baseColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SprintMeterColorShades.Value);
                    float fill = staminaBar != null ? staminaBar.fillAmount : 1f;
                    finalColor = HUDUtils.GetShadeColor(baseColor, fill);
                }
                break;

            default:
                {
                    Color baseColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SprintMeterColorGradient.Value);
                    float fill = staminaBar != null ? staminaBar.fillAmount : 1f;
                    finalColor = HUDUtils.GetGradientColor(baseColor, fill);
                }
                break;
        }

        if (staminaBar != null) staminaBar.color = finalColor;
        if (staminaText != null) staminaText.color = finalColor;
    }

    private static void UpdateWeightDisplay(TextMeshProUGUI carryText, float carryWeight)
    {
        float convertedWeight = WeightController.ConvertWeight(Mathf.RoundToInt(Mathf.Clamp(carryWeight - 1f, 0f, 100f) * 105f));
        string unitString = WeightController.GetUnitString();

        float maxWeight = Plugins.ConfigEntries.WeightUnitConfig.Value switch
        {
            Enums.WeightUnit.Pounds => 130f,
            Enums.WeightUnit.Kilograms => 130f * 0.453592f,
            Enums.WeightUnit.Manuls => 130f / 9.9f,
            _ => 130f
        };

        float normalizedWeight = Mathf.Clamp01(convertedWeight / maxWeight);
        Color color = HUDUtils.GetWeightColor(normalizedWeight);

        carryText.text = $"{convertedWeight:F0} {unitString}";
        carryText.color = color;
    }
}
