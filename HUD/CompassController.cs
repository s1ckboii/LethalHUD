using GameNetcodeStuff;
using LethalHUD.Configs;
using SoftMasking;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.HUD;
internal static class CompassController
{
    internal static RawImage CompassImage => HUDManager.Instance?.compassImage;

    internal static void SetCompassColor(Color? overrideColor = null)
    {
        if (CompassImage == null)
            return;

        PlayerControllerB player = StartOfRound.Instance?.localPlayerController;
        if (player == null)
            return;

        Color color = overrideColor ?? ConfigHelper.GetSlotColor();

        color.a = player.isPlayerDead
            ? 0f
            : Plugins.ConfigEntries.CompassAlpha.Value;

        CompassImage.color = color;
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
        if (CompassImage == null)
            return;

        PlayerControllerB player = StartOfRound.Instance?.localPlayerController;
        if (player == null)
            return;

        SoftMask softMask = CompassImage.GetComponentInParent<SoftMask>();
        if (softMask == null)
            return;

        bool invertMaskConfig = Plugins.ConfigEntries.CompassInvertMask.Value;
        bool invertOutsidesConfig = Plugins.ConfigEntries.CompassInvertOutsides.Value;
        float alphaConfig = Plugins.ConfigEntries.CompassAlpha.Value;

        if (player.isPlayerDead)
        {
            softMask.invertMask = false;
            softMask.invertOutsides = false;

            Vector4 weights = softMask.channelWeights;
            weights.w = 0f;
            softMask.channelWeights = weights;
        }
        else
        {
            softMask.invertMask = invertMaskConfig;
            softMask.invertOutsides = invertOutsidesConfig;

            Vector4 weights = softMask.channelWeights;
            weights.w = alphaConfig;
            softMask.channelWeights = weights;
        }
    }
}