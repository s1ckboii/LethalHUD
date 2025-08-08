using GameNetcodeStuff;
using HarmonyLib;
using LethalHUD.HUD;


namespace LethalHUD.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
internal static class PlayerControllerBPatch
{

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerControllerB.Awake))]
    public static void OnPlayerControllerBAwake(PlayerControllerB __instance)
    {
        if (!__instance.IsLocalPlayer)
            return;

        if (ChatController.Instance == null)
        {
            StartOfRound.Instance.gameObject.AddComponent<ChatController>();
        }

        if (Plugins.ConfigEntries.NameColors.Value &&
            Unity.Netcode.NetworkManager.Singleton?.IsClient == true)
        {
            ChatController.Instance?.SendColorToServer(Plugins.ConfigEntries.LocalNameColor.Value);
        }
    }
}