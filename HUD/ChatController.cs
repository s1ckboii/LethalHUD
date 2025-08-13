using LethalHUD.Configs;
using System;
using TMPro;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class ChatController
{
    internal static string GetColoredPlayerName(string playerName)
    {
        if (!Plugins.ConfigEntries.ColoredNames.Value || string.IsNullOrEmpty(playerName))
            return playerName;

        string gradientA = Plugins.ConfigEntries.GradientNameColorA.Value;
        string gradientB = Plugins.ConfigEntries.GradientNameColorB.Value;
        string solidColor = Plugins.ConfigEntries.LocalNameColor.Value;

        bool useGradient = !(gradientA.Equals("#FF0000", StringComparison.OrdinalIgnoreCase) &&
                             gradientB.Equals("#FF0000", StringComparison.OrdinalIgnoreCase));

        if (useGradient)
        {
            ColorUtility.TryParseHtmlString(gradientA, out Color colorA);
            ColorUtility.TryParseHtmlString(gradientB, out Color colorB);
            return HUDUtils.ApplyStaticGradient(playerName, colorA, colorB);
        }
        else
        {
            return $"<color={solidColor}>{playerName}</color>";
        }
    }
    internal static string GetDefaultChatColorTag()
    {
        Color color = ConfigHelper.GetSlotColor();
        string hex = ColorUtility.ToHtmlStringRGB(color);
        return $"<color=#{hex}>";
    }

    internal static void PlayerTypingIndicator()
    {
        var indicator = HUDManager.Instance.typingIndicator;
        indicator.enableVertexGradient = true;
        indicator.color = Color.white;
        if (indicator == null)
            return;

        if (HUDUtils.HasCustomGradient(Plugins.ConfigEntries.GradientColorA.Value, Plugins.ConfigEntries.GradientColorB.Value))
        {
            if (ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorA.Value, out Color startColor) &&
                ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorB.Value, out Color endColor))
            {
                HUDUtils.ApplyVertexGradient(indicator, startColor, endColor, Time.time * 0.2f);
                return;
            }
        }

        switch (InventoryFrames.CurrentSlotColorMode)
        {
            case SlotEnums.Rainbow:
                HUDUtils.ApplyRainbowGradient(indicator, Time.time * 0.25f);
                break;

            case SlotEnums.None:
                indicator.colorGradient = new VertexGradient(ConfigHelper.GetSlotColor());
                break;

            default:
                HUDUtils.ApplyVertexGradient(indicator, InventoryFrames.CurrentGradientStartColor, InventoryFrames.CurrentGradientEndColor, Time.time * 0.25f);
                break;
        }
    }

    internal static void ColorChatInputField(TMP_InputField inputField, float time)
    {
        Color targetColor;

        if (HUDUtils.HasCustomGradient(Plugins.ConfigEntries.GradientColorA.Value, Plugins.ConfigEntries.GradientColorB.Value))
        {
            if (ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorA.Value, out Color startColor) &&
                ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorB.Value, out Color endColor))
            {
                float wave = Mathf.SmoothStep(0f, 1f, Mathf.Sin(time * Mathf.PI * 2f) * 0.5f + 0.5f);
                targetColor = Color.Lerp(startColor, endColor, wave);
            }
            else
            {
                targetColor = ConfigHelper.GetSlotColor();
            }
        }
        else
        {
            switch (InventoryFrames.CurrentSlotColorMode)
            {
                case SlotEnums.Rainbow:
                    float speed = 0.5f;
                    float hueShift = (time * speed) % 1f;
                    targetColor = Color.HSVToRGB(hueShift, 1f, 1f);
                    break;

                case SlotEnums.None:
                    targetColor = ConfigHelper.GetSlotColor();
                    break;

                default:
                    float wave = Mathf.SmoothStep(0f, 1f, Mathf.Sin(time * Mathf.PI * 2f) * 0.5f + 0.5f);
                    targetColor = Color.Lerp(InventoryFrames.CurrentGradientStartColor, InventoryFrames.CurrentGradientEndColor, wave);
                    break;
            }
        }

        inputField.caretColor = targetColor;

        if (inputField.textComponent != null)
        {
            Color invertedColor = new(1f - targetColor.r, 1f - targetColor.g, 1f - targetColor.b, targetColor.a);
            inputField.textComponent.color = invertedColor;
        }
        if (inputField.placeholder is TMP_Text placeholderText)
        {
            placeholderText.color = targetColor;
        }
    }
}
