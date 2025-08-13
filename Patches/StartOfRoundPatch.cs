using MonoDetour;
using MonoDetour.HookGen;


namespace LethalHUD.Patches;
[MonoDetourTargets(typeof(StartOfRound), Members = [])]
internal static class StartOfRoundPatch
{
    [MonoDetourHookInitialize]
    public static void Init()
    {

    }
    public static void OnStartOfRoundStart(StartOfRound self)
    {
        // .-.
    }
    private static void Postfix_OnClientConnect(StartOfRound self)
    {
        // ChatController.RefreshPlayerCache();
    }
    private static void Postfix_OnClientDisconnect(StartOfRound self)
    {
        // ChatController.RefreshPlayerCache();
    }
}
