using GameNetcodeStuff;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class SprintMeterController
{

    internal static void UpdateSprintMeterColor()
    {
        if (!Plugins.ConfigEntries.SprintBool.Value)
            return;

        if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
            return;

        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (player?.sprintMeterUI == null)
            return;

        switch (Plugins.ConfigEntries.SprintColoring.Value)
        {
            case SprintStyle.Gradient:
                ApplyGradientMode(player);
                break;
            case SprintStyle.Shades:
                ApplyShadesMode(player);
                break;
            default:
                ApplySolidMode(player);
                break;
        }
    }

    private static void ApplySolidMode(PlayerControllerB player)
    {
        Color solidColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SprintMeterColor.Value);

        player.sprintMeterUI.color = solidColor;
    }

    private static void ApplyGradientMode(PlayerControllerB player)
    {
        Color baseColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SprintMeterColor.Value);

        float fillAmount = player.sprintMeterUI.fillAmount;
        Color finalColor = HUDUtils.GetGradientColor(baseColor, fillAmount);

        player.sprintMeterUI.color = finalColor;
    }

    private static void ApplyShadesMode(PlayerControllerB player)
    {
        Color baseColor = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SprintMeterColor.Value);

        float fillAmount = player.sprintMeterUI.fillAmount;
        Color finalColor = HUDUtils.GetShadeColor(baseColor, fillAmount);

        player.sprintMeterUI.color = finalColor;
    }
}