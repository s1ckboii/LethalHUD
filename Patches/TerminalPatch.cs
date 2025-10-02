using HarmonyLib;

namespace LethalHUD.Patches;

[HarmonyPatch(typeof(Terminal))]
internal static class TerminalPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("BeginUsingTerminal")]
    private static void OnTerminalBeginUsingTerminal()
    {
        HUDManager.Instance.PingHUDElement(HUDManager.Instance.Inventory, Plugins.ConfigEntries.TerminalFadeDelaysTime.Value, Plugins.ConfigEntries.SlotFade.Value, 0f);
        HUDManager.Instance.PingHUDElement(HUDManager.Instance.Chat, Plugins.ConfigEntries.TerminalFadeDelaysTime.Value, 0.13f, 0.01f);
        HUDManager.Instance.PingHUDElement(HUDManager.Instance.Compass, Plugins.ConfigEntries.TerminalFadeDelaysTime.Value, 0.8f, 0.01f);
    }

    [HarmonyPostfix]
    [HarmonyPatch("QuitTerminal")]
    private static void OnTerminalQuitTerminal()
    {
        HUDManager.Instance.PingHUDElement(HUDManager.Instance.Inventory, Plugins.ConfigEntries.TerminalFadeDelaysTime.Value, 0.13f, Plugins.ConfigEntries.SlotFade.Value);
        HUDManager.Instance.PingHUDElement(HUDManager.Instance.Chat, Plugins.ConfigEntries.TerminalFadeDelaysTime.Value, 0.13f, 0f);
        HUDManager.Instance.PingHUDElement(HUDManager.Instance.Compass, Plugins.ConfigEntries.TerminalFadeDelaysTime.Value, 0.8f, 0.12f);
    }
}
