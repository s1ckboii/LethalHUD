using System.Collections;
using LethalHUD.CustomHUD.Refs;
using LethalHUD.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.CustomHUD;
internal static class CustomFrames
{
    private static Image[] _vanillaFrames;
    private static Image[] _vanillaIcons;

    private static Image[] _customFrames;
    private static Image[] _customIcons;
    private static GameObject[] _customRoots;

    private static string _activeStyle = "Default";
    private static Coroutine _initializationRoutine;
    private static bool _isApplying = false;

    internal static void OnHUDEnable(HUDManager hud)
    {
        if (hud == null) return;

        _activeStyle = "Default";
        _vanillaFrames = null;
        _vanillaIcons = null;

        if (_initializationRoutine != null)
            hud.StopCoroutine(_initializationRoutine);

        _initializationRoutine = hud.StartCoroutine(DeferredApply(hud));
    }

    private static IEnumerator DeferredApply(HUDManager hud)
    {
        yield return new WaitForEndOfFrame();

        if (hud == null || hud.itemSlotIconFrames == null || hud.itemSlotIcons == null)
            yield break;

        CacheVanilla(hud);
        Apply(Plugins.ConfigEntries.CustomInventoryFrames.Value);
        _initializationRoutine = null;
    }

    internal static void Apply(string style)
    {
        if (_isApplying) return;

        HUDManager hud = HUDManager.Instance;
        if (hud == null) return;

        if (!IsVanillaValid())
        {
            if (hud.itemSlotIconFrames != null && hud.itemSlotIconFrames != _customFrames)
                CacheVanilla(hud);
            else if (_activeStyle != "Default")
                return;
        }

        if (_activeStyle == style && _customFrames != null && hud.itemSlotIconFrames == _customFrames)
            return;

        _isApplying = true;
        RestoreVanilla();

        if (style == "Default")
        {
            CleanupCustom();
            _activeStyle = style;
            _isApplying = false;
            return;
        }

        CleanupCustom();

        if (!Plugins.SlotPrefabs.TryGetValue(style, out GameObject prefab) || prefab == null)
        {
            _activeStyle = "Default";
            _isApplying = false;
            return;
        }

        _activeStyle = style;
        BuildCustom(hud, prefab);
        EnableCustom();

        _isApplying = false;
    }

    private static bool IsVanillaValid()
    {
        if (_vanillaFrames == null || _vanillaFrames.Length == 0) return false;
        foreach (Image frame in _vanillaFrames) if (frame == null) return false;
        return true;
    }

    private static void CacheVanilla(HUDManager hud)
    {
        _vanillaFrames = (Image[])hud.itemSlotIconFrames.Clone();
        _vanillaIcons = (Image[])hud.itemSlotIcons.Clone();
    }

    private static void BuildCustom(HUDManager hud, GameObject prefab)
    {
        int count = _vanillaFrames.Length;
        _customFrames = new Image[count];
        _customIcons = new Image[count];
        _customRoots = new GameObject[count];

        Transform parent = _vanillaFrames[0].transform.parent;

        for (int i = 0; i < count; i++)
        {
            GameObject slot = Object.Instantiate(prefab, parent);
            slot.name = $"{prefab.name}_{i}";
            slot.transform.localPosition = _vanillaFrames[i].transform.localPosition;
            slot.transform.localScale = Vector3.one;

            LHSlotRefs refs = slot.GetComponent<LHSlotRefs>();
            if (refs == null || refs.Frame == null || refs.Icon == null)
            {
                _customFrames[i] = _vanillaFrames[i];
                _customIcons[i] = _vanillaIcons[i];
                _customRoots[i] = slot;
                continue;
            }

            _customFrames[i] = refs.Frame;
            _customIcons[i] = refs.Icon;
            _customRoots[i] = slot;
        }
    }

    private static void EnableCustom()
    {
        HUDManager hud = HUDManager.Instance;
        Color flowColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.CustomFrameShaderColor.Value, Color.yellow);

        for (int i = 0; i < _vanillaFrames.Length; i++)
        {
            if (_vanillaFrames[i] == null || _vanillaIcons[i] == null) continue;

            _vanillaFrames[i].gameObject.SetActive(false);
            _vanillaIcons[i].gameObject.SetActive(false);

            if (_customRoots[i] != null)
            {
                _customRoots[i].SetActive(true);
                _customIcons[i].sprite = _vanillaIcons[i].sprite;
                _customIcons[i].enabled = _vanillaIcons[i].enabled;
                _customIcons[i].color = _vanillaIcons[i].color;

                if (_customFrames[i].material != null)
                {
                    _customFrames[i].material = Object.Instantiate(_customFrames[i].material);
                    _customFrames[i].material.SetColor("_FlowColor", flowColor);
                }
            }
        }

        hud.itemSlotIconFrames = _customFrames;
        hud.itemSlotIcons = _customIcons;
        SafeRefresh();
    }

    private static void RestoreVanilla()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null || _vanillaFrames == null) return;

        hud.itemSlotIconFrames = _vanillaFrames;
        hud.itemSlotIcons = _vanillaIcons;

        for (int i = 0; i < _vanillaFrames.Length; i++)
        {
            if (_vanillaFrames[i] != null)
                _vanillaFrames[i].gameObject.SetActive(true);

            if (_vanillaIcons[i] != null)
                _vanillaIcons[i].gameObject.SetActive(true);
        }

        SafeRefresh();
    }

    private static void SafeRefresh()
    {
        try { ScrapValueDisplay.RefreshSlots(); } catch { }
    }

    private static void CleanupCustom()
    {
        if (_customRoots == null) return;
        for (int i = 0; i < _customRoots.Length; i++)
        {
            if (_customRoots[i] != null) Object.Destroy(_customRoots[i]);
        }
        _customRoots = null;
        _customFrames = null;
        _customIcons = null;
    }

    internal static void UpdateShaderColor()
    {
        if (_customFrames == null || _activeStyle == "Default") return;
        Color flowColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.CustomFrameShaderColor.Value, Color.yellow);
        foreach (Image frame in _customFrames)
        {
            if (frame != null && frame.material != null)
                frame.material.SetColor("_FlowColor", flowColor);
        }
    }
}