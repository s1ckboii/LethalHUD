using LethalHUD;
using LethalHUD.HUD;
using System.Text;
using UnityEngine;
using static LethalHUD.HUD.InventoryGradientEnums;

public static class ChatController
{
    private static float _gradientWaveTime = 0f;

    public static float GradientWaveTime
    {
        get => _gradientWaveTime;
        set => _gradientWaveTime = value;
    }

    public static string GetColoredPlayerName(string playerName)
    {
        if (!Plugins.ConfigEntries.ColoredNames.Value || string.IsNullOrEmpty(playerName))
            return playerName;

        SlotEnums mode = Plugins.ConfigEntries.ChatNameColorMode.Value;

        if (mode == SlotEnums.None)
        {
            string hexColor = Plugins.ConfigEntries.LocalNameColor.Value;
            return $"<color={hexColor}>{playerName}</color>";
        }

        if (mode == SlotEnums.Rainbow)
        {
            return ApplyRainbowToText(playerName);
        }

        if (TryGetGradientColors(out Color startColor, out Color endColor))
        {
            return ApplyWavyGradientToText(playerName, startColor, endColor);
        }

        string fallbackColor = Plugins.ConfigEntries.LocalNameColor.Value;
        return $"<color={fallbackColor}>{playerName}</color>";
    }

    public static bool TryGetGradientColors(out Color startColor, out Color endColor)
    {
        startColor = Color.white;
        endColor = Color.white;

        switch (Plugins.ConfigEntries.ChatNameColorMode.Value)
        {
            case SlotEnums.Summer:
                startColor = InventoryUtils.solarFlare;
                endColor = InventoryUtils.moltenCore;
                return true;
            case SlotEnums.Winter:
                startColor = InventoryUtils.skyWave;
                endColor = InventoryUtils.moonlitMist;
                return true;
            case SlotEnums.Vaporwave:
                startColor = InventoryUtils.pinkPrism;
                endColor = InventoryUtils.aquaPulse;
                return true;
            case SlotEnums.Deepmint:
                startColor = InventoryUtils.mintWave;
                endColor = InventoryUtils.deepTeal;
                return true;
            case SlotEnums.Radioactive:
                startColor = InventoryUtils.neonLime;
                endColor = InventoryUtils.lemonGlow;
                return true;
            case SlotEnums.TideEmber:
                startColor = InventoryUtils.crimsonSpark;
                endColor = InventoryUtils.deepOcean;
                return true;
            case SlotEnums.None:
                return false;
            case SlotEnums.Rainbow:
                return false;
            default:
                return false;
        }
    }

    public static string ApplyRainbowToText(string text)
    {
        StringBuilder sb = new();
        float hueShift = Time.time * 0.15f;
        int length = text.Length;

        for (int i = 0; i < length; i++)
        {
            float hue = (hueShift + (float)i / length) % 1f;
            Color color = Color.HSVToRGB(hue, 1f, 1f);
            string hex = ColorUtility.ToHtmlStringRGB(color);
            sb.Append($"<color=#{hex}>{text[i]}</color>");
        }

        return sb.ToString();
    }

    public static string ApplyWavyGradientToText(string text, Color startColor, Color endColor, float speed = 0.15f, float waveFrequency = 2f)
    {
        StringBuilder sb = new();
        _gradientWaveTime = (_gradientWaveTime + Time.deltaTime * speed) % 1f;

        int length = text.Length;
        for (int i = 0; i < length; i++)
        {
            float normalizedIndex = length > 1 ? (float)i / (length - 1) : 0f;
            float waveOffset = Mathf.SmoothStep(0f, 1f, Mathf.Sin((_gradientWaveTime + normalizedIndex * waveFrequency) * Mathf.PI * 2f) * 0.5f + 0.5f);
            Color color = Color.Lerp(startColor, endColor, waveOffset);
            string hex = ColorUtility.ToHtmlStringRGB(color);
            sb.Append($"<color=#{hex}>{text[i]}</color>");
        }

        return sb.ToString();
    }
}
