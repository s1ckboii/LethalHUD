using HarmonyLib;
using LethalHUD.Configs;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.HUD.InventoryGradientEnums;

namespace LethalHUD.HUD;

public static class InventoryFrames
{
    public static SlotEnums CurrentSlotColorMode { get; private set; } = SlotEnums.None;
    public static Color CurrentGradientStartColor { get; private set; } = Color.white;
    public static Color CurrentGradientEndColor { get; private set; } = Color.white;
    public static void SetSlotColors()
    {
        if (HUDManager.Instance == null || HUDManager.Instance.itemSlotIconFrames == null)
            return;

        Image[] frames = HUDManager.Instance.itemSlotIconFrames;
            GameObject bottomLeftCorner = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/BottomLeftCorner");
            Transform imageTransform = bottomLeftCorner.transform.Find("Image");
            Image chatFrame = imageTransform.GetComponent<Image>();
            if (!frames.Contains(chatFrame))
            {
                HUDManager.Instance.itemSlotIconFrames = frames.Concat(new[] { chatFrame }).ToArray();
            }


        if (HUDUtils.HasCustomGradient(Plugins.ConfigEntries.GradientColorA.Value, Plugins.ConfigEntries.GradientColorB.Value))
        {
            if (ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorA.Value, out Color colorA)
             && ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorB.Value, out Color colorB))
            {
                CurrentGradientStartColor =colorA;
                CurrentGradientEndColor =colorB;
                HUDUtils.ApplyWavyGradient(frames, colorA, colorB);
                HUDUtils.ApplyCompassWavyGradient(colorA, colorB);
                return;
            }
        }

        CurrentSlotColorMode = Plugins.ConfigEntries.SlotRainbowColor.Value;
        switch (CurrentSlotColorMode)
        {
            case SlotEnums.Rainbow:
                HUDUtils.ApplyRainbow(frames);
                HUDUtils.ApplyCompassRainbow();
                break;

            case SlotEnums.Summer:
                CurrentGradientStartColor = solarFlare;
                CurrentGradientEndColor = moltenCore;
                HUDUtils.ApplyWavyGradient(frames, CurrentGradientStartColor, CurrentGradientEndColor);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Winter:
                CurrentGradientStartColor = skyWave;
                CurrentGradientEndColor = moonlitMist;
                HUDUtils.ApplyWavyGradient(frames, skyWave, moonlitMist);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Vaporwave:
                CurrentGradientStartColor = pinkPrism;
                CurrentGradientEndColor = aquaPulse;
                HUDUtils.ApplyWavyGradient(frames, pinkPrism, aquaPulse);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Deepmint:
                CurrentGradientStartColor = mintWave;
                CurrentGradientEndColor = deepTeal;
                HUDUtils.ApplyWavyGradient(frames, mintWave, deepTeal);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Radioactive:
                CurrentGradientStartColor = neonLime;
                CurrentGradientEndColor = lemonGlow;
                HUDUtils.ApplyWavyGradient(frames, neonLime, lemonGlow);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.TideEmber:
                CurrentGradientStartColor = crimsonSpark;
                CurrentGradientEndColor = deepOcean;
                HUDUtils.ApplyWavyGradient(frames, crimsonSpark, deepOcean);
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
    public static void HandsFull()
    {
        string handsfullColor = Plugins.ConfigEntries.HandsFullColor.Value;
        ColorUtility.TryParseHtmlString(handsfullColor, out Color fullColor);
        HUDManager.Instance.holdingTwoHandedItem.color = fullColor;
    }
}