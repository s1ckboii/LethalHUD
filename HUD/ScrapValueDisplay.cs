using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class ScrapValueDisplay
{
    internal static TMP_Text[] slotTexts;
    private static TMP_Text totalText;

    private static TMP_FontAsset defaultFont;
    private static TMP_FontAsset dollarFont;

    private static RectTransform totalRT;

    private static int[] slotValues;

    private static int lastTotal = 0;
    private static float deltaTimer = 0f;
    private static string deltaText = "";
    private static string deltaRaw = "";
    private static string deltaPlain = "";
    private static string deltaColor = "green";
    private static bool erasingDelta = false;
    private static readonly float eraseSpeed = 0.05f;
    private static float eraseTimer = 0f;

    private static readonly Color lowColor = new(0.85f, 0.85f, 0.85f);
    private static readonly Color highColor = Color.green;

    internal static void Init()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null) return;

        defaultFont = hud.totalValueText.font;
        dollarFont = hud.chatText.font;

        SetupSlots();
    }

    internal static void SetupSlots()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null || hud.itemSlotIconFrames == null || hud.itemSlotIconFrames.Length == 0) return;

        int slotCount = hud.itemSlotIconFrames.Length;

        if (slotTexts == null || slotTexts.Length != slotCount)
            slotTexts = new TMP_Text[slotCount];
        if (slotValues == null || slotValues.Length != slotCount)
            slotValues = new int[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            Image slot = hud.itemSlotIconFrames[i];
            if (slot == null) continue;

            if (slotTexts[i] != null) continue;

            CreateSlotTextForIndex(i, slot);
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
        rt.localPosition = new Vector2(0f, -3f);
        rt.localRotation = Quaternion.identity;

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
        if (totalText != null)
            Object.Destroy(totalText.gameObject);

        if (hud.itemSlotIconFrames.Length == 0) return;

        GameObject totalGO = new("InventoryScrapTotalValueText");
        totalGO.transform.SetParent(hud.itemSlotIconFrames[0].transform.parent, false);

        totalRT = totalGO.AddComponent<RectTransform>();
        totalRT.localScale = Vector3.one * 0.5f;
        totalRT.localRotation = Quaternion.identity;
        totalRT.anchorMin = totalRT.anchorMax = new Vector2(0f, 0.5f);
        totalRT.pivot = new Vector2(1f, 0.5f);
        totalRT.localPosition = new Vector2(Plugins.ConfigEntries.TotalValueOffsetX.Value, Plugins.ConfigEntries.TotalValueOffsetY.Value);

        totalText = totalGO.AddComponent<TextMeshProUGUI>();
        totalText.fontSize = 14;
        totalText.alignment = TextAlignmentOptions.Right;
        totalText.color = Color.green;
        totalText.raycastTarget = false;
        totalText.text = "";
    }

    internal static void RefreshSlots()
    {
        SetupSlots();
    }

    internal static void UpdateSlot(int slotIndex, int value)
    {
        if (!Plugins.ConfigEntries.ShowItemValue.Value) return;
        if (slotTexts == null || slotIndex < 0 || slotIndex >= slotTexts.Length) return;

        TMP_Text tmp = slotTexts[slotIndex];
        if (tmp == null) return;

        tmp.font = Plugins.ConfigEntries.SetDollar.Value == Enums.ItemValue.Default
            ? defaultFont : dollarFont;

        tmp.text = value > 0 ? $"${value}" : "";
        slotValues[slotIndex] = value;

        UpdateInventoryTotal();
        UpdateSlotValueTextColors();
    }

    internal static void UpdateTotalTextPosition()
    {
        if (totalText == null) return;

        RectTransform totalRT = totalText.rectTransform;
        totalRT.localPosition = new Vector2(
            Plugins.ConfigEntries.TotalValueOffsetX.Value,
            Plugins.ConfigEntries.TotalValueOffsetY.Value
        );
    }

    internal static void Hide(int slotIndex)
    {
        if (slotTexts == null || slotIndex < 0 || slotIndex >= slotTexts.Length) return;

        TMP_Text tmp = slotTexts[slotIndex];
        if (tmp != null) tmp.text = "";

        slotValues[slotIndex] = 0;
        UpdateInventoryTotal();
        UpdateSlotValueTextColors();
    }

    internal static void ClearItemSlots()
    {
        if (slotTexts == null || slotValues == null) return;

        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (slotTexts[i] != null)
                slotTexts[i].text = "";

            slotValues[i] = 0;
        }

        UpdateInventoryTotal();
        UpdateSlotValueTextColors();
    }

    private static void UpdateSlotValueTextColors()
    {
        int min = int.MaxValue;
        int max = int.MinValue;
        bool hasValues = false;

        for (int i = 0; i < slotValues.Length; i++)
        {
            int v = slotValues[i];
            if (v > 0)
            {
                hasValues = true;
                if (v < min) min = v;
                if (v > max) max = v;
            }
        }

        if (!hasValues) return;

        for (int i = 0; i < slotValues.Length; i++)
        {
            TMP_Text tmp = slotTexts[i];
            if (tmp == null) continue;

            int v = slotValues[i];
            tmp.color = v <= 0 ? lowColor : Color.Lerp(lowColor, highColor, (max == min) ? 1f : Mathf.InverseLerp(min, max, v));
        }
    }

    private static void UpdateInventoryTotal()
    {
        if (!Plugins.ConfigEntries.ShowTotalInventoryValue.Value)
        {
            if (totalText != null && totalText.text != string.Empty)
                totalText.text = string.Empty;
            return;
        }

        int total = 0;
        for (int i = 0; i < slotValues.Length; i++)
            total += slotValues[i];

        if (total != lastTotal)
        {
            int diff = total - lastTotal;
            if (diff != 0 && Plugins.ConfigEntries.ShowTotalDelta.Value)
            {
                string color = diff > 0 ? "green" : "red";
                string sign = diff > 0 ? "+" : "-";

                deltaColor = color;
                deltaRaw = $"<color={color}>({sign}${Mathf.Abs(diff)})</color>";
                deltaPlain = $"({sign}${Mathf.Abs(diff)})";

                deltaText = deltaRaw;
                deltaTimer = 1.5f;
                erasingDelta = false;
            }
            lastTotal = total;
        }

        string prefix = Plugins.ConfigEntries.TotalPrefix.Value switch
        {
            TotalValuePrefix.Full => "Total Value: ",
            TotalValuePrefix.Short => "Total: ",
            TotalValuePrefix.None => "",
            _ => "Total Value: "
        };

        string display = total > 0 ? $"{prefix}${total}" : "";
        if (!string.IsNullOrEmpty(deltaText))
            display += deltaText;

        if (totalText != null)
        {
            totalText.font = Plugins.ConfigEntries.SetDollar.Value == ItemValue.Default
                ? defaultFont : dollarFont;
            totalText.text = display;
        }
    }

    internal static void Tick(float deltaTime)
    {
        if (!erasingDelta)
        {
            if (deltaTimer > 0f)
            {
                deltaTimer -= deltaTime;
                if (deltaTimer <= 0f && !string.IsNullOrEmpty(deltaText))
                {
                    erasingDelta = true;
                    eraseTimer = eraseSpeed;
                }
            }
        }
        else
        {
            eraseTimer -= deltaTime;
            if (eraseTimer <= 0f && deltaPlain.Length > 0)
            {
                deltaPlain = deltaPlain[..^1];
                deltaText = deltaPlain.Length > 0 ? $"<color={deltaColor}>{deltaPlain}</color>" : "";
                eraseTimer = eraseSpeed;
                UpdateInventoryTotal();
            }

            if (deltaPlain.Length == 0)
            {
                erasingDelta = false;
                UpdateInventoryTotal();
            }
        }

        if (slotTexts != null)
        {
            for (int i = 0; i < slotTexts.Length; i++)
                if (slotTexts[i] != null)
                    slotTexts[i].transform.rotation = Quaternion.identity;
        }

        if (totalText != null)
            totalText.transform.rotation = Quaternion.identity;
    }
}