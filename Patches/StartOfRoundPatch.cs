using HarmonyLib;


namespace LethalHUD.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class StartOfRoundPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.Start))]
        public static void OnStartOfRoundStart(StartOfRound __instance)
        {
            // .-.
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.OnClientConnect))]
        private static void Postfix_OnClientConnect()
        {
            // ChatController.RefreshPlayerCache();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.OnClientDisconnect))]
        private static void Postfix_OnClientDisconnect()
        {
            // ChatController.RefreshPlayerCache();
        }
    }
}
