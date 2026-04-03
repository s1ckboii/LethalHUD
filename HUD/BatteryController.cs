using GameNetcodeStuff;
using UnityEngine;
using TMPro;

namespace LethalHUD.HUD;
internal static class BatteryController
{
    internal static void Update(PlayerControllerB player)
    {
        if (CustomHUD.CustomBattery.UsingCustom)
        {
            float charge = 1f;
            bool active = false;

            if (player.isHoldingObject &&
                player.currentlyHeldObjectServer != null &&
                player.currentlyHeldObjectServer.itemProperties.requiresBattery)
            {
                charge = player.currentlyHeldObjectServer.insertedBattery.charge / 1.3f;
                active = true;
            }
            else if (player.helmetLight.enabled)
            {
                charge = player.pocketedFlashlight.insertedBattery.charge / 1.3f;
                active = true;
            }

            CustomHUD.CustomBattery.Update(charge, active);
            return;
        }

        UpdateVanillaColor();
    }

    internal static void UpdateVanillaColor()
    {
        var hud = HUDManager.Instance;
        if (hud == null) return;

        Color col = HUDUtils.ParseHexColor(Plugins.ConfigEntries.BatteryColor.Value, Color.yellow);

        if (hud.batteryMeter != null)
            hud.batteryMeter.color = col;

        if (hud.batteryIcon != null)
            hud.batteryIcon.color = col;

        if (hud.batteryBlinkUI != null)
        {
            bool visible = hud.batteryMeter != null && hud.batteryMeter.gameObject.activeSelf;

            hud.batteryBlinkUI.gameObject.SetActive(visible);

            foreach (var img in hud.batteryBlinkUI.GetComponentsInChildren<UnityEngine.UI.Image>(true))
            {
                img.color = col;
            }
        }
    }
}