using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using LethalHUD.Compats;
using LethalHUD.Configs;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD;
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(ModCompats.LethalConfig_PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModCompats.NiceChat_PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugins : BaseUnityPlugin
{
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

        Patches.HUDManagerPatch.Init();
        Patches.PlayerControllerBPatch.Init();

        string pluginFolderPath = Path.GetDirectoryName(Info.Location);
        string assetBundleFilePath = Path.Combine(pluginFolderPath, "insanelyoriginalassetbundlenameforlethalhud");
        AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

        ConfigEntries = new ConfigEntries();

        if (assetBundle == null)
        {
            Logger.LogError("Failed to load insanelyoriginalassetbundlenameforlethalhud assetbundle.");
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
