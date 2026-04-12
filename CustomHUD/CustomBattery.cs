using GameNetcodeStuff;
using LethalHUD.CustomHUD.Refs;
using LethalHUD.HUD;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace LethalHUD.CustomHUD;
internal static class CustomBattery
{
    private static LHBatteryRefs _refs;
    private static GameObject _root;
    private static readonly Dictionary<string, GameObject> _pool = [];

    private static Image _vanillaMeter;
    private static Image _vanillaIcon;

    private static string _activeStyle = "Default";

    internal static bool UsingCustom => _activeStyle != "Default";
    internal static LHBatteryRefs Refs => _refs;

    internal static void Init()
    {
        var hud = HUDManager.Instance;
        if (hud == null) return;

        _vanillaMeter = hud.batteryMeter;
        _vanillaIcon = hud.batteryIcon;

        Apply(Plugins.ConfigEntries.CustomBatteryBar.Value);
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
        else if (Plugins.BatteryPrefabs.TryGetValue(style, out var entry) && entry.Asset != null)
        {
            Build(entry.Asset, style);
        }
        else
        {
            _activeStyle = "Default";
            RestoreVanilla();
            return;
        }

        _refs = _root?.GetComponent<LHBatteryRefs>();
        _activeStyle = style;

        UpdateColor();
        HideVanilla();
    }

    private static void Build(GameObject prefab, string style)
    {
        var hud = HUDManager.Instance;
        if (hud == null) return;

        Transform parent = hud.batteryMeter.transform.parent;
        if (parent == null) return;

        _root = Object.Instantiate(prefab, parent, false);
        _root.name = prefab.name;
        _root.transform.SetAsLastSibling();

        _pool[style] = _root;
    }

    private static void HideVanilla()
    {
        var hud = HUDManager.Instance;

        if (_vanillaMeter != null)
            _vanillaMeter.gameObject.SetActive(false);

        if (_vanillaIcon != null)
            _vanillaIcon.enabled = false;

        if (hud?.batteryBlinkUI != null)
            hud.batteryBlinkUI.gameObject.SetActive(false);

        GameObject tpc = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/");
        if (tpc != null)
        {
            var off = tpc.transform.Find("BatteriesOff");
            if (off != null)
                off.gameObject.SetActive(false);
        }
    }
    private static void RestoreVanilla()
    {
        var hud = HUDManager.Instance;

        if (_vanillaMeter != null)
            _vanillaMeter.gameObject.SetActive(true);

        if (_vanillaIcon != null)
            _vanillaIcon.enabled = true;

        if (hud?.batteryBlinkUI != null)
            hud.batteryBlinkUI.gameObject.SetActive(true);

        GameObject tpc = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/");
        if (tpc != null)
        {
            var off = tpc.transform.Find("BatteriesOff");
            if (off != null)
                off.gameObject.SetActive(true);
        }
    }

    internal static void Update(float fill, bool visible)
    {
        if (!UsingCustom || _refs == null) return;

        _root.SetActive(visible);

        _refs.UpdateBattery(fill);

        if (_refs.Icon != null)
            _refs.Icon.enabled = visible;

        if (_refs.Text != null)
            _refs.Text.text = Mathf.RoundToInt(fill * 100f) + "%";
    }

    internal static void UpdateColor()
    {
        if (_refs == null) return;

        Color col = HUDUtils.ParseHexColor(Plugins.ConfigEntries.CustomBatteryColor.Value, Color.white);
        _refs.SetColor(col);
    }

    internal static void Cleanup()
    {
        foreach (var obj in _pool.Values)
            if (obj != null) Object.Destroy(obj);

        _pool.Clear();
        _root = null;
    }
}