using LethalHUD.Configs;
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
}