using GameNetcodeStuff;
using UnityEngine;

namespace LethalHUD.HUD;
internal static class SprintMeter
{
    internal const string PlayerPrefsKey = "SprintMeterLastMode";
    internal static void UpdateSprintMeterColor()
    {
        var player = StartOfRound.Instance?.localPlayerController;
        if (player?.sprintMeterUI == null)
            return;

        string lastMode = PlayerPrefs.GetString(PlayerPrefsKey, "Solid");

        switch (lastMode)
        {
            case "Solid":
                ApplySolidMode(player);
                break;
            case "Shades":
                ApplyShadesMode(player);
                break;
            default:
                ApplyGradientMode(player);
                break;
        }
    }

    private static void ApplyGradientMode(PlayerControllerB player)
    {
        Color baseColor = HUDUtils.ParseHexColor(
            Plugins.ConfigEntries.SprintMeterColorGradient.Value
        );

        float fillAmount = player.sprintMeterUI.fillAmount;
        Color finalColor = HUDUtils.GetGradientColor(baseColor, fillAmount);

        player.sprintMeterUI.color = finalColor;
    }

    private static void ApplySolidMode(PlayerControllerB player)
    {
        Color solidColor = HUDUtils.ParseHexColor(
            Plugins.ConfigEntries.SprintMeterColorSolid.Value
        );

        player.sprintMeterUI.color = solidColor;
    }

    private static void ApplyShadesMode(PlayerControllerB player)
    {
        Color baseColor = HUDUtils.ParseHexColor(
            Plugins.ConfigEntries.SprintMeterColorShades.Value
        );

        float fillAmount = player.sprintMeterUI.fillAmount;
        Color finalColor = HUDUtils.GetShadeColor(baseColor, fillAmount);

        player.sprintMeterUI.color = finalColor;
    }
}