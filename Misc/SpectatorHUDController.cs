using LethalHUD.Configs;
using LethalHUD.HUD;
using UnityEngine;

namespace LethalHUD.Misc;
public static class SpectatorHUDController
{
    public static void ApplyColors()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null) return;


        //Testing purposes only
        ConfigEntries cfg = Plugins.ConfigEntries;

        ApplyColorPreserveAlpha(hud.spectatorTipText, cfg.SpectatorTipColor.Value);
        ApplyColorPreserveAlpha(hud.spectatingPlayerText, cfg.SpectatingPlayerColor.Value);
        ApplyColorPreserveAlpha(hud.holdButtonToEndGameEarlyText, cfg.HoldEndGameColor.Value);
        ApplyColorPreserveAlpha(hud.holdButtonToEndGameEarlyVotesText, cfg.HoldEndGameVotesColor.Value);
    }

    private static void ApplyColorPreserveAlpha(TMPro.TextMeshProUGUI text, string hexColor)
    {
        if (text == null) return;

        Color original = text.color;
        Color parsed = HUDUtils.ParseHexColor(hexColor, original);
        parsed.a = original.a;
        text.color = parsed;
    }
}