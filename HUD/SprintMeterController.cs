using GameNetcodeStuff;
using LethalHUD.CustomHUD;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class SprintMeterController
{
    private static Color BaseColor => HUDUtils.ParseHexColor(Plugins.ConfigEntries.SprintMeterColor.Value);
    internal static void UpdateSprintMeterColor()
    {
        if (!Plugins.ConfigEntries.SprintBool.Value)
            return;

        if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
            return;

        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (player?.sprintMeterUI == null)
            return;

        float fillAmount = player.sprintMeterUI.fillAmount;
        Color finalColor;

        switch (Plugins.ConfigEntries.SprintColoring.Value)
        {
            case SprintStyle.Gradient:
                finalColor = HUDUtils.GetGradientColor(BaseColor, fillAmount);
                break;
            case SprintStyle.Shades:
                finalColor = HUDUtils.GetShadeColor(BaseColor, fillAmount);
                break;
            default:
                finalColor = BaseColor;
                break;
        }

        if (CustomStaminaMeter.UsingCustom && CustomStaminaMeter.Refs != null)
        {
            CustomStaminaMeter.Refs.SetBarColor(finalColor);
            CustomStaminaMeter.Refs.UpdateStaminaUI(fillAmount, finalColor);
        }
        else
        {
            player.sprintMeterUI.color = finalColor;
        }
    }
}