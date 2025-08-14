using LethalHUD.Compats;
using LethalHUD.Configs;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;

internal static class InventoryFrames
{
    internal static SlotEnums CurrentSlotColorMode { get; private set; } = SlotEnums.None;
    internal static Color CurrentGradientStartColor { get; private set; } = Color.white;
    internal static Color CurrentGradientEndColor { get; private set; } = Color.white;

    internal static Image[] allFrames;
    internal static void SetSlotColors()
    {
        if (HUDManager.Instance == null || HUDManager.Instance.itemSlotIconFrames == null)
            return;
        
        Image[] frames = HUDManager.Instance.itemSlotIconFrames;
        if (ModCompats.IsNiceChatPresent)
        {
            GameObject bottomLeftCorner = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/BottomLeftCorner/taffyko.NiceChat.ChatContainer");
            Transform imageTransform = bottomLeftCorner.transform.Find("Image");
            Image chatFrame = imageTransform.GetComponent<Image>();

            Image[] combined = new Image[frames.Length + 1];

            for (int i = 0; i < frames.Length; i++)
                combined[i] = frames[i];

            combined[frames.Length] = chatFrame;

            allFrames = combined;
        }
        else
        {
            GameObject bottomLeftCorner = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/BottomLeftCorner");
            Transform imageTransform = bottomLeftCorner.transform.Find("Image");
            Image chatFrame = imageTransform.GetComponent<Image>();

            Image[] combined = new Image[frames.Length + 1];

            for (int i = 0; i < frames.Length; i++)
                combined[i] = frames[i];

            combined[frames.Length] = chatFrame;

            allFrames = combined;
        }



        if (HUDUtils.HasCustomGradient(Plugins.ConfigEntries.GradientColorA.Value, Plugins.ConfigEntries.GradientColorB.Value))
        {
            if (ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorA.Value, out Color colorA)
             && ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorB.Value, out Color colorB))
            {
                CurrentGradientStartColor =colorA;
                CurrentGradientEndColor =colorB;
                HUDUtils.ApplyWavyGradient(allFrames, colorA, colorB);
                HUDUtils.ApplyCompassWavyGradient(colorA, colorB);
                return;
            }
        }

        CurrentSlotColorMode = Plugins.ConfigEntries.SlotRainbowColor.Value;
        switch (CurrentSlotColorMode)
        {
            case SlotEnums.Rainbow:
                HUDUtils.ApplyRainbow(allFrames);
                HUDUtils.ApplyCompassRainbow();
                break;

            case SlotEnums.Summer:
                CurrentGradientStartColor = solarFlare;
                CurrentGradientEndColor = moltenCore;
                HUDUtils.ApplyWavyGradient(allFrames, CurrentGradientStartColor, CurrentGradientEndColor);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Winter:
                CurrentGradientStartColor = skyWave;
                CurrentGradientEndColor = moonlitMist;
                HUDUtils.ApplyWavyGradient(allFrames, skyWave, moonlitMist);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Vaporwave:
                CurrentGradientStartColor = pinkPrism;
                CurrentGradientEndColor = aquaPulse;
                HUDUtils.ApplyWavyGradient(allFrames, pinkPrism, aquaPulse);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Deepmint:
                CurrentGradientStartColor = mintWave;
                CurrentGradientEndColor = deepTeal;
                HUDUtils.ApplyWavyGradient(allFrames, mintWave, deepTeal);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Radioactive:
                CurrentGradientStartColor = neonLime;
                CurrentGradientEndColor = lemonGlow;
                HUDUtils.ApplyWavyGradient(allFrames, neonLime, lemonGlow);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.TideEmber:
                CurrentGradientStartColor = crimsonSpark;
                CurrentGradientEndColor = deepOcean;
                HUDUtils.ApplyWavyGradient(allFrames, crimsonSpark, deepOcean);
                CompassController.SetCompassWavyGradient();
                break;

            default:
                Color color = ConfigHelper.GetSlotColor();
                foreach (var frame in allFrames)
                {
                    if (frame != null)
                        frame.color = color;
                }
                CompassController.SetCompassColor(color);
                break;
        }
    }
    internal static void HandsFull()
    {
        string handsfullColor = Plugins.ConfigEntries.HandsFullColor.Value;
        ColorUtility.TryParseHtmlString(handsfullColor, out Color fullColor);
        HUDManager.Instance.holdingTwoHandedItem.color = fullColor;
    }
}