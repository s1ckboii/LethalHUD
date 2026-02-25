using LethalHUD.CustomHUD.Refs;
using LethalHUD.HUD;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.CustomHUD;
internal static class CustomFrames
{
    private static Image[] _vanillaFrames;
    private static Image[] _vanillaIcons;

    private static Image[] _customFrames;
    private static Image[] _customIcons;
    private static GameObject[] _customRoots;

    private static LHSlotRefs[] _slotRefs;

    private static int _lastSlotCount = -1;
    private static InventoryFrameStyle _activeStyle = InventoryFrameStyle.Default;


    // Vanilla frames are cached but if the players starts with a custom one, its gonna use the first custom one as default..
    // Smol brain go bloink
    internal static void OnHUDEnable(HUDManager hud)
    {
        if (hud == null)
            return;

        Image[] currentFrames = hud.itemSlotIconFrames;
        Image[] currentIcons = hud.itemSlotIcons;

        if (currentFrames == null || currentIcons == null)
            return;

        bool slotCountChanged = _lastSlotCount != currentFrames.Length;

        if (slotCountChanged)
        {
            CleanupCustom();
            CacheVanilla(hud);
            _lastSlotCount = currentFrames.Length;
        }

        Apply(Plugins.ConfigEntries.CustomInventoryFrames.Value);
    }

    internal static void Apply(InventoryFrameStyle style)
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null) return;

        if (_vanillaFrames == null)
            return;

        if (_activeStyle == style && _customFrames != null && _customFrames.Length == hud.itemSlotIconFrames.Length)
            return;

        if (_vanillaFrames == null || _vanillaFrames.Length != hud.itemSlotIconFrames.Length)
        {
            _vanillaFrames = hud.itemSlotIconFrames;
            _vanillaIcons = hud.itemSlotIcons;
            _lastSlotCount = _vanillaFrames.Length;
        }

        RestoreVanilla();
        CleanupCustom();

        if (style == InventoryFrameStyle.Default)
        {
            _activeStyle = style;
            return;
        }

        if (!Plugins.SlotPrefabs.TryGetValue(style, out GameObject prefab) || prefab == null)
        {
            Loggers.Error($"CustomFrames: Missing slot prefab for style {style}");
            _activeStyle = InventoryFrameStyle.Default;
            return;
        }

        BuildCustom(HUDManager.Instance, prefab);
        EnableCustom();

        _activeStyle = style;
    }

    private static void CacheVanilla(HUDManager hud)
    {
        _vanillaFrames = hud.itemSlotIconFrames;
        _vanillaIcons = hud.itemSlotIcons;
    }

    private static void BuildCustom(HUDManager hud, GameObject prefab)
    {
        int count = _vanillaFrames.Length;

        _customFrames = new Image[count];
        _customIcons = new Image[count];
        _customRoots = new GameObject[count];
        _slotRefs = new LHSlotRefs[count];

        Transform parent = _vanillaFrames[0].transform.parent;

        for (int i = 0; i < count; i++)
        {
            GameObject slot = Object.Instantiate(prefab, parent);
            slot.name = $"LHSlot_{_activeStyle}_{i}";
            slot.transform.localPosition = _vanillaFrames[i].transform.localPosition;
            slot.transform.localScale = Vector3.one;
            slot.SetActive(false);

            LHSlotRefs refs = slot.GetComponent<LHSlotRefs>();
            if (refs == null || refs.Frame == null || refs.Icon == null)
            {
                Loggers.Error($"CustomFrames: LHSlotRefs missing (slot {i})");

                _customFrames[i] = _vanillaFrames[i];
                _customIcons[i] = _vanillaIcons[i];
                _customRoots[i] = null;
                continue;
            }

            _customFrames[i] = refs.Frame;
            _customIcons[i] = refs.Icon;
            _customRoots[i] = slot;
            _slotRefs[i] = refs;
        }
    }

    private static void EnableCustom()
    {
        var hud = HUDManager.Instance;
        var sourceIcons = _vanillaIcons;

        Color flowColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.CustomFrameShaderColor.Value, Color.yellow);

        for (int i = 0; i < _vanillaFrames.Length; i++)
        {
            if (sourceIcons[i] == null || _customIcons[i] == null) continue;

            _vanillaFrames[i]?.gameObject.SetActive(false);
            _vanillaIcons[i]?.gameObject.SetActive(false);

            _customRoots[i]?.SetActive(true);

            _customIcons[i].sprite = sourceIcons[i].sprite;
            _customIcons[i].enabled = sourceIcons[i].enabled;
            _customIcons[i].color = sourceIcons[i].color;

            Image frame = _customFrames[i];
            if (frame != null && frame.material != null)
            {
                frame.material = Object.Instantiate(frame.material);
                frame.material.SetColor("_FlowColor", flowColor);
            }
        }

        if (_customFrames.Length == hud.itemSlotIconFrames.Length)
        {
            hud.itemSlotIconFrames = _customFrames;
            hud.itemSlotIcons = _customIcons;
        }

        ScrapValueDisplay.RefreshSlots();
    }

    private static void RestoreVanilla()
    {
        if (_vanillaFrames == null || HUDManager.Instance == null)
            return;

        var hud = HUDManager.Instance;
        var sourceIcons = hud.itemSlotIcons;

        int count = Mathf.Min(_vanillaFrames.Length, _vanillaIcons?.Length ?? 0, sourceIcons?.Length ?? 0);

        for (int i = 0; i < count; i++)
        {
            if (sourceIcons[i] != null && _vanillaIcons[i] != null)
            {
                _vanillaIcons[i].sprite = sourceIcons[i].sprite;
                _vanillaIcons[i].enabled = sourceIcons[i].enabled;
                _vanillaIcons[i].color = sourceIcons[i].color;
            }

            if (i < _customRoots?.Length)
                _customRoots[i]?.SetActive(false);

            _vanillaFrames[i]?.gameObject.SetActive(true);
            _vanillaIcons[i]?.gameObject.SetActive(true);
        }

        hud.itemSlotIconFrames = _vanillaFrames;
        hud.itemSlotIcons = _vanillaIcons;

        ScrapValueDisplay.RefreshSlots();
    }

    private static void CleanupCustom()
    {
        if (_customRoots == null)
            return;

        for (int i = 0; i < _customRoots.Length; i++)
        {
            if (_customRoots[i] != null)
                _customRoots[i].SetActive(false);
        }

        _customRoots = null;
        _customFrames = null;
        _customIcons = null;
    }
    internal static void UpdateShaderColor()
    {
        if (_customFrames == null || _activeStyle == InventoryFrameStyle.Default)
            return;

        Color flowColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.CustomFrameShaderColor.Value, Color.yellow);

        for (int i = 0; i < _customFrames.Length; i++)
        {
            Image frame = _customFrames[i];
            if (frame == null || frame.material == null)
                continue;

            frame.material.SetColor("_FlowColor", flowColor);
        }
    }
}
