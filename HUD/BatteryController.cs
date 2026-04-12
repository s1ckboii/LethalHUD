using GameNetcodeStuff;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LethalHUD.HUD;
internal static class BatteryController
{
    private static Image[] _batteryOffImages;
    private static Transform _batteryOffRoot;

    internal static void Update(PlayerControllerB player)
    {
        if (CustomHUD.CustomBattery.UsingCustom)
        {
            ForceHideVanilla();

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

    private static void FindBatteryOff()
    {
        if (_batteryOffRoot != null) return;

        GameObject tpc = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/");
        if (tpc == null) return;

        Transform off = tpc.transform.Find("BatteriesOff");
        if (off == null) return;

        _batteryOffRoot = off;

        var list = new List<Image>();

        var icon = off.Find("BatteryIcon")?.GetComponent<Image>();
        if (icon != null) list.Add(icon);

        var meter = off.Find("BatteryMeter");
        if (meter != null)
        {
            var frame = meter.Find("BatteryMeterFrame")?.GetComponent<Image>();
            if (frame != null) list.Add(frame);
        }

        _batteryOffImages = list.ToArray();
    }

    internal static void UpdateVanillaColor()
    {
        var hud = HUDManager.Instance;
        if (hud == null) return;

        FindBatteryOff();

        Color baseCol = HUDUtils.ParseHexColor(
            Plugins.ConfigEntries.BatteryColor.Value,
            Color.yellow
        );

        Color offCol = Darken(baseCol, 0.6f);

        if (hud.batteryMeter != null)
            hud.batteryMeter.color = baseCol;

        if (hud.batteryIcon != null)
            hud.batteryIcon.color = baseCol;

        if (hud.batteryBlinkUI != null)
        {
            bool visible = hud.batteryMeter != null && hud.batteryMeter.gameObject.activeSelf;

            hud.batteryBlinkUI.gameObject.SetActive(visible);

            foreach (var img in hud.batteryBlinkUI.GetComponentsInChildren<Image>(true))
                img.color = baseCol;
        }

        if (_batteryOffImages != null)
        {
            foreach (var img in _batteryOffImages)
                img.color = offCol;
        }
    }

    private static void ForceHideVanilla()
    {
        var hud = HUDManager.Instance;
        if (hud == null) return;

        if (hud.batteryMeter != null)
            hud.batteryMeter.gameObject.SetActive(false);

        if (hud.batteryIcon != null)
            hud.batteryIcon.enabled = false;

        if (hud.batteryBlinkUI != null)
            hud.batteryBlinkUI.gameObject.SetActive(false);

        FindBatteryOff();

        if (_batteryOffRoot != null)
            _batteryOffRoot.gameObject.SetActive(false);
    }

    private static Color Darken(Color c, float multiplier)
    {
        return new Color(
            c.r * multiplier,
            c.g * multiplier,
            c.b * multiplier,
            c.a
        );
    }
}