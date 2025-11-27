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

    private static readonly Color _lowColor = new(0.85f, 0.85f, 0.85f);
    private static readonly Color _highColor = Color.green;

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
            if (slotTexts != null)
            {
                for (int i = 0; i < slotTexts.Length; i++)
                {
                    if (slotTexts[i] != null)
                        Object.Destroy(slotTexts[i].gameObject);
                }
            }

            slotTexts = new TMP_Text[slotCount];
            _slotValues = new int[slotCount];
        }

        for (int i = 0; i < slotCount; i++)
        {
            Image slot = hud.itemSlotIconFrames[i];
            if (slot == null)
                continue;

            bool mustCreate = false;

            if (slotTexts[i] == null)
                mustCreate = true;

            else if (slotTexts[i].gameObject == null)
                mustCreate = true;

            else if (slotTexts[i].transform.parent != slot.transform)
                mustCreate = true;

            if (mustCreate)
            {
                foreach (Transform child in slot.transform)
                {
                    if (child.name == "InventoryScrapValueText")
                        Object.Destroy(child.gameObject);
                }

                CreateSlotTextForIndex(i, slot);
            }
        }

        SetupTotalText(hud);
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
        tmp.color = Color.green;
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
        if (slotTexts == null || slotIndex < 0 || slotIndex >= slotTexts.Length) return;

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

    internal static void UpdateTotalTextPosition()
    {
        if (_totalText == null) return;
        RectTransform totalRT = _totalText.rectTransform;
        totalRT.localPosition = new Vector2(Plugins.ConfigEntries.TotalValueOffsetX.Value, Plugins.ConfigEntries.TotalValueOffsetY.Value);
    }

    internal static void Hide(int slotIndex)
    {
        if (slotTexts == null || slotIndex < 0 || slotIndex >= slotTexts.Length) return;

        TMP_Text tmp = slotTexts[slotIndex];
        if (tmp != null) tmp.text = "";

        _slotValues[slotIndex] = 0;
        UpdateInventoryTotal();
        UpdateSlotValueTextColors();
    }

    internal static void ClearItemSlots()
    {
        if (slotTexts == null || _slotValues == null) return;

        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (slotTexts[i] != null)
                slotTexts[i].text = "";
            _slotValues[i] = 0;
        }

        UpdateInventoryTotal();
        UpdateSlotValueTextColors();
    }

    private static void UpdateSlotValueTextColors()
    {
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

        for (int i = 0; i < _slotValues.Length; i++)
        {
            TMP_Text tmp = slotTexts[i];
            if (tmp == null) continue;

            int v = _slotValues[i];
            tmp.color = v <= 0 ? _lowColor : Color.Lerp(_lowColor, _highColor, (max == min) ? 1f : Mathf.InverseLerp(min, max, v));
        }
    }

    private static void UpdateInventoryTotal()
    {
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
                if (slotTexts[i] != null)
                    slotTexts[i].transform.rotation = Quaternion.identity;
        }

        if (_totalText != null)
            _totalText.transform.rotation = Quaternion.identity;
    }
}