using GameNetcodeStuff;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class ClockController
{
    private static readonly HashSet<SelectableLevel> DisabledClockLevels = [];

    private static TextMeshProUGUI _clockNumber;
    private static Image _clockIcon;
    private static Transform _clockParent;
    private static Image _boxImage;

    private static bool _defaultsCached;

    private static Vector3 _defaultClockPos;
    private static Vector3 _defaultIconPos;
    private static Vector2 _defaultParentSize;
    private static Vector2 _defaultIconSize;
    private static bool _defaultWordWrap;

    private static int _prevMinutes = -1;
    private static int _prevHours = -1;

    private static ClockStyle _lastLayout;

    internal static void CacheDefaultLayout()
    {
        if (_clockParent == null || _clockNumber == null || _clockIcon == null)
            return;

        RectTransform parentRect = _clockParent.GetComponent<RectTransform>();
        RectTransform iconRect = _clockIcon.GetComponent<RectTransform>();

        _defaultParentSize = parentRect.sizeDelta;
        _defaultIconSize = iconRect.sizeDelta;
        _defaultClockPos = _clockNumber.transform.localPosition;
        _defaultIconPos = _clockIcon.transform.localPosition;
        _defaultWordWrap = _clockNumber.enableWordWrapping;
    }

    internal static void ApplyClockAppearance()
    {
        if (HUDManager.Instance == null) return;

        _clockNumber = HUDManager.Instance.clockNumber;
        _clockIcon = HUDManager.Instance.clockIcon;
        _clockParent = _clockNumber.transform.parent;
        _boxImage = _clockParent.GetComponent<Image>();

        if (_clockNumber != null)
            _clockNumber.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ClockNumberColor.Value, Color.white);

        if (_boxImage != null)
            _boxImage.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ClockBoxColor.Value, Color.white);

        if (_clockIcon != null)
            _clockIcon.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ClockIconColor.Value, new Color(1f, 0.31f, 0f));

        if (HUDManager.Instance.shipLeavingEarlyIcon != null)
            HUDManager.Instance.shipLeavingEarlyIcon.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ClockShipLeaveColor.Value, Color.white);

        Vector3 baseScale = new(-0.5893304f, 0.5893304f, 0.5893303f);
        _clockParent.localScale = baseScale * Plugins.ConfigEntries.ClockSizeMultiplier.Value;

        if (!_defaultsCached)
        {
            CacheDefaultLayout();
            _defaultsCached = true;
        }

        _lastLayout = Plugins.ConfigEntries.ClockFormat.Value;

        if (_lastLayout == ClockStyle.Compact)
            ApplyCompactLayout();
        else
            ApplyRegularLayout();
    }

    internal static void TryOverrideClock(HUDManager hud, float timeNormalized, float numberOfHours)
    {
        if (hud == null || hud.clockNumber == null)
            return;

        bool use24h = Plugins.ConfigEntries.NormalHumanBeingClock.Value;
        ClockStyle style = Plugins.ConfigEntries.ClockFormat.Value;

        int totalMinutes = (int)(timeNormalized * (60f * numberOfHours)) + 360;
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        string formatted;

        if (use24h)
        {
            formatted = $"{hours % 24:00}:{minutes:00}";
        }
        else
        {
            int displayHour = hours % 12;
            if (displayHour == 0)
                displayHour = 12;

            string separator = style == ClockStyle.Compact ? " " : "\n";
            string ampm = hours >= 12 ? "PM" : "AM";

            formatted = $"{displayHour:00}:{minutes:00}{separator}{ampm}";
        }

        bool timeUnchanged = minutes == _prevMinutes && hours == _prevHours;
        bool textAlreadyCorrect = hud.clockNumber.text == formatted;

        if (timeUnchanged && textAlreadyCorrect && style == _lastLayout)
            return;

        _prevMinutes = minutes;
        _prevHours = hours;

        hud.clockNumber.text = formatted;

        if (style != _lastLayout)
        {
            _lastLayout = style;

            if (style == ClockStyle.Compact)
                ApplyCompactLayout();
            else
                ApplyRegularLayout();
        }
    }
    internal static void ApplyCompactLayout()
    {
        if (_clockParent == null || _clockNumber == null || _clockIcon == null)
            return;

        RectTransform parentRect = _clockParent.GetComponent<RectTransform>();
        parentRect.sizeDelta = new Vector2(parentRect.sizeDelta.x, 50f);

        _clockNumber.enableWordWrapping = false;

        RectTransform iconRect = _clockIcon.GetComponent<RectTransform>();
        iconRect.sizeDelta = _defaultIconSize * 0.6f;

        if (Plugins.ConfigEntries.NormalHumanBeingClock.Value)
        {
            _clockNumber.transform.localPosition = _defaultClockPos + new Vector3(-10f, 0f, 0f);
            _clockIcon.transform.localPosition = _defaultIconPos + new Vector3(-10f, 0f, 0f);
        }
        else
        {
            _clockNumber.transform.localPosition = _defaultClockPos + new Vector3(10f, 0f, 0f);
            _clockIcon.transform.localPosition = _defaultIconPos + new Vector3(-25f, 0f, 0f);
        }
    }
    internal static void ApplyRegularLayout()
    {
        if (_clockParent == null || _clockNumber == null || _clockIcon == null)
            return;

        RectTransform parentRect = _clockParent.GetComponent<RectTransform>();
        RectTransform iconRect = _clockIcon.GetComponent<RectTransform>();

        parentRect.sizeDelta = _defaultParentSize;
        iconRect.sizeDelta = _defaultIconSize;
        _clockNumber.transform.localPosition = _defaultClockPos;
        _clockIcon.transform.localPosition = _defaultIconPos;
        _clockNumber.enableWordWrapping = _defaultWordWrap;
    }
    internal static void UpdateClockVisibility(ref bool visible)
    {
        StartOfRound round = StartOfRound.Instance;
        PlayerControllerB localPlayer = round != null ? round.localPlayerController : null;
        if (localPlayer == null)
            return;

        if (localPlayer.inTerminalMenu || DisabledClockLevels.Contains(round.currentLevel))
        {
            visible = false;
            return;
        }

        if (localPlayer.isInHangarShipRoom)
            visible = Plugins.ConfigEntries.ShowClockInShip.Value;
        else if (localPlayer.isInsideFactory)
            visible = Plugins.ConfigEntries.ShowClockInFacility.Value;
    }

    internal static void ApplyClockAlpha(HUDManager hud, bool visible)
    {
        if (hud == null || hud.Clock == null) return;

        hud.Clock.targetAlpha = visible ? GetTargetAlpha() : 0f;
    }

    private static float GetTargetAlpha()
    {
        StartOfRound round = StartOfRound.Instance;
        PlayerControllerB player = round != null ? round.localPlayerController : null;
        if (player == null) return 1f;

        if (player.isInHangarShipRoom)
            return Mathf.Clamp01(Plugins.ConfigEntries.ClockVisibilityInShip.Value);

        if (player.isInsideFactory)
            return Mathf.Clamp01(Plugins.ConfigEntries.ClockVisibilityInFacility.Value);

        return 1f;
    }

    internal static void ApplyRealtimeClock()
    {
        if (!Plugins.ConfigEntries.RealtimeClock.Value) return;

        if (TimeOfDay.Instance != null)
            TimeOfDay.Instance.changeHUDTimeInterval = 4f;
    }
}