using HarmonyLib;
using LethalHUD.Configs;
using LethalHUD.HUD;
using LethalHUD.Scan;
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
    [HarmonyPatch(nameof(HUDManager.OnEnable))]
    public static void OnHUDManagerEnable(HUDManager __instance)
    {
        if (__instance.gameObject.GetComponent<InventoryUtils>() == null)
        {
            __instance.gameObject.AddComponent<InventoryUtils>();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HUDManager.DisplayNewScrapFound))]
    public static void OnHUDManagerDisplayNewScrapFound()
    {
        InventoryFrames.SetSlotColors();
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(HUDManager.Update))]
    public static void OnHUDManagerUpdate(HUDManager __instance)
    {
        if (ConfigEntries.Instance.HoldScan.Value && IngamePlayerSettings.Instance.playerInput.actions.FindAction("PingScan").IsPressed())
            __instance.PingScan_performed(pingScan);

        ScanController.UpdateScanAlpha();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(HUDManager.PingScan_performed))]
    public static void OnScanTriggered(CallbackContext context)
    {
        pingScan = context;
    }
}
