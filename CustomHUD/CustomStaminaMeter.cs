using GameNetcodeStuff;
using LethalHUD.CustomHUD.Refs;
using LethalHUD.HUD;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LethalHUD.CustomHUD;
internal static class CustomStaminaMeter
{
    private static LHStaminaBarRefs _refs;
    private static GameObject _root;
    private static readonly Dictionary<string, GameObject> _pool = [];

    private static Image _vanillaMeterFill;
    private static Image _vanillaMeterFrame;
    private static string _activeStyle = "Default";

    internal static LHStaminaBarRefs Refs => _refs;
    internal static bool UsingCustom => _activeStyle != "Default";

    internal static void Init(PlayerControllerB player)
    {
        if (player == null || player.sprintMeterUI == null) return;
        
        _vanillaMeterFill = player.sprintMeterUI;
        _vanillaMeterFrame = _vanillaMeterFill.transform.parent.GetComponent<Image>();

        Apply(Plugins.ConfigEntries.CustomStaminaBar.Value);
    }

    internal static void Apply(string style)
    {
        if (_root != null) _root.SetActive(false);

        if (style == "Default")
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
        else if (Plugins.StaminaBarPrefabs.TryGetValue(style, out var entry) && entry.Asset != null)
        {
            Build(entry.Asset, style);
        }
        else
        {
            _activeStyle = "Default";
            RestoreVanilla();
            return;
        }

        _refs = _root?.GetComponent<LHStaminaBarRefs>();
        _activeStyle = style;

        UpdateShaderColor();
        HideVanilla();
    }
    private static void Build(GameObject prefab, string style)
    {
        Transform hudParent = _vanillaMeterFill?.transform.parent?.parent;

        if (hudParent == null || hudParent.name != "TopLeftCorner")
        {
            hudParent = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner")?.transform;
        }

        if (hudParent == null) return;

        _root = Object.Instantiate(prefab, hudParent, false);
        _root.name = prefab.name;
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