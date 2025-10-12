/*
using HarmonyLib;
using LethalHUD.HUD;
using Unity.Netcode;

namespace LethalHUD.Patches;
[HarmonyPatch(typeof(StartOfRound))]
internal static class StartOfRoundPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    private static void OnStartOfRoundAwake(StartOfRound __instance)
    {
        if (__instance.gameObject.GetComponent<ChatNetworkManager>() == null)
        {
            __instance.gameObject.AddComponent<ChatNetworkManager>();
        }

        ChatController.ApplyLocalPlayerColor( Plugins.ConfigEntries.GradientNameColorA.Value, Plugins.ConfigEntries.GradientNameColorB.Value);
    }

    [HarmonyPostfix]
    [HarmonyPatch("OnPlayerConnectedClientRpc")]
    private static void OnStarOfRoundOnPlayerConnectedClientRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            ChatController.ApplyLocalPlayerColor(Plugins.ConfigEntries.GradientNameColorA.Value, Plugins.ConfigEntries.GradientNameColorB.Value);
        }
    }
}
*/