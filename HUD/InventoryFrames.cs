using LethalHUD.Compats;
using LethalHUD.Configs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class InventoryFrames
{
    internal static SlotEnums CurrentSlotColorMode { get; private set; } = SlotEnums.None;
    internal static Color CurrentGradientStartColor { get; private set; } = Color.white;
    internal static Color CurrentGradientEndColor { get; private set; } = Color.white;

    private static readonly Dictionary<Image, Image> _fadeIconMap = [];
    private static Image[] _lastFramesReference;
    private static Image[] _allFrames;
    private static Image _chatFrame;
    private static float _nextChatFrameLookupTime;
    private static bool _reportedMissingChatFrame;

    internal static void SetSlotColors()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud?.itemSlotIconFrames == null)
            return;

        Image[] frames = hud.itemSlotIconFrames;
        EnsureFrameCache(frames);

        if (_allFrames == null || _allFrames.Length == 0)
            return;

        if (HUDUtils.HasCustomGradient(Plugins.ConfigEntries.GradientColorA.Value, Plugins.ConfigEntries.GradientColorB.Value))
        {
            if (ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorA.Value, out Color colorA)
             && ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorB.Value, out Color colorB))
            {
                CurrentGradientStartColor = colorA;
                CurrentGradientEndColor = colorB;
                HUDUtils.ApplyWavyGradient(_allFrames, colorA, colorB, _fadeIconMap);
                HUDUtils.ApplyCompassWavyGradient(colorA, colorB);
                return;
            }
        }

        CurrentSlotColorMode = Plugins.ConfigEntries.SlotRainbowColor.Value;
        switch (CurrentSlotColorMode)
        {
            case SlotEnums.Rainbow:
                HUDUtils.ApplyRainbow(_allFrames, _fadeIconMap);
                HUDUtils.ApplyCompassRainbow();
                break;

            case SlotEnums.Summer:
                CurrentGradientStartColor = solarFlare;
                CurrentGradientEndColor = moltenCore;
                HUDUtils.ApplyWavyGradient(_allFrames, CurrentGradientStartColor, CurrentGradientEndColor, _fadeIconMap);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Winter:
                CurrentGradientStartColor = skyWave;
                CurrentGradientEndColor = moonlitMist;
                HUDUtils.ApplyWavyGradient(_allFrames, skyWave, moonlitMist, _fadeIconMap);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Vaporwave:
                CurrentGradientStartColor = pinkPrism;
                CurrentGradientEndColor = aquaPulse;
                HUDUtils.ApplyWavyGradient(_allFrames, pinkPrism, aquaPulse, _fadeIconMap);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Deepmint:
                CurrentGradientStartColor = mintWave;
                CurrentGradientEndColor = deepTeal;
                HUDUtils.ApplyWavyGradient(_allFrames, mintWave, deepTeal, _fadeIconMap);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.Radioactive:
                CurrentGradientStartColor = neonLime;
                CurrentGradientEndColor = lemonGlow;
                HUDUtils.ApplyWavyGradient(_allFrames, neonLime, lemonGlow, _fadeIconMap);
                CompassController.SetCompassWavyGradient();
                break;

            case SlotEnums.TideEmber:
                CurrentGradientStartColor = crimsonSpark;
                CurrentGradientEndColor = deepOcean;
                HUDUtils.ApplyWavyGradient(_allFrames, crimsonSpark, deepOcean, _fadeIconMap);
                CompassController.SetCompassWavyGradient();
                break;

            default:
                Color color = ConfigHelper.GetSlotColor();
                for (int i = 0; i < _allFrames.Length; i++)
                {
                    Image frame = _allFrames[i];
                    if (frame == null)
                        continue;

                    Color current = frame.color;
                    Color target = new(color.r, color.g, color.b, current.a);
                    if (current != target)
                        frame.color = target;

                    if (_fadeIconMap.TryGetValue(frame, out Image fadeImg) && fadeImg != null)
                    {
                        Color fadeCurrent = fadeImg.color;
                        Color fadeTarget = new(color.r, color.g, color.b, fadeCurrent.a);
                        if (fadeCurrent != fadeTarget)
                            fadeImg.color = fadeTarget;
                    }
                }

                CompassController.SetCompassColor(color);
                break;
        }
    }

    private static void EnsureFrameCache(Image[] frames)
    {
        Image previousChatFrame = _chatFrame;

        if (_chatFrame == null && Time.unscaledTime >= _nextChatFrameLookupTime)
        {
            _nextChatFrameLookupTime = Time.unscaledTime + 1f;
            _chatFrame = FindChatFrame();
        }

        bool framesChanged = !ReferenceEquals(_lastFramesReference, frames);
        bool chatFrameChanged = !ReferenceEquals(previousChatFrame, _chatFrame);

        if (!framesChanged && !chatFrameChanged && _allFrames != null)
            return;

        _lastFramesReference = frames;

        if (_chatFrame == null)
        {
            _allFrames = frames;
        }
        else
        {
            Image[] combined = new Image[frames.Length + 1];
            for (int i = 0; i < frames.Length; i++)
                combined[i] = frames[i];

            combined[frames.Length] = _chatFrame;
            _allFrames = combined;
        }

        RebuildFadeCache(_allFrames);
    }

    private static Image FindChatFrame()
    {
        string path = ModCompats.IsNiceChatPresent
            ? "Systems/UI/Canvas/IngamePlayerHUD/BottomLeftCorner/taffyko.NiceChat.ChatContainer"
            : "Systems/UI/Canvas/IngamePlayerHUD/BottomLeftCorner";

        GameObject bottomLeftCorner = GameObject.Find(path);
        if (bottomLeftCorner == null)
        {
            ReportMissingChatFrameOnce("BottomLeftCorner not found; inventory frame cache will retry.");
            return null;
        }

        Transform imageTransform = bottomLeftCorner.transform.Find("Image");
        if (imageTransform == null)
        {
            ReportMissingChatFrameOnce("Image transform not found under BottomLeftCorner; inventory frame cache will retry.");
            return null;
        }

        Image chatFrame = imageTransform.GetComponent<Image>();
        if (chatFrame == null)
        {
            ReportMissingChatFrameOnce("Image component not found; inventory frame cache will retry.");
            return null;
        }

        _reportedMissingChatFrame = false;
        return chatFrame;
    }

    private static void ReportMissingChatFrameOnce(string message)
    {
        if (_reportedMissingChatFrame)
            return;

        _reportedMissingChatFrame = true;
        Loggers.Warning($"InventoryFrames: {message}");
    }

    private static void RebuildFadeCache(Image[] frames)
    {
        _fadeIconMap.Clear();

        for (int i = 0; i < frames.Length; i++)
        {
            Image frame = frames[i];
            if (frame == null)
                continue;

            Transform fade = frame.transform.Find("fadeIcon");
            if (fade != null && fade.TryGetComponent(out Image fadeImg))
                _fadeIconMap[frame] = fadeImg;
        }
    }

    internal static void ResetCache()
    {
        _fadeIconMap.Clear();
        _lastFramesReference = null;
        _allFrames = null;
        _chatFrame = null;
        _nextChatFrameLookupTime = 0f;
        _reportedMissingChatFrame = false;
    }

    internal static void HandsFull()
    {
        string handsfullColor = Plugins.ConfigEntries.HandsFullColor.Value;
        ColorUtility.TryParseHtmlString(handsfullColor, out Color fullColor);
        HUDManager.Instance.holdingTwoHandedItem.color = fullColor;
    }
}