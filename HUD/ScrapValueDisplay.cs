using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class ScrapValueDisplay
{
    internal static TMP_Text[] slotTexts;

    internal static void Init()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null) return;

        int slotCount = hud.itemSlotIconFrames.Length;
        slotTexts = new TMP_Text[slotCount];

        for (int i = 0; i < slotCount; i++)
        {
            Image slot = hud.itemSlotIconFrames[i];
            if (slot == null) continue;

            GameObject go = new("ScrapValueText");
            go.transform.SetParent(slot.transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.localRotation = Quaternion.Euler(0, 0, 90);
            rt.localScale = Vector3.one * 0.5f;

            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            SetFont(tmp);
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.green;
            tmp.raycastTarget = false;
            tmp.text = "";

            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, -27f);

            slotTexts[i] = tmp;
        }
    }

    internal static void UpdateSlot(int slotIndex, int value)
    {
        if (!Plugins.ConfigEntries.ShowItemValue.Value) { return; }
        if (slotTexts == null || slotIndex < 0 || slotIndex >= slotTexts.Length) return;

        TMP_Text tmp = slotTexts[slotIndex];
        SetFont(tmp);
        if (tmp == null) return;

        tmp.text = value > 0 ? $"${value}" : "";
    }

    private static void SetFont(TMP_Text tmp)
    {
        HUDManager hud = HUDManager.Instance;
        switch (Plugins.ConfigEntries.SetDollar.Value)
        {
            case ItemValue.Default:
                tmp.font = hud.totalValueText.font;
                break;
            case ItemValue.Dollar:
                tmp.font = hud.chatText.font;
                break;
        }
    }
    internal static void Hide(int slotIndex)
    {
        TMP_Text tmp = slotTexts[slotIndex];
        tmp.text = "";
    }
}