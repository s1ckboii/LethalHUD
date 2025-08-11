using HarmonyLib;
using LethalHUD.HUD;
using LethalHUD.Scan;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace LethalHUD.Patches;
[HarmonyPatch(typeof(HUDManager))]
internal static class HUDManagerPatch
{
    private static CallbackContext pingScan;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(HUDManager.PingScan_performed))]
    public static void OnScanTriggered(CallbackContext context)
    {
        pingScan = context;
    }

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
        if (__instance.gameObject.GetComponent<LethalHUDMono>() == null)
        {
            __instance.gameObject.AddComponent<LethalHUDMono>();
        }
        ChatController.ColorChatInputField(__instance.chatTextField,Time.time * 0.25f);
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
        if (Plugins.ConfigEntries.HoldScan.Value && IngamePlayerSettings.Instance.playerInput.actions.FindAction("PingScan").IsPressed())
            __instance.PingScan_performed(pingScan);

        ScanController.UpdateScanAlpha();
    }

    [HarmonyTranspiler]
    [HarmonyPatch(nameof(HUDManager.AddChatMessage))]
    public static IEnumerable<CodeInstruction> AddChatMessage_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var getColoredNameMethod = typeof(ChatController).GetMethod(nameof(ChatController.GetColoredPlayerName));
        var getDefaultColorTagMethod = typeof(ChatController).GetMethod(nameof(ChatController.GetDefaultChatColorTag));
        var matcher = new CodeMatcher(instructions);

        // Patch the hardcoded red chat name color
        while (true)
        {
            var foundMatcher = matcher.MatchForward(false, new CodeMatch(OpCodes.Ldarg_2));
            if (foundMatcher == null || !foundMatcher.IsValid)
                break;

            foundMatcher.Advance(1);
            foundMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, getColoredNameMethod));

            matcher = foundMatcher;
        }
        matcher = new CodeMatcher(matcher.InstructionEnumeration());

        // Patch the hardcoded blue chat color <color=#7069ff>
        while (true)
        {
            var foundMatcher = matcher.MatchForward(false, new CodeMatch(OpCodes.Ldstr, "<color=#7069ff>"));
            if (foundMatcher == null || !foundMatcher.IsValid)
                break;

            foundMatcher.SetInstruction(new CodeInstruction(OpCodes.Call, getDefaultColorTagMethod));

            matcher = foundMatcher;
        }

        return matcher.InstructionEnumeration();
    }
}
