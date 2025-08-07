using GameNetcodeStuff;
using HarmonyLib;


namespace LethalHUD.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
internal static class PlayerControllerBPatch
{

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControllerB.Awake))]
    public static void OnPlayerControllerBAwake(PlayerControllerB __instance)
    {
        // ._.
    }
}