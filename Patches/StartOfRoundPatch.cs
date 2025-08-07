using HarmonyLib;
using LethalHUD.HUD;


namespace LethalHUD.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class StartOfRoundPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.Start))]
        public static void OnStartOfRoundStart(StartOfRound __instance)
        {
            if (__instance.GetComponent<ChatController>() == null)
                __instance.gameObject.AddComponent<ChatController>();
        }
    }
}
