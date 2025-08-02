using GameNetcodeStuff;
using HarmonyLib;

[HarmonyPatch(typeof(PlayerControllerB))]
internal static class PlayerControllerBPatch
{

    [HarmonyPostfix]
    [HarmonyPatch("LateUpdate")]
    public static void Postfix(PlayerControllerB __instance)
    {

    }
}