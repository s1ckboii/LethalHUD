using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using LethalHUD.HUD;
using GameNetcodeStuff;
using TMPro;
using static LethalHUD.Enums;

namespace LethalHUD.Compats;

internal static class EladsHUDProxy
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void OverrideHUD()
    {
        if (!ModCompats.IsEladsHUDPresent) return;

        if (StartOfRound.Instance == null) return;
        PlayerControllerB player = StartOfRound.Instance.localPlayerController;
        if (player == null) return;

        CustomHUD_Mono hudInstance = CustomHUD_Mono.instance;
        if (hudInstance == null) return;

        Image staminaBar = hudInstance.staminaBar;
        TextMeshProUGUI staminaText = hudInstance.staminaText;

        if (staminaBar != null || staminaText != null)
            ApplyStaminaColors(staminaBar, staminaText);

        Image healthBar = hudInstance.healthBar;
        if (healthBar != null)
        {
            healthBar.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.HealthColor.Value);

            Transform parent = healthBar.transform.parent;
            if (parent != null)
            {
                Transform bgContainer = parent.Find("Healthbar BG");
                if (bgContainer != null && bgContainer.TryGetComponent(out Image bg))
                    bg.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SlotColor.Value);
            }
        }

        TextMeshProUGUI carryText = hudInstance.carryText;
        if (carryText != null)
            UpdateWeightDisplay(carryText, player);
    }

    private static void ApplyStaminaColors(Image staminaBar, TextMeshProUGUI staminaText)
    {
        Color baseColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SprintMeterColor.Value);
        float fill = staminaBar != null ? staminaBar.fillAmount : 1f;
        var finalColor = Plugins.ConfigEntries.SprintColoring.Value switch
        {
            SprintStyle.Gradient => HUDUtils.GetGradientColor(baseColor, fill),
            SprintStyle.Shades => HUDUtils.GetShadeColor(baseColor, fill),
            _ => HUDUtils.ParseHexColor(Plugins.ConfigEntries.SprintMeterColor.Value),
        };
        if (staminaBar != null) staminaBar.color = finalColor;
        if (staminaText != null) staminaText.color = finalColor;
    }
    private static void UpdateWeightDisplay(TextMeshProUGUI carryText, PlayerControllerB player)
    {
        if (carryText == null || player == null) return;

        float carryWeight = player.carryWeight;
        int num2 = Mathf.RoundToInt(Mathf.Clamp(carryWeight - 1f, 0f, 100f) * 105f);
        float convertedWeight = WeightController.ConvertWeight(num2);

        string displayText;
        if (Plugins.ConfigEntries.WeightUnitConfig.Value == WeightUnit.Manuls)
        {
            displayText = $"{WeightController.FormatWeight(convertedWeight)} manuls\n{WeightController.GetManulAsciiByWeight(convertedWeight)}";
        }
        else
        {
            displayText = WeightController.GetUnitString(num2, true);
        }

        carryText.enableVertexGradient = true;
        carryText.color = Color.white;

        float maxWeight = Plugins.ConfigEntries.WeightUnitConfig.Value switch
        {
            WeightUnit.Pounds => 130f,
            WeightUnit.Kilograms => 130f * 0.453592f,
            WeightUnit.Manuls => 130f / 9.9f,
            _ => 130f
        };
        float normalizedWeight = Mathf.Clamp01(convertedWeight / maxWeight);

        carryText.colorGradient = HUDUtils.GetWeightGradient(normalizedWeight);
        carryText.text = displayText;
    }
}