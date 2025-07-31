using HarmonyLib;
using LethalHUD.Scan;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace LethalHUD.Patches;

[HarmonyPatch(typeof(HUDManager))]
internal static class HUDManagerPatch
{
    private static CallbackContext pingScan;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HUDManager.Start))]
    public static void OnHUDManagerStart()
    {
        ScanController.SetScanColor();
        ScanController.UpdateScanTexture();

    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HUDManager.Update))]
    public static void OnHUDManagerUpdate(HUDManager __instance)
    {
        if (Configs.Instance.HoldScan.Value && IngamePlayerSettings.Instance.playerInput.actions.FindAction("PingScan").IsPressed())
            __instance.PingScan_performed(pingScan);

        if (Configs.Instance.FadeOut.Value && HUDManager.Instance.playerPingingScan > -1f)
        {
            float fadeAlpha = ScanController.ScanProgress * Configs.Instance.Alpha.Value;
            ScanController.SetScanColorAlpha(fadeAlpha);
        }
        ScanController.RandomColoringnt();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(HUDManager.PingScan_performed))]
    public static void OnScanTriggered(CallbackContext context)
    {
        pingScan = context;
        ScanController.RandomColoring();
    }
}
