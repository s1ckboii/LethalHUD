using GameNetcodeStuff;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class ClockController
{
    private static bool skipAlphaCheck;
    private static readonly HashSet<SelectableLevel> DisabledClockLevels = [];

    private static TextMeshProUGUI clockNumber;
    private static Image clockIcon;
    private static Transform clockParent;
    private static Image boxImage;

    // ClockStyle.Regular and Compact to add -> Regular is default, Compact is similar to betterclock where AM/PM is in the same line and the rectangle and image is smaller

    internal static void ApplyInitialClockAppearance()
    {
        if (HUDManager.Instance == null) return;

        ApplyClockAppearance();
    }

    internal static void ApplyClockAppearance()
    {
        if (HUDManager.Instance == null) return;

        clockNumber = HUDManager.Instance.clockNumber;
        clockIcon = HUDManager.Instance.clockIcon;
        clockParent = clockNumber.transform.parent;
        boxImage = clockParent.GetComponent<Image>();

        clockNumber.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ClockNumberColor.Value, Color.white);
        if (boxImage != null)
            boxImage.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ClockBoxColor.Value, Color.white);
        if (clockIcon != null)
            clockIcon.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ClockIconColor.Value, new Color(1f, 0.31f, 0f));
        if (HUDManager.Instance.shipLeavingEarlyIcon != null)
            HUDManager.Instance.shipLeavingEarlyIcon.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ClockShipLeaveColor.Value, Color.white);

        Vector3 baseScale = new(-0.5893304f, 0.5893304f, 0.5893303f);
        clockParent.localScale = baseScale * Plugins.ConfigEntries.ClockSizeMultiplier.Value;
    }
    internal static string TryOverrideClock(HUDManager hud, float timeNormalized, float numberOfHours, bool createNewLine)
    {
        int totalMinutes = (int)(timeNormalized * (60f * numberOfHours)) + 360;
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        string formatted;

        if (Plugins.ConfigEntries.NormalHumanBeingClock.Value)
        {
            formatted = $"{hours % 24:00}:{minutes:00}";
        }
        else
        {
            string newLine = createNewLine ? "\n" : " ";
            string ampm = hours >= 12 ? "PM" : "AM";
            int displayHour = hours % 12;
            if (displayHour == 0) displayHour = 12;
            formatted = $"{displayHour:00}:{minutes:00}{newLine}{ampm}";
        }

        hud.clockNumber.text = formatted;

        return formatted;
    }

    internal static void UpdateClockVisibility(ref bool visible)
    {
        SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;
        if (DisabledClockLevels.Contains(currentLevel))
        {
            visible = false;
            return;
        }

        PlayerControllerB localPlayer = StartOfRound.Instance.localPlayerController;
        if (localPlayer == null)
        {
            return;
        }

        if (localPlayer.inTerminalMenu)
        {
            visible = false;
            return;
        }

        if (visible)
        {
            skipAlphaCheck = true;
            return;
        }

        if (!Plugins.ConfigEntries.ShowClockInShip.Value && localPlayer.isInHangarShipRoom)
        {
            visible = false;
            return;
        }

        if (!Plugins.ConfigEntries.ShowClockInFacility.Value && localPlayer.isInsideFactory)
        {
            visible = false;
            return;
        }

        visible = true;
    }
    internal static void ApplyClockAlpha()
    {
        if (HUDManager.Instance == null) return;

        CanvasGroup canvasGroup = clockParent.GetComponent<CanvasGroup>() ?? clockParent.gameObject.AddComponent<CanvasGroup>();
        float targetAlpha = GetTargetAlpha();
        canvasGroup.alpha = targetAlpha;
    }

    private static float GetTargetAlpha()
    {
        if (skipAlphaCheck)
        {
            skipAlphaCheck = false;
            return 1f;
        }

        PlayerControllerB player = StartOfRound.Instance.localPlayerController;
        if (player == null) return 1f;

        if (player.isInsideFactory)
            return Plugins.ConfigEntries.ClockVisibilityInFacility.Value;

        if (player.isInHangarShipRoom)
            return Plugins.ConfigEntries.ClockVisibilityInShip.Value;

        return 1f;
    }
    internal static void ApplyRealtimeClock()
    {
        if (!Plugins.ConfigEntries.RealtimeClock.Value) return;

        if (TimeOfDay.Instance != null)
            TimeOfDay.Instance.changeHUDTimeInterval = 4f;
    }
}