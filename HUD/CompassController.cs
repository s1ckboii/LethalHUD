using LethalHUD.Configs;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.HUD.InventoryGradientEnums;

namespace LethalHUD.HUD;

public static class CompassController
{
    private static RawImage CompassImage => HUDManager.Instance?.compassImage;

    private static float _rainbowTime = 0f;
    private static float _gradientWaveTime = 0f;

    public static void SetCompassColor(Color? overrideColor = null)
    {
        if (CompassImage == null)
            return;

        Color color = overrideColor ?? ConfigHelper.GetSlotColor();

        color.a = CompassImage.color.a;

        CompassImage.color = color;
    }

    public static void ApplyCompassRainbow()
    {
        if (CompassImage == null) return;

        _rainbowTime += Time.deltaTime * 0.15f;
        float hue = _rainbowTime % 1f;
        Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

        rainbowColor.a = CompassImage.color.a;
        CompassImage.color = rainbowColor;
    }
    public static void ApplyCompassWavyGradient(Color startColor, Color endColor)
    {
        if (CompassImage == null) return;

        _gradientWaveTime += Time.deltaTime * 0.15f;
        float waveOffset = Mathf.SmoothStep(0f, 1f, Mathf.Sin(_gradientWaveTime * Mathf.PI * 2f) * 0.5f + 0.5f); // *magic numbers*

        Color interpolated = Color.Lerp(startColor, endColor, waveOffset).gamma;
        interpolated.a = CompassImage.color.a;
        CompassImage.color = interpolated;
    }

    public static void SetCompassWavyGradient()
    {
            ApplyCompassWavyGradient(InventoryFrames.CurrentGradientStartColor, InventoryFrames.CurrentGradientEndColor);
    }
}