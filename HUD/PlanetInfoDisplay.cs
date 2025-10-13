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

    private static bool initialized = false;
    private static string activeLayout = "Unknown";

    private static readonly string[][] possibleLayouts =
    [
        [
            "Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/CinematicGraphics/Site/HazardLevel",
            "Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/CinematicGraphics/PlanetDescription/HeaderAndFooterLines/IntroText (2)",
            "Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/CinematicGraphics/PlanetDescription/HeaderAndFooterLines/IntroText (3)",
            "Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/CinematicGraphics/Site/HeaderAndFooterLines (1)/IntroText (2)",
            "Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/CinematicGraphics/Site/HeaderAndFooterLines (1)/IntroText (3)"
        ],
        [
            "Systems/UI/Canvas/IngamePlayerHUD/CinematicGraphics/Site/HazardLevel",
            "Systems/UI/Canvas/IngamePlayerHUD/CinematicGraphics/PlanetDescription/HeaderAndFooterLines/IntroText (2)",
            "Systems/UI/Canvas/IngamePlayerHUD/CinematicGraphics/PlanetDescription/HeaderAndFooterLines/IntroText (3)",
            "Systems/UI/Canvas/IngamePlayerHUD/CinematicGraphics/Site/HeaderAndFooterLines (1)/IntroText (2)",
            "Systems/UI/Canvas/IngamePlayerHUD/CinematicGraphics/Site/HeaderAndFooterLines (1)/IntroText (3)"
        ]
    ];

    internal static void Init()
    {
        if (initialized)
            return;

        targetImages = new Image[4];
        hazardTMP = null;

        foreach (string[] layout in possibleLayouts)
        {
            bool anyFound = false;

            GameObject hazardObj = GameObject.Find(layout[0]);
            if (hazardObj != null)
            {
                hazardTMP = hazardObj.GetComponent<TMP_Text>();
                anyFound = true;
            }

            for (int i = 0; i < targetImages.Length; i++)
            {
                GameObject obj = GameObject.Find(layout[i + 1]);
                if (obj != null)
                {
                    targetImages[i] = obj.GetComponent<Image>();
                    anyFound = true;
                }
            }

            if (anyFound)
            {
                activeLayout = layout[0].Contains("TopLeftCorner") ? "Original" : "Modified";
                Loggers.Info($"[PlanetInfoDisplay] Using {activeLayout} HUD layout.");
                initialized = true;
                break;
            }
        }

        if (!initialized)
            Loggers.Warning("[PlanetInfoDisplay] No known HUD layout found; skipping HUD coloring.");
    }

    internal static void HeaderAndFooterAndHazardLevel()
    {
        if (!initialized)
        {
            Init();
            if (!initialized) return;
        }

        headerColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.PlanetHeaderColor.Value, Color.white);
        summaryColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.PlanetSummaryColor.Value, Color.white);

        if (hazardTMP != null)
            hazardTMP.color = summaryColor;

        if (targetImages != null)
        {
            foreach (Image img in targetImages)
            {
                if (img != null)
                    img.color = headerColor;
            }
        }
    }

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