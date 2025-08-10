using LethalHUD.Configs;
using System;
using TMPro;
using UnityEngine;
using static LethalHUD.HUD.InventoryGradientEnums;

namespace LethalHUD.HUD;
public static class ChatController
{
    public static string GetColoredPlayerName(string playerName)
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
            return ApplyStaticGradient(playerName, colorA, colorB);
        }
        else
        {
            return $"<color={solidColor}>{playerName}</color>";
        }
    }

    private static string ApplyStaticGradient(string input, Color startColor, Color endColor, float minBrightness = 0.15f)
    {
        string result = "";
        int len = input.Length;
        
        for (int i = 0; i < len; i++)
        {
            float t = (float)i / Mathf.Max(1, len - 1);
            Color lerped = Color.Lerp(startColor, endColor, t);

            float brightness = 0.299f * lerped.r + 0.587f * lerped.g + 0.114f * lerped.b;
            if (brightness < minBrightness)
            {
                float boost = Mathf.Clamp01((minBrightness - brightness) * 0.5f);
                lerped = Color.Lerp(lerped, Color.white, boost);
            }

            string hex = ColorUtility.ToHtmlStringRGB(lerped);
            result += $"<color=#{hex}>{input[i]}</color>";
        }

        return result;
    }
    public static void ApplyVertexGradient(TextMeshProUGUI tmpText, Color startColor, Color endColor, float time, float waveFrequency = 1.5f)
    {
        if (tmpText == null)
            return;

        float leftWave = Mathf.SmoothStep(0f, 1f, Mathf.Sin((time + 0f) * waveFrequency * Mathf.PI * 2f) * 0.5f + 0.5f);
        float rightWave = Mathf.SmoothStep(0f, 1f, Mathf.Sin((time + 1f) * waveFrequency * Mathf.PI * 2f) * 0.5f + 0.5f);

        Color leftColor = Color.Lerp(startColor, endColor, leftWave);
        Color rightColor = Color.Lerp(startColor, endColor, rightWave);

        VertexGradient vertexGradient = new VertexGradient(leftColor, leftColor, rightColor, rightColor);

        tmpText.colorGradient = vertexGradient;
        tmpText.ForceMeshUpdate();
    }

    public static void ApplyRainbowGradient(TextMeshProUGUI tmpText, float time)
    {
        if (tmpText == null)
            return;

        float speed = 0.5f;
        float hueShift = (time * speed) % 1f;

        Color topLeft = Color.HSVToRGB((hueShift + 0f) % 1f, 1f, 1f);
        Color topRight = Color.HSVToRGB((hueShift + 0.33f) % 1f, 1f, 1f);
        Color bottomLeft = Color.HSVToRGB((hueShift + 0.66f) % 1f, 1f, 1f);
        Color bottomRight = Color.HSVToRGB((hueShift + 0.99f) % 1f, 1f, 1f);

        tmpText.colorGradient = new VertexGradient(topLeft, topRight, bottomLeft, bottomRight);
        tmpText.ForceMeshUpdate();
    }

    public static void PlayerTypingIndicator()
    {
        var indicator = HUDManager.Instance.typingIndicator;
        indicator.enableVertexGradient = true;
        indicator.color = Color.white;
        if (indicator == null)
            return;

        if (HasCustomGradient())
        {
            if (ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorA.Value, out Color startColor) &&
                ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorB.Value, out Color endColor))
            {
                ApplyVertexGradient(indicator, startColor, endColor, Time.time * 0.2f);
                return;
            }
        }

        switch (InventoryFrames.CurrentSlotColorMode)
        {
            case SlotEnums.Rainbow:
                ApplyRainbowGradient(indicator, Time.time * 0.25f);
                break;

            case SlotEnums.None:
                indicator.colorGradient = new VertexGradient(ConfigHelper.GetSlotColor());
                break;

            default:
                ApplyVertexGradient(indicator, InventoryFrames.CurrentGradientStartColor, InventoryFrames.CurrentGradientEndColor, Time.time * 0.25f);
                break;
        }
    }

    public static void ColorChatInputField(TMP_InputField inputField, float time)
    {
        Color targetColor;

        if (HasCustomGradient())
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
