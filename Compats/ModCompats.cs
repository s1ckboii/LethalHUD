using BepInEx.Bootstrap;

namespace LethalHUD.Compats;
internal class ModCompats
{
    internal const string LethalConfig_PLUGIN_GUID = "ainavt.lc.lethalconfig";
    internal const string NiceChat_PLUGIN_GUID = "taffyko.NiceChat";
    internal static bool IsLethalConfigPresent => Chainloader.PluginInfos.ContainsKey(LethalConfig_PLUGIN_GUID);
    internal static bool IsNiceChatPresent => Chainloader.PluginInfos.ContainsKey(NiceChat_PLUGIN_GUID);
}
