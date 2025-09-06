using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using LethalHUD.HUD;
using LethalHUD.Configs;
using GameNetcodeStuff;
using TMPro;

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
                        break;
                    }
                }
                catch (ReflectionTypeLoadException) { continue; }
            }

            if (_customHudType == null)
            {
                if (!_triedFindingType)
                {
                    Loggers.Warning("[CustomHUDProxy] Failed to find CustomHUD_Mono type.");
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

        object hudInstance = _instanceField.GetValue(null);
        if (hudInstance == null)
        {
            _hudInstance = null;
            return;
        }

        if (!ReferenceEquals(_hudInstance, hudInstance))
        {
            _hudInstance = hudInstance;
        }

        PlayerControllerB player = StartOfRound.Instance?.localPlayerController;
        if (player == null) return;

        Image staminaBar = _staminaBarField?.GetValue(hudInstance) as Image;
        TextMeshProUGUI staminaText = _staminaTextField?.GetValue(hudInstance) as TextMeshProUGUI;

        if (staminaBar != null || staminaText != null && Plugins.ConfigEntries.SprintMeterBoolean.Value)
            ApplyStaminaColors(staminaBar, staminaText);

        Image healthBar = _healthBarField?.GetValue(hudInstance) as Image;
        if (healthBar != null && Plugins.ConfigEntries.HealthStarterColor.Value)
        {
            healthBar.color = ConfigHelper.GetSlotColor();

            Transform parent = healthBar.transform.parent;
            if (parent != null)
            {
                Image bg = parent.Find("Healthbar BG")?.GetComponent<Image>();
                if (bg != null)
                    bg.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.UnifyMostColors.Value);
            }
        }

        TextMeshProUGUI carryText = _carryTextField?.GetValue(hudInstance) as TextMeshProUGUI;
        if (carryText != null)
            UpdateWeightDisplay(carryText, player);
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
    private static void UpdateWeightDisplay(TextMeshProUGUI carryText, PlayerControllerB player)
    {
        float carryWeight = player.carryWeight;
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