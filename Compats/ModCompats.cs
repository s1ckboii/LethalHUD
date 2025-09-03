using BepInEx.Bootstrap;

namespace LethalHUD.Compats;
internal class ModCompats
{
    internal const string LethalConfig_PLUGIN_GUID = "ainavt.lc.lethalconfig";
    internal const string NiceChat_PLUGIN_GUID = "taffyko.NiceChat";
    internal const string BetterScanVision_PLUGIN_GUID = "DBJ.BetterScanVision";
    internal const string EladsHUD_PLUGIN_GUID = "me.eladnlg.customhud";
    internal static bool IsLethalConfigPresent => Chainloader.PluginInfos.ContainsKey(LethalConfig_PLUGIN_GUID);
    internal static bool IsNiceChatPresent => Chainloader.PluginInfos.ContainsKey(NiceChat_PLUGIN_GUID);
    internal static bool IsGoodItemScanPresent => Chainloader.PluginInfos.ContainsKey(GoodItemScan.MyPluginInfo.PLUGIN_GUID);
    internal static bool IsBetterScanVisionPresent => Chainloader.PluginInfos.ContainsKey(BetterScanVision_PLUGIN_GUID);
    internal static bool IsEladsHUDPresent => Chainloader.PluginInfos.ContainsKey(EladsHUD_PLUGIN_GUID);
}