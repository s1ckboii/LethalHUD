using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.HUD;
internal static class PlanetInfoDisplay
{
    private static TMP_Text _hazardTMP;
    private static Image[] _targetImages;

    private static readonly Color fallbackColor = new(0.525f, 0.925f, 1f);
    private static Color _headerColor = fallbackColor;
    private static Color _summaryColor = fallbackColor;
    private static string _lastHeaderHex;
    private static string _lastSummaryHex;

    private static bool _initialized;
    private static bool _reportedMissingLayout;
    private static float _nextInitAttemptTime;
    private static string _activeLayout = "Unknown";

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
        EnsureInitialized(true);
    }

    private static bool EnsureInitialized(bool force = false)
    {
        if (_initialized && HasAnyLiveTarget())
            return true;

        if (_initialized)
        {
            _initialized = false;
            _hazardTMP = null;
            _targetImages = null;
        }

        if (!force && Time.unscaledTime < _nextInitAttemptTime)
            return false;

        _nextInitAttemptTime = Time.unscaledTime + 1f;
        _targetImages = new Image[4];
        _hazardTMP = null;

        foreach (string[] layout in _possibleLayouts)
        {
            bool anyFound = false;

            GameObject hazardObj = GameObject.Find(layout[0]);
            if (hazardObj != null)
            {
                _hazardTMP = hazardObj.GetComponent<TMP_Text>();
                anyFound = _hazardTMP != null;
            }

            for (int i = 0; i < _targetImages.Length; i++)
            {
                GameObject obj = GameObject.Find(layout[i + 1]);
                if (obj == null)
                    continue;

                _targetImages[i] = obj.GetComponent<Image>();
                anyFound |= _targetImages[i] != null;
            }

            if (!anyFound)
                continue;

            _activeLayout = layout[0].Contains("TopLeftCorner") ? "Original" : "Modified";
            _initialized = true;
            _reportedMissingLayout = false;
            return true;
        }

        if (!_reportedMissingLayout)
        {
            _reportedMissingLayout = true;
        }

        return false;
    }

    private static bool HasAnyLiveTarget()
    {
        if (_hazardTMP != null)
            return true;

        if (_targetImages == null)
            return false;

        for (int i = 0; i < _targetImages.Length; i++)
        {
            if (_targetImages[i] != null)
                return true;
        }

        return false;
    }

    private static void RefreshConfiguredColors()
    {
        string headerHex = Plugins.ConfigEntries.PlanetHeaderColor.Value;
        string summaryHex = Plugins.ConfigEntries.PlanetSummaryColor.Value;

        if (_lastHeaderHex != headerHex)
        {
            _lastHeaderHex = headerHex;
            _headerColor = HUDUtils.ParseHexColor(headerHex, fallbackColor);
        }

        if (_lastSummaryHex != summaryHex)
        {
            _lastSummaryHex = summaryHex;
            _summaryColor = HUDUtils.ParseHexColor(summaryHex, fallbackColor);
        }
    }

    internal static void HeaderAndFooterAndHazardLevel()
    {
        if (!EnsureInitialized())
            return;

        RefreshConfiguredColors();

        if (_hazardTMP != null && _hazardTMP.color != _summaryColor)
            _hazardTMP.color = _summaryColor;

        if (_targetImages == null)
            return;

        for (int i = 0; i < _targetImages.Length; i++)
        {
            Image img = _targetImages[i];
            if (img != null && img.color != _headerColor)
                img.color = _headerColor;
        }
    }

    internal static void ApplyColors()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null)
            return;

        RefreshConfiguredColors();

        if (hud.planetInfoHeaderText != null && hud.planetInfoHeaderText.color != _headerColor)
            hud.planetInfoHeaderText.color = _headerColor;

        if (hud.planetInfoSummaryText != null && hud.planetInfoSummaryText.color != _summaryColor)
            hud.planetInfoSummaryText.color = _summaryColor;

        HeaderAndFooterAndHazardLevel();
    }

    internal static void UpdateColors()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud?.planetRiskLevelText == null)
            return;

        Color target = Plugins.ConfigEntries.PlanetRisk.Value
            ? GetRiskLevelColor(hud.planetRiskLevelText.text)
            : fallbackColor;

        if (hud.planetRiskLevelText.color != target)
            hud.planetRiskLevelText.color = target;
    }

    private static Color GetRiskLevelColor(string riskLetter)
    {
        string currentRisk = StartOfRound.Instance?.currentLevel?.riskLevel;
        if (!string.IsNullOrEmpty(currentRisk))
            riskLetter = currentRisk;

        if (string.IsNullOrEmpty(riskLetter))
            return fallbackColor;

        if (riskLetter.Equals("Safe", StringComparison.OrdinalIgnoreCase))
            return Color.green;

        if (riskLetter.StartsWith("S"))
        {
            int sCount = 1;
            while (sCount < riskLetter.Length && riskLetter[sCount] == 'S')
                sCount++;

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
            _ => _headerColor,
        };
    }

    internal static void ResetCache()
    {
        _hazardTMP = null;
        _targetImages = null;
        _initialized = false;
        _reportedMissingLayout = false;
        _nextInitAttemptTime = 0f;
        _activeLayout = "Unknown";
        _lastHeaderHex = null;
        _lastSummaryHex = null;
        _headerColor = fallbackColor;
        _summaryColor = fallbackColor;
    }
}