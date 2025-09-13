using LethalHUD.HUD;
using MonoDetour;
using MonoDetour.HookGen;

namespace LethalHUD.Patches;

[MonoDetourTargets(typeof(GameNetworkManager), Members = ["NoPunctuation"])]
internal static class GameNetworkManagerPatch
{
    [MonoDetourHookInitialize]
    public static void Init()
    {
        On.GameNetworkManager.NoPunctuation.Postfix(OnGameNetworkManagerNoPunctation);
    }

    private static void OnGameNetworkManagerNoPunctation(GameNetworkManager self, ref string input, ref string returnValue)
    {
        returnValue = ChatController.NoPunctuation(input);

        if (string.IsNullOrEmpty(input))
            returnValue = "Nameless";
    }
}