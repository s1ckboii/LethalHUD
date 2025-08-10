using HarmonyLib;
using LethalHUD.Configs;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.HUD.InventoryGradientEnums;

namespace LethalHUD.HUD;

public static class InventoryFrames
{
    private static float _gradientWaveTime = 0f;

    public static SlotEnums CurrentSlotColorMode { get; private set; } = SlotEnums.None;
    public static Color CurrentGradientStartColor { get; private set; } = Color.white;
    public static Color CurrentGradientEndColor { get; private set; } = Color.white;
    public static void SetSlotColors()
    {
        if (HUDManager.Instance == null || HUDManager.Instance.itemSlotIconFrames == null)
            return;

        Image[] frames = HUDManager.Instance.itemSlotIconFrames;

        if (HasCustomGradient())
        {
            if (ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorA.Value, out Color colorA)
             && ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorB.Value, out Color colorB))
            {
                CurrentGradientStartColor =colorA;
                CurrentGradientEndColor =colorB;
                ApplyWavyGradient(frames, colorA, colorB);
                CompassController.ApplyCompassWavyGradient(colorA, colorB);
                return;
            }
        }

        CurrentSlotColorMode = Plugins.ConfigEntries.SlotRainbowColor.Value;
        switch (CurrentSlotColorMode)
        {
            case SlotEnums.Rainbow:
                ApplyRainbow(frames);
                CompassController.ApplyCompassRainbow();
                break;

            case SlotEnums.Summer:
                CurrentGradientStartColor = InventoryUtils.solarFlare;
                CurrentGradientEndColor = InventoryUtils.moltenCore;
                ApplyWavyGradient(frames, CurrentGradientStartColor, CurrentGradientEndColor);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Winter:
                CurrentGradientStartColor = InventoryUtils.skyWave;
                CurrentGradientEndColor = InventoryUtils.moonlitMist;
                ApplyWavyGradient(frames, InventoryUtils.skyWave, InventoryUtils.moonlitMist);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Vaporwave:
                CurrentGradientStartColor = InventoryUtils.pinkPrism;
                CurrentGradientEndColor = InventoryUtils.aquaPulse;
                ApplyWavyGradient(frames, InventoryUtils.pinkPrism, InventoryUtils.aquaPulse);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Deepmint:
                CurrentGradientStartColor = InventoryUtils.mintWave;
                CurrentGradientEndColor = InventoryUtils.deepTeal;
                ApplyWavyGradient(frames, InventoryUtils.mintWave, InventoryUtils.deepTeal);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Radioactive:
                CurrentGradientStartColor = InventoryUtils.neonLime;
                CurrentGradientEndColor = InventoryUtils.lemonGlow;
                ApplyWavyGradient(frames, InventoryUtils.neonLime, InventoryUtils.lemonGlow);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.TideEmber:
                CurrentGradientStartColor = InventoryUtils.crimsonSpark;
                CurrentGradientEndColor = InventoryUtils.deepOcean;
                ApplyWavyGradient(frames, InventoryUtils.crimsonSpark, InventoryUtils.deepOcean);
                CompassController.SetCompassWavyGradient();
                break;

            default:
                Color color = ConfigHelper.GetSlotColor();
                foreach (var frame in frames)
                {
                    if (frame != null)
                        frame.color = color;
                }
                CompassController.SetCompassColor(color);
                break;
        }
    }
    private static void ApplyRainbow(Image[] frames)
    {
        int count = frames.Length;
        float hueShift = Time.time * 0.15f;

        for (int i = 0; i < count; i++)
        {
            float hue = (hueShift + (float)i / count) % 1f;
            Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

            if (frames[i] != null)
                frames[i].color = rainbowColor;
        }
    }

    private static void ApplyWavyGradient(Image[] frames, Color startColor, Color endColor, float speed = 0.15f, float waveFrequency = 2f)
    {
        if (frames == null || frames.Length == 0)
            return;

        _gradientWaveTime = (_gradientWaveTime + Time.deltaTime * speed) % 1f;

        int count = frames.Length;
        for (int i = 0; i < count; i++)
        {
            float normalizedIndex = (float)i / (count - 1);
            float waveOffset = Mathf.SmoothStep(0f, 1f, Mathf.Sin((_gradientWaveTime + normalizedIndex * waveFrequency) * Mathf.PI * 2f) * 0.5f + 0.5f);

            Color interpolated = Color.Lerp(startColor, endColor, waveOffset).gamma;
            frames[i].color = interpolated;
        }
    }

    public static void HandsFull()
    {
        string handsfullColor = Plugins.ConfigEntries.HandsFullColor.Value;
        ColorUtility.TryParseHtmlString(handsfullColor, out Color fullColor);
        HUDManager.Instance.holdingTwoHandedItem.color = fullColor;
    }
}