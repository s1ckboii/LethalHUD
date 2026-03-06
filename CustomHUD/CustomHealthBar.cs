using GameNetcodeStuff;
using LethalHUD.CustomHUD.Refs;
using LethalHUD.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.CustomHUD;
internal static class CustomHealthBar
{
    private static LHHealthBarRefs _refs;
    private static GameObject _root;

    private static Image _vanillaFrameImage;
    private static Image _vanillaPulseImage;

    private static GameObject _vanillaFrameObj;

    private static Vector3 _vanillaFrameScale = Vector3.one;

    private static string _activeStyle = "Default";

    internal static bool UsingCustom => _activeStyle != "Default";
    internal static bool HasCustomNumber => _refs != null && _refs.Number != null;

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

        if (hud.localPlayer != null)
        {
            UpdateFromPlayer(hud.localPlayer);
        }
    }
    internal static void Apply(string style)
    {
        if (_activeStyle == style && (style == "Default" || _root != null)) return;

        Cleanup();

        if (style == "Default")
        {
            _activeStyle = style;
            RestoreVanilla();
            return;
        }

        if (!Plugins.HealthBarPrefabs.TryGetValue(style, out GameObject prefab) || prefab == null)
        {
            Cleanup();
            _activeStyle = "Default";
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
    private static void Build(GameObject prefab, string style)
    {
        Transform hudParent = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner")?.transform;

        if (hudParent == null) return;

        _root = Object.Instantiate(prefab, hudParent, false);
        _root.name = prefab.name;

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

        _refs = _root.GetComponent<LHHealthBarRefs>();
        _root.transform.SetAsLastSibling();
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