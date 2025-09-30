using LethalHUD.HUD;
using HarmonyLib;
namespace LethalHUD.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
internal static class GameNetworkManagerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("NoPunctuation")]
    private static bool NoPunctuation_Prefix(string input, ref string __result)
    {
        if (string.IsNullOrEmpty(input))
        {
            __result = "Nameless";
        }
        else
        {
            __result = ChatController.NoPunctuation(input);
        }

        return false;
    }
}