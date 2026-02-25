using GameNetcodeStuff;
using LethalHUD.CustomHUD.Refs;
using LethalHUD.HUD;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.CustomHUD;

internal static class CustomHealthBar
{
    private static LHHealthBarRefs _refs;
    private static GameObject _root;

    private static Image _vanillaFrameImage;
    private static Image _vanillaPulseImage;

    private static GameObject _vanillaFrameObj;

    private static Vector3 _vanillaFrameScale = Vector3.one;

    private static HealthBarStyle _activeStyle = HealthBarStyle.Default;

    internal static bool UsingCustom => _activeStyle != HealthBarStyle.Default;

    internal static void OnHUDEnable(HUDManager hud)
    {
        if (hud == null) return;

        if (hud.selfRedCanvasGroup != null)
        {
            _vanillaPulseImage = hud.selfRedCanvasGroup.GetComponent<Image>();

            Transform frameTransform = hud.selfRedCanvasGroup.transform.parent.Find("Self");
            if (frameTransform != null)
            {
                _vanillaFrameImage = frameTransform.GetComponent<Image>();
                _vanillaFrameObj = frameTransform.gameObject;

                _vanillaFrameScale = frameTransform.localScale;
            }
        }

        Apply(Plugins.ConfigEntries.CustomHealthBar.Value);
    }
    internal static void Apply(HealthBarStyle style)
    {
        if (_activeStyle == style && (style == HealthBarStyle.Default || _root != null)) return;

        Cleanup();

        if (style == HealthBarStyle.Default)
        {
            _activeStyle = style;
            RestoreVanilla();
            return;
        }

        if (!Plugins.HealthBarPrefabs.TryGetValue(style, out GameObject prefab) || prefab == null)
        {
            _activeStyle = HealthBarStyle.Default;
            RestoreVanilla();
            return;
        }

        Build(prefab, style);

        UpdateShaderColor();
        HideVanilla();
        _activeStyle = style;
    }
    internal static void UpdateFromPlayer(PlayerControllerB player)
    {
        if (_refs == null || player == null) return;

        int health = player.health;
        float hpFill = Mathf.Clamp01(health / 100f);
        float ohFill = (health > 100) ? Mathf.Clamp01((health - 100f) / 100f) : 0f;

        Color hpColor = HUDUtils.GetHPColor(Mathf.Min(health, 100));
        Color ohColor = (health > 100) ? HUDUtils.GetHPColor(health) : Color.clear;

        _refs.UpdateHealthUI(health, hpFill, hpColor, ohFill, ohColor);

        if (_refs.Number != null)
        {
            PlayerHPDisplay.UpdateNumber(_refs.Number, _refs.NumberBasePosition);
        }
    }
    private static void Build(GameObject prefab, HealthBarStyle style)
    {
        Transform hudParent = _vanillaPulseImage?.transform.parent;
        if (hudParent == null) return;

        _root = Object.Instantiate(prefab, hudParent, false);
        _root.name = $"LHHealthBar_{style}";
        _root.transform.SetAsLastSibling();

        _refs = _root.GetComponent<LHHealthBarRefs>();

        RectTransform rect = _root.GetComponent<RectTransform>();
        if (rect != null && _vanillaFrameImage != null)
        {
            RectTransform vanillaRect = _vanillaFrameImage.rectTransform;

            rect.anchorMin = vanillaRect.anchorMin;
            rect.anchorMax = vanillaRect.anchorMax;
            rect.pivot = vanillaRect.pivot;
            rect.sizeDelta = vanillaRect.sizeDelta;
            rect.anchoredPosition = vanillaRect.anchoredPosition;
            rect.localScale = Vector3.one;

            if (_refs != null && _refs.Number != null)
            {
                RectTransform prefabRect = prefab.GetComponent<LHHealthBarRefs>().Number.rectTransform;
                _refs.Number.rectTransform.anchoredPosition = prefabRect.anchoredPosition;
                _refs.Number.rectTransform.localPosition = prefabRect.localPosition;
            }
        }
    }
    internal static void UpdateShaderColor()
    {
        if (_refs == null) return;

        Color flowColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.CustomHealthShaderColor.Value, Color.yellow);

        _refs.SetFlowColor(flowColor);
    }
    private static void HideVanilla()
    {
        if (_vanillaPulseImage != null) _vanillaPulseImage.enabled = false;
        if (_vanillaFrameObj != null) _vanillaFrameObj.transform.localScale = Vector3.zero;
    }

    private static void RestoreVanilla()
    {
        if (_vanillaPulseImage != null) _vanillaPulseImage.enabled = true;
        if (_vanillaFrameImage != null) _vanillaFrameImage.enabled = true;

        if (_vanillaFrameObj != null)
        {
            _vanillaFrameObj.transform.localScale = _vanillaFrameScale;
        }

        PlayerRedCanvasController.ChangeSetting();
    }
    internal static void Cleanup()
    {
        if (_root != null) Object.Destroy(_root);
        _root = null;
        _refs = null;
    }
}