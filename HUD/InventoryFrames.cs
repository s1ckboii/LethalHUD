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

    private static Image[] _allFrames;
    internal static void SetSlotColors()
    {
        if (HUDManager.Instance == null || HUDManager.Instance.itemSlotIconFrames == null)
            return;
        
        Image[] frames = HUDManager.Instance.itemSlotIconFrames;
        GameObject bottomLeftCorner;
        if (ModCompats.IsNiceChatPresent)
            bottomLeftCorner = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/BottomLeftCorner/taffyko.NiceChat.ChatContainer");
        else
            bottomLeftCorner = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/BottomLeftCorner");

        if (bottomLeftCorner == null)
        {
            Loggers.Warning("InventoryFrames: BottomLeftCorner not found.");
            _allFrames = frames;
            return;
        }

        Transform imageTransform = bottomLeftCorner.transform.Find("Image");
        if (imageTransform == null)
        {
            Loggers.Warning("InventoryFrames: Image transform not found under BottomLeftCorner.");
            _allFrames = frames;
            return;
        }

        Image chatFrame = imageTransform.GetComponent<Image>();
        if (chatFrame == null)
        {
            Loggers.Warning("InventoryFrames: Image component not found.");
            _allFrames = frames;
            return;
        }

        Image[] combined = new Image[frames.Length + 1];
        for (int i = 0; i < frames.Length; i++)
            combined[i] = frames[i];
        combined[frames.Length] = chatFrame;
        _allFrames = combined;


        if (HUDUtils.HasCustomGradient(Plugins.ConfigEntries.GradientColorA.Value, Plugins.ConfigEntries.GradientColorB.Value))
        {
            if (ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorA.Value, out Color colorA)
             && ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorB.Value, out Color colorB))
            {
                CurrentGradientStartColor =colorA;
                CurrentGradientEndColor =colorB;
                HUDUtils.ApplyWavyGradient(_allFrames, colorA, colorB);
                HUDUtils.ApplyCompassWavyGradient(colorA, colorB);
                return;
            }
        }

        CurrentSlotColorMode = Plugins.ConfigEntries.SlotRainbowColor.Value;
        switch (CurrentSlotColorMode)
        {
            case SlotEnums.Rainbow:
                HUDUtils.ApplyRainbow(_allFrames);
                HUDUtils.ApplyCompassRainbow();
                break;

            case SlotEnums.Summer:
                CurrentGradientStartColor = solarFlare;
                CurrentGradientEndColor = moltenCore;
                HUDUtils.ApplyWavyGradient(_allFrames, CurrentGradientStartColor, CurrentGradientEndColor);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Winter:
                CurrentGradientStartColor = skyWave;
                CurrentGradientEndColor = moonlitMist;
                HUDUtils.ApplyWavyGradient(_allFrames, skyWave, moonlitMist);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Vaporwave:
                CurrentGradientStartColor = pinkPrism;
                CurrentGradientEndColor = aquaPulse;
                HUDUtils.ApplyWavyGradient(_allFrames, pinkPrism, aquaPulse);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Deepmint:
                CurrentGradientStartColor = mintWave;
                CurrentGradientEndColor = deepTeal;
                HUDUtils.ApplyWavyGradient(_allFrames, mintWave, deepTeal);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Radioactive:
                CurrentGradientStartColor = neonLime;
                CurrentGradientEndColor = lemonGlow;
                HUDUtils.ApplyWavyGradient(_allFrames, neonLime, lemonGlow);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.TideEmber:
                CurrentGradientStartColor = crimsonSpark;
                CurrentGradientEndColor = deepOcean;
                HUDUtils.ApplyWavyGradient(_allFrames, crimsonSpark, deepOcean);
                CompassController.SetCompassWavyGradient();
                break;

            default:
                Color color = ConfigHelper.GetSlotColor();
                foreach (var frame in _allFrames)
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