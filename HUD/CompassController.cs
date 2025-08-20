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

        Color color = overrideColor ?? ConfigHelper.GetSlotColor();

        color.a = CompassImage.color.a;

        CompassImage.color = color;
    }
    internal static void SetCompassWavyGradient()
    {
        HUDUtils.ApplyCompassWavyGradient(InventoryFrames.CurrentGradientStartColor, InventoryFrames.CurrentGradientEndColor);
    }
    internal static void SoftMaskStuff()
    {
        if (CompassImage == null)
            return;

        SoftMask softMask = CompassImage.GetComponentInParent<SoftMask>();
        if (softMask == null)
            return;

        bool invertMaskConfig = Plugins.ConfigEntries.CompassInvertMask.Value;
        bool invertOutsidesConfig = Plugins.ConfigEntries.CompassInvertOutsides.Value;
        float alphaConfig = Plugins.ConfigEntries.CompassAlpha.Value;

        softMask.invertMask = invertMaskConfig;
        softMask.invertOutsides = invertOutsidesConfig;

        Vector4 weights = softMask.channelWeights;
        weights.w = alphaConfig;

        softMask.channelWeights = weights;
    }
}