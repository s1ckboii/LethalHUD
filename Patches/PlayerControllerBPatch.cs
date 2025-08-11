using GameNetcodeStuff;
using HarmonyLib;
using LethalHUD.HUD;

namespace LethalHUD.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
internal static class PlayerControllerBPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControllerB.Awake))]
    public static void OnPlayerControllerBAwake()
    {
        // ._.
    }
    [HarmonyPostfix]
    [HarmonyPatch (nameof(PlayerControllerB.GrabObject))]
    public static void OnPlayerControllerBGrabObject()
    {
        InventoryFrames.HandsFull();
    }
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControllerB.DamagePlayer))]
    public static void OnPlayerControllerBDamagePlayer()
    {

    }
    [HarmonyPostfix]
    [HarmonyPatch (nameof(PlayerControllerB.LateUpdate))]
    public static void OnPlayerIsTyping(PlayerControllerB __instance)
    {
        if (__instance.isTypingChat)
            ChatController.PlayerTypingIndicator();
        SprintMeter.UpdateSprintMeterColor();
    }
}