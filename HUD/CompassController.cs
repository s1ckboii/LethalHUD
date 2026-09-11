using GameNetcodeStuff;
using LethalHUD.Configs;
using SoftMasking;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.HUD;
internal static class CompassController
{
    private static RawImage _cachedCompassImage;
    private static SoftMask _cachedSoftMask;

    internal static RawImage CompassImage => HUDManager.Instance?.compassImage;

    internal static void SetCompassColor(Color? overrideColor = null)
    {
        RawImage compassImage = CompassImage;
        if (compassImage == null)
            return;

        PlayerControllerB player = StartOfRound.Instance?.localPlayerController;
        if (player == null)
            return;

        Color color = overrideColor ?? ConfigHelper.GetSlotColor();

        color.a = player.isPlayerDead
            ? 0f
            : Plugins.ConfigEntries.CompassAlpha.Value;

        if (compassImage.color != color)
            compassImage.color = color;
    }

    internal static void SetCompassWavyGradient()
    {
        PlayerControllerB player = StartOfRound.Instance?.localPlayerController;
        if (player == null || player.isPlayerDead)
            return;

        HUDUtils.ApplyCompassWavyGradient(InventoryFrames.CurrentGradientStartColor, InventoryFrames.CurrentGradientEndColor);
    }

    internal static void SoftMaskStuff()
    {
        RawImage compassImage = CompassImage;
        if (compassImage == null)
            return;

        PlayerControllerB player = StartOfRound.Instance?.localPlayerController;
        if (player == null)
            return;

        if (_cachedCompassImage != compassImage || _cachedSoftMask == null)
        {
            _cachedCompassImage = compassImage;
            _cachedSoftMask = compassImage.GetComponentInParent<SoftMask>();
        }

        SoftMask softMask = _cachedSoftMask;
        if (softMask == null)
            return;

        bool invertMask;
        bool invertOutsides;
        float alpha;

        if (player.isPlayerDead)
        {
            invertMask = false;
            invertOutsides = false;
            alpha = 0f;
        }
        else
        {
            invertMask = Plugins.ConfigEntries.CompassInvertMask.Value;
            invertOutsides = Plugins.ConfigEntries.CompassInvertOutsides.Value;
            alpha = Plugins.ConfigEntries.CompassAlpha.Value;
        }

        if (softMask.invertMask != invertMask)
            softMask.invertMask = invertMask;

        if (softMask.invertOutsides != invertOutsides)
            softMask.invertOutsides = invertOutsides;

        Vector4 weights = softMask.channelWeights;
        if (!Mathf.Approximately(weights.w, alpha))
        {
            weights.w = alpha;
            softMask.channelWeights = weights;
        }
    }

    internal static void ResetCache()
    {
        _cachedCompassImage = null;
        _cachedSoftMask = null;
    }
}