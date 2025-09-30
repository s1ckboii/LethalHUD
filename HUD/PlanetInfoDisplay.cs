using System;
using UnityEngine;

namespace LethalHUD.HUD;
internal static class PlanetInfoDisplay
{
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


    private static Color GetRiskLevelColor(string level)
    {
        if (string.IsNullOrEmpty(level)) return Color.white;

        level = StartOfRound.Instance.currentLevel.riskLevel;

        if (Plugins.ConfigEntries.HalloweenMode.Value)
        {
            if (level.StartsWith("S")) return new Color(0.6f, 0f, 0.3f);
            return level[0] switch
            {
                'A' => new Color(1f, 0.5f, 0f),
                'B' => new Color(1f, 0.65f, 0f),
                'C' => new Color(0.8f, 0.2f, 0.8f),
                'D' => new Color(0.7f, 0.5f, 0f),
                'F' => Color.gray,
                _ => Color.magenta
            };
        }

        if (level.Equals("Safe", StringComparison.OrdinalIgnoreCase))
            return Color.green;

        if (level.StartsWith("S"))
        {
            int sCount = 1;
            while (sCount < level.Length && level[sCount] == 'S') sCount++;
            float t = Mathf.Clamp01((sCount - 1) / 4f);
            return Color.Lerp(Color.red, new Color(0.5f, 0f, 0f), t);
        }

        return level[0] switch
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