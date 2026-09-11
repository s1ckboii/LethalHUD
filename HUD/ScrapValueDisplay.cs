using GameNetcodeStuff;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class ScrapValueDisplay
{
    internal static TMP_Text[] slotTexts;

    private static TMP_Text _totalText;
    private static TMP_FontAsset _defaultFont;
    private static TMP_FontAsset _dollarFont;
    private static RectTransform totalRT;

    private static int[] _slotValues;

    private static int _lastTotal = 0;
    private static float _deltaTimer = 0f;
    private static string _deltaColor = "green";
    private static bool _erasingDelta = false;
    private static readonly float _eraseSpeed = 0.05f;
    private static float _eraseTimer = 0f;

    private const float InventorySyncInterval = 0.25f;
    private static float _inventorySyncTimer = 0f;

    private static readonly StringBuilder _deltaPlainBuilder = new();
    private static readonly StringBuilder _deltaTextBuilder = new();
    private static readonly StringBuilder _displayBuilder = new();

    internal static void Init()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null) return;

        _defaultFont = hud.totalValueText.font;
        _dollarFont = hud.chatText.font;

        SetupSlots();
    }

    internal static void SetupSlots()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null || hud.itemSlotIconFrames == null) return;

        int slotCount = hud.itemSlotIconFrames.Length;

        if (slotTexts == null || slotTexts.Length != slotCount)
        {
            slotTexts = new TMP_Text[slotCount];
            _slotValues = new int[slotCount];
        }

        for (int i = 0; i < slotCount; i++)
        {
            Image currentSlotFrame = hud.itemSlotIconFrames[i];
            if (currentSlotFrame == null) continue;

            bool needsUpdate = slotTexts[i] == null || slotTexts[i].gameObject == null || slotTexts[i].transform.parent != currentSlotFrame.transform;

            if (needsUpdate)
            {
                if (slotTexts[i] != null)
                    Object.Destroy(slotTexts[i].gameObject);

                foreach (Transform child in currentSlotFrame.transform)
                {
                    if (child.name == "InventoryScrapValueText")
                        Object.Destroy(child.gameObject);
                }

                CreateSlotTextForIndex(i, currentSlotFrame);
            }
        }

        SetupTotalText(hud);
        SyncFromLocalInventory(true);
    }

    private static void CreateSlotTextForIndex(int index, Image slot)
    {
        GameObject go = new("InventoryScrapValueText");
        go.transform.SetParent(slot.transform, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.localScale = Vector3.one * 0.75f;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.localRotation = Quaternion.identity;

        Vector3 offset = new(-0.015f, 0.025f, 0f);
        rt.position = slot.transform.position + offset;

        TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = 14;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ItemValueColor.Value, Color.green);
        tmp.raycastTarget = false;
        tmp.text = "";

        slotTexts[index] = tmp;
    }

    private static void SetupTotalText(HUDManager hud)
    {
        if (_totalText != null)
            Object.Destroy(_totalText.gameObject);

        if (hud.itemSlotIconFrames.Length == 0) return;

        GameObject totalGO = new("InventoryScrapTotalValueText");
        totalGO.transform.SetParent(hud.itemSlotIconFrames[0].transform.parent, false);

        totalRT = totalGO.AddComponent<RectTransform>();
        totalRT.localScale = Vector3.one * 0.5f;
        totalRT.localRotation = Quaternion.identity;
        totalRT.anchorMin = totalRT.anchorMax = new Vector2(0f, 0.5f);
        totalRT.pivot = new Vector2(1f, 0.5f);
        totalRT.localPosition = new Vector2(Plugins.ConfigEntries.TotalValueOffsetX.Value, Plugins.ConfigEntries.TotalValueOffsetY.Value);

        _totalText = totalGO.AddComponent<TextMeshProUGUI>();
        _totalText.fontSize = 14;
        _totalText.alignment = TextAlignmentOptions.Right;
        _totalText.color = Color.green;
        _totalText.raycastTarget = false;
        _totalText.text = "";
    }

    internal static void RefreshSlots() => SetupSlots();

    internal static void UpdateSlot(int slotIndex, int value)
    {
        if (!Plugins.ConfigEntries.ShowItemValue.Value) return;
        if (slotTexts == null || _slotValues == null || slotIndex < 0 || slotIndex >= slotTexts.Length || slotIndex >= _slotValues.Length) return;

        TMP_Text tmp = slotTexts[slotIndex];
        if (tmp == null) return;

        tmp.font = Plugins.ConfigEntries.SetDollar.Value == ItemValue.Default
            ? _defaultFont : _dollarFont;

        if (value > 0)
            tmp.text = $"${value}";
        else
            tmp.text = "";

        _slotValues[slotIndex] = value;
        UpdateInventoryTotal();
        UpdateSlotValueTextColors();
    }

    internal static void SyncFromLocalInventory(bool forceRefresh = false)
    {
        if (!Plugins.ConfigEntries.ShowItemValue.Value) return;

        PlayerControllerB player = GameNetworkManager.Instance?.localPlayerController;
        if (player?.ItemSlots == null || slotTexts == null || _slotValues == null) return;

        int count = Mathf.Min(player.ItemSlots.Length, Mathf.Min(slotTexts.Length, _slotValues.Length));
        bool changed = false;

        for (int i = 0; i < count; i++)
        {
            TMP_Text tmp = slotTexts[i];
            if (tmp == null) continue;

            GrabbableObject item = player.ItemSlots[i];
            int value = item != null ? item.scrapValue : 0;

            TMP_FontAsset wantedFont = Plugins.ConfigEntries.SetDollar.Value == ItemValue.Default
                ? _defaultFont : _dollarFont;

            if (tmp.font != wantedFont)
                tmp.font = wantedFont;

            bool textNeedsRestore = value > 0
                ? string.IsNullOrEmpty(tmp.text)
                : !string.IsNullOrEmpty(tmp.text);

            if (!forceRefresh && _slotValues[i] == value && !textNeedsRestore)
                continue;

            _slotValues[i] = value;
            tmp.text = value > 0 ? $"${value}" : "";
            changed = true;
        }

        for (int i = count; i < slotTexts.Length; i++)
        {
            if (i >= _slotValues.Length) break;

            TMP_Text tmp = slotTexts[i];

            if (!forceRefresh && _slotValues[i] == 0 && (tmp == null || string.IsNullOrEmpty(tmp.text)))
                continue;

            _slotValues[i] = 0;

            tmp?.text = "";

            changed = true;
        }

        if (changed || forceRefresh)
        {
            UpdateInventoryTotal();
            UpdateSlotValueTextColors();
        }
    }

    internal static void UpdateTotalTextPosition()
    {
        if (_totalText == null) return;
        RectTransform totalRT = _totalText.rectTransform;
        totalRT.localPosition = new Vector2(Plugins.ConfigEntries.TotalValueOffsetX.Value, Plugins.ConfigEntries.TotalValueOffsetY.Value);
    }

    internal static void Hide(int slotIndex)
    {
        if (slotTexts == null || _slotValues == null || slotIndex < 0 || slotIndex >= slotTexts.Length || slotIndex >= _slotValues.Length) return;

        TMP_Text tmp = slotTexts[slotIndex];
        if (tmp != null) tmp.text = "";

        _slotValues[slotIndex] = 0;
        UpdateInventoryTotal();
        UpdateSlotValueTextColors();
    }

    internal static void ClearItemSlots()
    {
        if (slotTexts == null || _slotValues == null) return;

        int count = Mathf.Min(slotTexts.Length, _slotValues.Length);

        for (int i = 0; i < count; i++)
        {
            if (slotTexts[i] != null)
                slotTexts[i].text = "";
            _slotValues[i] = 0;
        }

        UpdateInventoryTotal();
        UpdateSlotValueTextColors();
    }

    internal static void RefreshValueColors() => UpdateSlotValueTextColors();

    private static void UpdateSlotValueTextColors()
    {
        if (_slotValues == null || slotTexts == null) return;

        Color valueColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.ItemValueColor.Value, Color.green);

        if (!Plugins.ConfigEntries.ItemValueGradient.Value)
        {
            for (int i = 0; i < slotTexts.Length; i++)
            {
                TMP_Text tmp = slotTexts[i];
                if (tmp == null) continue;

                tmp.color = valueColor;
            }

            return;
        }

        int min = int.MaxValue;
        int max = int.MinValue;
        bool hasValues = false;

        for (int i = 0; i < _slotValues.Length; i++)
        {
            int v = _slotValues[i];
            if (v > 0)
            {
                hasValues = true;
                if (v < min) min = v;
                if (v > max) max = v;
            }
        }

        if (!hasValues) return;

        int count = Mathf.Min(slotTexts.Length, _slotValues.Length);

        for (int i = 0; i < count; i++)
        {
            TMP_Text tmp = slotTexts[i];
            if (tmp == null) continue;

            int v = _slotValues[i];
            tmp.color = v <= 0 ? Color.white : Color.Lerp(Color.white, valueColor, (max == min) ? 1f : Mathf.InverseLerp(min, max, v));
        }
    }

    private static void UpdateInventoryTotal()
    {
        if (_slotValues == null) return;

        if (!Plugins.ConfigEntries.ShowTotalInventoryValue.Value)
        {
            if (_totalText != null && _totalText.text != string.Empty)
                _totalText.text = string.Empty;
            return;
        }

        int total = 0;
        for (int i = 0; i < _slotValues.Length; i++)
            total += _slotValues[i];

        if (total != _lastTotal)
        {
            int diff = total - _lastTotal;
            if (diff != 0 && Plugins.ConfigEntries.ShowTotalDelta.Value)
            {
                string sign = diff > 0 ? "+" : "-";
                string numeric = $"{sign}${Mathf.Abs(diff)}";

                _deltaPlainBuilder.Clear();
                _deltaPlainBuilder.Append('(').Append(numeric).Append(')');

                _deltaTextBuilder.Clear();

                _deltaColor = diff > 0 ? "green" : "red";
                _deltaTextBuilder.Append("<color=").Append(_deltaColor).Append(">(").Append(numeric).Append(")</color>");

                _deltaTimer = 1.5f;
                _erasingDelta = false;
            }
            _lastTotal = total;
        }

        _displayBuilder.Clear();

        string prefix = Plugins.ConfigEntries.TotalPrefix.Value switch
        {
            TotalValuePrefix.Full => "Total Value: ",
            TotalValuePrefix.Short => "Total: ",
            TotalValuePrefix.None => "",
            _ => "Total Value: "
        };

        if (total > 0)
        {
            _displayBuilder.Append(prefix).Append('$').Append(total);
            if (_deltaTextBuilder.Length > 0)
                _displayBuilder.Append(_deltaTextBuilder);
        }

        if (_totalText != null)
        {
            _totalText.font = Plugins.ConfigEntries.SetDollar.Value == ItemValue.Default
                ? _defaultFont : _dollarFont;

            _totalText.text = _displayBuilder.ToString();
        }
    }

    internal static void Tick(float deltaTime)
    {
        _inventorySyncTimer -= deltaTime;

        if (_inventorySyncTimer <= 0f)
        {
            _inventorySyncTimer = InventorySyncInterval;
            SyncFromLocalInventory();
        }

        if (!_erasingDelta)
        {
            if (_deltaTimer > 0f)
            {
                _deltaTimer -= deltaTime;
                if (_deltaTimer <= 0f && _deltaTextBuilder.Length > 0)
                {
                    _erasingDelta = true;
                    _eraseTimer = _eraseSpeed;
                }
            }
        }
        else
        {
            _eraseTimer -= deltaTime;
            if (_eraseTimer <= 0f && _deltaPlainBuilder.Length > 0)
            {
                _deltaPlainBuilder.Length--;

                _deltaTextBuilder.Clear();

                _deltaTextBuilder.Append("<color=").Append(_deltaColor).Append('>')
                    .Append(_deltaPlainBuilder)
                    .Append("</color>");

                _eraseTimer = _eraseSpeed;
                UpdateInventoryTotal();
            }

            if (_deltaPlainBuilder.Length == 0)
            {
                _erasingDelta = false;
                UpdateInventoryTotal();
            }
        }

        if (slotTexts != null)
        {
            for (int i = 0; i < slotTexts.Length; i++)
                slotTexts[i]?.transform.rotation = Quaternion.identity;
        }

        if (_totalText != null)
            _totalText.transform.rotation = Quaternion.identity;
    }

    internal static void ResetForNewHUD()
    {
        if (slotTexts != null)
        {
            for (int i = 0; i < slotTexts.Length; i++)
            {
                if (slotTexts[i] != null)
                    Object.Destroy(slotTexts[i].gameObject);
            }
        }

        slotTexts = null;
        _slotValues = null;

        if (_totalText != null)
        {
            Object.Destroy(_totalText.gameObject);
            _totalText = null;
        }

        _lastTotal = 0;
        _inventorySyncTimer = 0f;
    }
}