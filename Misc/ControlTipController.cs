using LethalHUD.HUD;
using TMPro;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.Misc;
internal static class ControlTipController
{
    private static readonly Color DefaultColorA = Color.white;
    private static readonly Color DefaultColorB = Color.white;

    private static TMP_Text[] lastTips;
    private static string[] lastTipTexts;
    private static string lastHexA;
    private static string lastHexB;
    private static MTColorMode lastMode;

    internal static void ApplyColor()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud?.controlTipLines == null || hud.controlTipLines.Length == 0) return;

        // Skip update if nothing changed
        if (hud.controlTipLines == lastTips && !HasConfigChanged() && !HasTextChanged(hud))
            return;

        string hexA = Plugins.ConfigEntries.MTColorGradientA.Value;
        string hexB = Plugins.ConfigEntries.MTColorGradientB.Value;

        switch (Plugins.ConfigEntries.MTColorSelection.Value)
        {
            case MTColorMode.Solid:
                ApplySingleColor(hud, hexA, DefaultColorA);
                break;

            case MTColorMode.Gradient:
                if (HUDUtils.HasCustomGradient(hexA, hexB))
                    ApplyGradient(hud, hexA, hexB, DefaultColorA, DefaultColorB);
                else
                    ApplySingleColor(hud, hexA, DefaultColorA);
                break;
        }

        CacheState(hud);
    }

    private static bool HasConfigChanged()
    {
        MTColorMode mode = Plugins.ConfigEntries.MTColorSelection.Value;
        string hexA = Plugins.ConfigEntries.MTColorGradientA.Value;
        string hexB = Plugins.ConfigEntries.MTColorGradientB.Value;

        return mode != lastMode || hexA != lastHexA || hexB != lastHexB;
    }

    private static bool HasTextChanged(HUDManager hud)
    {
        if (lastTipTexts == null || lastTipTexts.Length != hud.controlTipLines.Length)
            return true;

        for (int i = 0; i < hud.controlTipLines.Length; i++)
        {
            var tmp = hud.controlTipLines[i];
            if (tmp == null) continue;
            string text = tmp.text;
            if (lastTipTexts[i] != text)
                return true;
        }

        return false;
    }

    private static void CacheState(HUDManager hud)
    {
        lastTips = hud.controlTipLines;
        lastMode = Plugins.ConfigEntries.MTColorSelection.Value;
        lastHexA = Plugins.ConfigEntries.MTColorGradientA.Value;
        lastHexB = Plugins.ConfigEntries.MTColorGradientB.Value;

        lastTipTexts = new string[hud.controlTipLines.Length];
        for (int i = 0; i < hud.controlTipLines.Length; i++)
            lastTipTexts[i] = hud.controlTipLines[i]?.text;
    }

    private static void ApplySingleColor(HUDManager hud, string hex, Color fallback)
    {
        Color color = HUDUtils.ParseHexColor(hex, fallback);

        foreach (var tip in hud.controlTipLines)
        {
            if (tip != null)
                tip.color = color;
        }
    }

    private static void ApplyGradient(HUDManager hud, string hexA, string hexB, Color fallbackA, Color fallbackB)
    {
        Color colorA = HUDUtils.ParseHexColor(hexA, fallbackA);
        Color colorB = HUDUtils.ParseHexColor(hexB, fallbackB);

        foreach (var tmp in hud.controlTipLines)
        {
            if (tmp == null) continue;

            tmp.ForceMeshUpdate();
            TMP_TextInfo textInfo = tmp.textInfo;

            int charCount = textInfo.characterCount;
            if (charCount == 0) continue;

            for (int i = 0; i < charCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                float t = (charCount > 1) ? i / (float)(charCount - 1) : 0f;
                Color charColor = Color.Lerp(colorA, colorB, t);

                int vertexIndex = charInfo.vertexIndex;
                var meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];

                meshInfo.colors32[vertexIndex + 0] = charColor;
                meshInfo.colors32[vertexIndex + 1] = charColor;
                meshInfo.colors32[vertexIndex + 2] = charColor;
                meshInfo.colors32[vertexIndex + 3] = charColor;
            }

            for (int i = 0; i < tmp.textInfo.meshInfo.Length; i++)
            {
                tmp.textInfo.meshInfo[i].mesh.colors32 = tmp.textInfo.meshInfo[i].colors32;
                tmp.UpdateGeometry(tmp.textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}