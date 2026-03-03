using GameNetcodeStuff;
using LethalHUD.CustomHUD.Refs;
using LethalHUD.HUD;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;
using System.Collections.Generic;

namespace LethalHUD.CustomHUD;

internal static class CustomStaminaMeter
{
    private static LHStaminaBarRefs _refs;
    private static GameObject _root;
    private static readonly Dictionary<StaminaBarStyle, GameObject> _pool = [];

    private static Image _vanillaMeterFill;
    private static Image _vanillaMeterFrame;
    private static StaminaBarStyle _activeStyle = StaminaBarStyle.Default;

    internal static LHStaminaBarRefs Refs => _refs;
    internal static bool UsingCustom => _activeStyle != StaminaBarStyle.Default;

    internal static void Init(PlayerControllerB player)
    {
        if (player == null || player.sprintMeterUI == null) return;
        
        _vanillaMeterFill = player.sprintMeterUI;
        _vanillaMeterFrame = _vanillaMeterFill.transform.parent.GetComponent<Image>();

        Apply(Plugins.ConfigEntries.CustomStaminaBar.Value);
    }

    internal static void Apply(StaminaBarStyle style)
    {
        if (_root != null) _root.SetActive(false);

        if (style == StaminaBarStyle.Default)
        {
            _activeStyle = style;
            RestoreVanilla();
            return;
        }

        if (_pool.TryGetValue(style, out GameObject existing) && existing != null)
        {
            _root = existing;
            _root.SetActive(true);
        }
        else if (Plugins.StaminaBarPrefabs.TryGetValue(style, out GameObject prefab) && prefab != null)
        {
            Build(prefab, style);
        }

        _refs = _root?.GetComponent<LHStaminaBarRefs>();
        _activeStyle = style;

        UpdateShaderColor();
        HideVanilla();
    }
    private static void Build(GameObject prefab, StaminaBarStyle style)
    {
        Transform hudParent = _vanillaMeterFill?.transform.parent?.parent;

        if (hudParent == null || hudParent.name != "TopLeftCorner")
        {
            hudParent = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner")?.transform;
        }

        if (hudParent == null) return;

        _root = Object.Instantiate(prefab, hudParent, false);
        _root.name = $"LHStaminaBar_{style}";
        _root.transform.SetAsLastSibling();
        _pool[style] = _root;

        RectTransform rect = _root.GetComponent<RectTransform>();
        RectTransform prefabRect = prefab.GetComponent<RectTransform>();

        if (rect != null && prefabRect != null)
        {
            rect.anchorMin = prefabRect.anchorMin;
            rect.anchorMax = prefabRect.anchorMax;
            rect.pivot = prefabRect.pivot;
            rect.anchoredPosition = prefabRect.anchoredPosition;
            rect.sizeDelta = prefabRect.sizeDelta;
            rect.localScale = prefabRect.localScale;
        }
    }
    private static void HideVanilla()
    {
        if (_vanillaMeterFill != null)
            _vanillaMeterFill.gameObject.SetActive(false);

        if (_vanillaMeterFrame != null)
            _vanillaMeterFrame.gameObject.SetActive(false);
    }

    private static void RestoreVanilla()
    {
        if (_vanillaMeterFill != null)
            _vanillaMeterFill.gameObject.SetActive(true);

        if (_vanillaMeterFrame != null)
            _vanillaMeterFrame.gameObject.SetActive(true);
    }

    internal static void UpdateFromPlayer(PlayerControllerB player)
    {
        if (!UsingCustom || _root == null || !_root.activeSelf || _refs == null || player == null) return;

        float fillAmount = player.sprintMeterUI.fillAmount;

        _refs.UpdateStaminaUI(fillAmount, _refs.Fill.color);
    }

    internal static void Cleanup()
    {
        foreach (var obj in _pool.Values) if (obj != null) Object.Destroy(obj);
        _pool.Clear();
        _root = null;
    }

    internal static void UpdateShaderColor()
    {
        if (_refs == null) return;
        Color flowColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.CustomStaminaShaderColor.Value, Color.white);
        _refs.SetFlowColor(flowColor);
    }
}