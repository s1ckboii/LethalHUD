using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalHUD.Configs;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static LethalHUD.Scan.ScanlinesEnums;

namespace LethalHUD;
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(LethalConfigProxy.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugins : BaseUnityPlugin
{

    private readonly Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);

    internal static Dictionary<ScanLines, Texture2D> ScanlineTextures = [];

    internal static Plugins Instance { get; private set; }
    internal static new ManualLogSource Logger { get; private set; }
    internal static new ConfigFile Config { get; private set; }

    internal static ConfigEntries ConfigEntries {  get; private set; }



    public void Awake()
    {
        if (Instance == null) Instance = this;

        Logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);
        Logger.LogInfo("Plugin " + MyPluginInfo.PLUGIN_NAME + " loaded!");

        Config = ConfigUtils.CreateGlobalConfigFile(this);

        harmony.PatchAll(typeof(Patches.HUDManagerPatch));
        harmony.PatchAll(typeof(Patches.StartOfRoundPatch));
        harmony.PatchAll(typeof(Patches.PlayerControllerBPatch));

        string pluginFolderPath = Path.GetDirectoryName(Info.Location);
        string assetBundleFilePath = Path.Combine(pluginFolderPath, "unimaginablyoriginalassetbundlenameforlethalhud");
        AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

        ConfigEntries = new ConfigEntries();

        if (assetBundle == null)
        {
            Logger.LogError("Failed to load unimaginablyoriginalassetbundlenameforlethalhud assetbundle.");
            return;
        }

        foreach (ScanLines scanLine in System.Enum.GetValues(typeof(ScanLines)))
        {
            string name = scanLine.ToString();
            Texture2D tex = assetBundle.LoadAsset<Texture2D>(name);

            if (tex != null)
            {
                ScanlineTextures[scanLine] = tex;
            }
            else
            {
                Logger.LogWarning($"Texture '{name}' not found in asset bundle.");
            }
        }
    }
}
