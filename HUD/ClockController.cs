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

    private static bool _skipAlphaCheck;
    private static bool _defaultsCached;

    private static Vector3 _defaultClockPos;
    private static Vector3 _defaultIconPos;
    private static Vector2 _defaultParentSize;
    private static Vector2 _defaultIconSize;
    private static bool _defaultWordWrap;

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

        if (_clockNumber != null )
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

        if (Plugins.ConfigEntries.ClockFormat.Value == ClockStyle.Compact)
            ApplyCompactLayout();
        else
            ApplyRegularLayout();
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
        if (visible)
        {
            _skipAlphaCheck = true;
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

        CanvasGroup canvasGroup = _clockParent.GetComponent<CanvasGroup>() ?? _clockParent.gameObject.AddComponent<CanvasGroup>();
        float targetAlpha = GetTargetAlpha();
        canvasGroup.alpha = targetAlpha;
    }

    private static float GetTargetAlpha()
    {
        if (_skipAlphaCheck)
        {
            _skipAlphaCheck = false;
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