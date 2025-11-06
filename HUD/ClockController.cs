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

    private static bool _defaultsCached;

    private static Vector3 defaultClockPos;
    private static Vector3 defaultIconPos;
    private static Vector2 defaultParentSize;
    private static Vector2 defaultIconSize;
    private static bool defaultWordWrap;

    internal static void CacheDefaultLayout()
    {
        if (clockParent == null || clockNumber == null || clockIcon == null)
            return;

        RectTransform parentRect = clockParent.GetComponent<RectTransform>();
        RectTransform iconRect = clockIcon.GetComponent<RectTransform>();

        defaultParentSize = parentRect.sizeDelta;
        defaultIconSize = iconRect.sizeDelta;
        defaultClockPos = clockNumber.transform.localPosition;
        defaultIconPos = clockIcon.transform.localPosition;
        defaultWordWrap = clockNumber.enableWordWrapping;
    }

    internal static void ApplyClockAppearance()
    {
        if (HUDManager.Instance == null) return;

        clockNumber = HUDManager.Instance.clockNumber;
        clockIcon = HUDManager.Instance.clockIcon;
        clockParent = clockNumber.transform.parent;
        boxImage = clockParent.GetComponent<Image>();

        if (clockNumber != null )
            clockNumber.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ClockNumberColor.Value, Color.white);
        if (boxImage != null)
            boxImage.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ClockBoxColor.Value, Color.white);
        if (clockIcon != null)
            clockIcon.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ClockIconColor.Value, new Color(1f, 0.31f, 0f));
        if (HUDManager.Instance.shipLeavingEarlyIcon != null)
            HUDManager.Instance.shipLeavingEarlyIcon.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ClockShipLeaveColor.Value, Color.white);

        Vector3 baseScale = new(-0.5893304f, 0.5893304f, 0.5893303f);

        clockParent.localScale = baseScale * Plugins.ConfigEntries.ClockSizeMultiplier.Value;

        if (!_defaultsCached)
        {
            CacheDefaultLayout();
            _defaultsCached = true;
        }

        if (Plugins.ConfigEntries.ClockFormat.Value == ClockStyle.Compact)
            ApplyCompactLayout();
        else
            ApplyRegularLayout();
    }
    internal static void TryOverrideClock(HUDManager hud, float timeNormalized, float numberOfHours)
    {
        if (hud == null || hud.clockNumber == null)
            return;

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
            int displayHour = hours % 12;
            if (displayHour == 0)
                displayHour = 12;

            bool compact = Plugins.ConfigEntries.ClockFormat.Value == ClockStyle.Compact;
            string separator = compact ? " " : "\n";
            string ampm = hours >= 12 ? "PM" : "AM";

            formatted = $"{displayHour:00}:{minutes:00}{separator}{ampm}";
        }

        hud.clockNumber.text = formatted;
    }
    internal static void ApplyCompactLayout()
    {
        if (clockParent == null || clockNumber == null || clockIcon == null)
            return;

        RectTransform parentRect = clockParent.GetComponent<RectTransform>();
        parentRect.sizeDelta = new Vector2(parentRect.sizeDelta.x, 50f);

        clockNumber.enableWordWrapping = false;

        RectTransform iconRect = clockIcon.GetComponent<RectTransform>();
        iconRect.sizeDelta = defaultIconSize * 0.6f;
    }

    internal static void ApplyRegularLayout()
    {
        if (clockParent == null || clockNumber == null || clockIcon == null)
            return;

        RectTransform parentRect = clockParent.GetComponent<RectTransform>();
        RectTransform iconRect = clockIcon.GetComponent<RectTransform>();

        parentRect.sizeDelta = defaultParentSize;
        iconRect.sizeDelta = defaultIconSize;
        clockNumber.transform.localPosition = defaultClockPos;
        clockIcon.transform.localPosition = defaultIconPos;
        clockNumber.enableWordWrapping = defaultWordWrap;
    }

    internal static void UpdateClockVisibility(ref bool visible)
    {
        if (visible)
        {
            skipAlphaCheck = true;
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

        SelectableLevel currentLevel = StartOfRound.Instance.currentLevel;
        if (DisabledClockLevels.Contains(currentLevel))
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