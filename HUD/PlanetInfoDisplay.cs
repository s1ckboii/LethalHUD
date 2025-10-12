using HarmonyLib;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.HUD;
internal static class PlanetInfoDisplay
{
    private static TMP_Text hazardTMP;
    private static Image[] targetImages;

    private static Color headerColor;
    private static Color summaryColor;
    internal static void ApplyColors()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null) return;


        if (hud.planetInfoHeaderText != null)
            hud.planetInfoHeaderText.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.PlanetHeaderColor.Value, Color.white);

        if (hud.planetInfoSummaryText != null)
            hud.planetInfoSummaryText.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.PlanetSummaryColor.Value, Color.white);
    }

    internal static void UpdateColors()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null) return;

        if (hud.planetRiskLevelText != null)
        {
            if (Plugins.ConfigEntries.PlanetRisk.Value)
                hud.planetRiskLevelText.color = GetRiskLevelColor(hud.planetRiskLevelText.text);
            else
                hud.planetRiskLevelText.color = Color.white;
        }
    }
    internal static void Init()
    {
        GameObject hazardLevel = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/CinematicGraphics/Site/HazardLevel");
        if (hazardLevel != null)
            hazardTMP = hazardLevel.GetComponent<TMP_Text>();

        targetImages =
        [
                GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/CinematicGraphics/PlanetDescription/HeaderAndFooterLines/IntroText (2)")?.GetComponent<Image>(),
                GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/CinematicGraphics/PlanetDescription/HeaderAndFooterLines/IntroText (3)")?.GetComponent<Image>(),
                GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/CinematicGraphics/Site/HeaderAndFooterLines (1)/IntroText (2)")?.GetComponent<Image>(),
                GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/CinematicGraphics/Site/HeaderAndFooterLines (1)/IntroText (3)")?.GetComponent<Image>()
        ];
    }
    internal static void HeaderAndFooterAndHazardLevel()
    {
        headerColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.PlanetHeaderColor.Value, Color.white);
        summaryColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.PlanetSummaryColor.Value, Color.white);

        if (hazardTMP != null)
            hazardTMP.color = summaryColor;

        if (targetImages != null)
        {
            foreach (Image img in targetImages)
                img.color = headerColor;
        }
    }

    private static Color GetRiskLevelColor(string riskLetter)
    {
        if (string.IsNullOrEmpty(riskLetter)) return Color.white;

        riskLetter = StartOfRound.Instance.currentLevel.riskLevel;
        if (Plugins.ConfigEntries.HalloweenMode.Value)
        {
            if (riskLetter.StartsWith("S")) return new Color(0.6f, 0f, 0.3f);
            return riskLetter[0] switch
            {
                'A' => new Color(1f, 0.5f, 0f),
                'B' => new Color(1f, 0.65f, 0f),
                'C' => new Color(0.8f, 0.2f, 0.8f),
                'D' => new Color(0.7f, 0.5f, 0f),
                'F' => Color.gray,
                _ => Color.magenta
            };
        }

        if (riskLetter.Equals("Safe", StringComparison.OrdinalIgnoreCase))
            return Color.green;

        if (riskLetter.StartsWith("S"))
        {
            int sCount = 1;
            while (sCount < riskLetter.Length && riskLetter[sCount] == 'S') sCount++;
            float t = Mathf.Clamp01((sCount - 1) / 4f);
            return Color.Lerp(Color.red, new Color(0.5f, 0f, 0f), t);
        }

        return riskLetter[0] switch
        {
            'A' => new Color(1f, 0.5f, 0f),
            'B' => new Color(1f, 0.65f, 0f),
            'C' => Color.yellow,
            'D' => new Color(0.5f, 1f, 0f),
            'F' => Color.gray,
            _ => Color.white,
        };
    }
}