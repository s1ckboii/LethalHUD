using HarmonyLib;
using LethalHUD.HUD;
using LethalHUD.Configs;


namespace LethalHUD.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class StartOfRoundPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.Start))]
        public static void OnStartOfRoundStart(StartOfRound __instance)
        {
            if (__instance.gameObject.GetComponent<ChatController>() == null)
            {
                __instance.gameObject.AddComponent<ChatController>();
            }

            ChatController.RefreshPlayerCache();

            if (Plugins.ConfigEntries.NameColors.Value)
            {
                ChatController.Instance?.SendColorToServer(Plugins.ConfigEntries.LocalNameColor.Value);
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.OnClientConnect))]
        private static void Postfix_OnClientConnect()
        {
            ChatController.RefreshPlayerCache();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.OnClientDisconnect))]
        private static void Postfix_OnClientDisconnect()
        {
            ChatController.RefreshPlayerCache();
        }
    }
}
