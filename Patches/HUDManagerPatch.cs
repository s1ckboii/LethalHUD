using HarmonyLib;
using LethalHUD.Configs;
using LethalHUD.HUD;
using LethalHUD.Scan;
using System.Collections.Generic;
using System.Reflection.Emit;
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
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(HUDManager.AddChatMessage))]
    public static IEnumerable<CodeInstruction> AddChatMessage_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        var getColoredNameMethod = typeof(ChatController).GetMethod(nameof(ChatController.GetColoredPlayerName));

        for (int i = 0; i < codes.Count; i++)
        {
            var inst = codes[i];

            // ldarg.2 = playerName
            if (inst.opcode == OpCodes.Ldarg_2)
            {
                // Instead of just loading playerName, call GetColoredPlayerName(playerName)
                yield return new CodeInstruction(OpCodes.Ldarg_2);
                yield return new CodeInstruction(OpCodes.Call, getColoredNameMethod);

                continue;
            }

            yield return inst;
        }
    }
}