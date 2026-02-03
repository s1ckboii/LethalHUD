using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.CustomHUD;

// HOTBARPLUS INCOMPAT
// Got an issue with changing to vanilla
// Wanna add a list of frame variants, one thats super clean
// Gotta work on the ScrapValueDisplay for this one to work with it well

internal static class CustomFrames
{
    private static Image[] _vanillaFrames;
    private static Image[] _vanillaIcons;

    private static Image[] _customFrames;
    private static Image[] _customIcons;
    private static GameObject[] _customRoots;

    private static int _lastSlotCount = -1;
    private static bool _enabled;

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
            BuildCustom(hud);
            _lastSlotCount = currentFrames.Length;
        }

        Apply(Plugins.ConfigEntries.CustomInventoryFrames.Value);
    }

    internal static void Apply(bool enable)
    {
        if (_customFrames == null || _vanillaFrames == null)
            return;

        if (_enabled == enable)
            return;

        if (enable)
            EnableCustom();
        else
            RestoreVanilla();

        _enabled = enable;
    }

    private static void CacheVanilla(HUDManager hud)
    {
        _vanillaFrames = hud.itemSlotIconFrames;
        _vanillaIcons = hud.itemSlotIcons;
    }

    private static void BuildCustom(HUDManager hud)
    {
        if (Plugins.LHSlotPrefab == null)
        {
            Loggers.Error("CustomFrames: LHSlotPrefab is null.");
            return;
        }

        int count = _vanillaFrames.Length;

        _customFrames = new Image[count];
        _customIcons = new Image[count];
        _customRoots = new GameObject[count];

        Transform parent = _vanillaFrames[0].transform.parent;

        for (int i = 0; i < count; i++)
        {
            GameObject slot = Object.Instantiate(Plugins.LHSlotPrefab, parent);
            slot.name = $"LHSlot_{i}";
            slot.transform.localPosition = _vanillaFrames[i].transform.localPosition;
            slot.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
            slot.transform.localScale = Vector3.one;
            slot.SetActive(false);

            LHSlotRefs refs = slot.GetComponent<LHSlotRefs>();
            if (refs == null || refs.Frame == null || refs.Icon == null)
            {
                Loggers.Error($"CustomFrames: LHSlotRefs missing (slot {i})");
                Object.Destroy(slot);
                continue;
            }

            _customFrames[i] = refs.Frame;
            _customIcons[i] = refs.Icon;
            _customRoots[i] = slot;
        }
    }

    private static void EnableCustom()
    {
        for (int i = 0; i < _vanillaFrames.Length; i++)
        {
            _vanillaFrames[i]?.gameObject.SetActive(false);
            _vanillaIcons[i]?.gameObject.SetActive(false);
            _customRoots[i]?.SetActive(true);

            _customIcons[i].sprite = _vanillaIcons[i].sprite;
            _customIcons[i].enabled = _vanillaIcons[i].enabled;
        }

        HUDManager.Instance.itemSlotIconFrames = _customFrames;
        HUDManager.Instance.itemSlotIcons = _customIcons;
    }

    private static void RestoreVanilla()
    {
        for (int i = 0; i < _vanillaFrames.Length; i++)
        {
            _customRoots[i]?.SetActive(false);
            _vanillaFrames[i]?.gameObject.SetActive(true);
            _vanillaIcons[i]?.gameObject.SetActive(true);
        }

        HUDManager.Instance.itemSlotIconFrames = _vanillaFrames;
        HUDManager.Instance.itemSlotIcons = _vanillaIcons;
    }

    private static void CleanupCustom()
    {
        if (_customRoots == null)
            return;

        for (int i = 0; i < _customRoots.Length; i++)
        {
            if (_customRoots[i] != null)
                Object.Destroy(_customRoots[i]);
        }

        _customRoots = null;
        _customFrames = null;
        _customIcons = null;
        _enabled = false;
    }
}
