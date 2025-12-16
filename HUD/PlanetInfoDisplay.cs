using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.HUD;
internal static class PlanetInfoDisplay
{
    private static TMP_Text _hazardTMP;
    private static Image[] _targetImages;

    private static readonly Color fallbackColor = new(134f, 236f, 355f);
    private static Color _headerColor;
    private static Color _summaryColor;

    private static bool _initialized = false;
    private static string _activeLayout = "Unknown";

    // Change this later

    private static readonly string[][] _possibleLayouts =
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
        if (_initialized)
            return;

        _targetImages = new Image[4];
        _hazardTMP = null;

        foreach (string[] layout in _possibleLayouts)
        {
            bool anyFound = false;

            GameObject hazardObj = GameObject.Find(layout[0]);
            if (hazardObj != null)
            {
                _hazardTMP = hazardObj.GetComponent<TMP_Text>();
                anyFound = true;
            }

            for (int i = 0; i < _targetImages.Length; i++)
            {
                GameObject obj = GameObject.Find(layout[i + 1]);
                if (obj != null)
                {
                    _targetImages[i] = obj.GetComponent<Image>();
                    anyFound = true;
                }
            }

            if (anyFound)
            {
                _activeLayout = layout[0].Contains("TopLeftCorner") ? "Original" : "Modified";
                Loggers.Info($"[PlanetInfoDisplay] Using {_activeLayout} HUD layout.");
                _initialized = true;
                break;
            }
        }

        if (!_initialized)
            Loggers.Warning("[PlanetInfoDisplay] No known HUD layout found; skipping HUD coloring.");
    }

    internal static void HeaderAndFooterAndHazardLevel()
    {
        if (!_initialized)
        {
            Init();
            if (!_initialized) return;
        }

        _headerColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.PlanetHeaderColor.Value, fallbackColor);
        _summaryColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.PlanetSummaryColor.Value, fallbackColor);

        if (_hazardTMP != null)
            _hazardTMP.color = _summaryColor;

        if (_targetImages != null)
        {
            foreach (Image img in _targetImages)
            {
                if (img != null)
                    img.color = _headerColor;
            }
        }
    }

    internal static void ApplyColors()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null) return;

        if (hud.planetInfoHeaderText != null)
            hud.planetInfoHeaderText.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.PlanetHeaderColor.Value, fallbackColor);

        if (hud.planetInfoSummaryText != null)
            hud.planetInfoSummaryText.color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.PlanetSummaryColor.Value, fallbackColor);
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
                hud.planetRiskLevelText.color = fallbackColor;
        }
    }

    private static Color GetRiskLevelColor(string riskLetter)
    {
        if (string.IsNullOrEmpty(riskLetter)) return fallbackColor;

        riskLetter = StartOfRound.Instance.currentLevel.riskLevel;

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