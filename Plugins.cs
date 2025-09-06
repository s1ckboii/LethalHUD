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
[BepInDependency(GoodItemScan.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModCompats.BetterScanVision_PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModCompats.EladsHUD_PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModCompats.HotbarPlus_PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugins : BaseUnityPlugin
{

    internal static Plugins Instance { get; private set; }
    internal static new ManualLogSource Logger { get; private set; }
    internal static new ConfigFile Config { get; private set; }
    internal static ConfigEntries ConfigEntries { get; private set; }

    internal static Dictionary<ScanLines, Texture2D> ScanlineTextures = [];
    internal struct ScanNodeCircleTextures
    {
        public Sprite Outer;
        public Sprite Inner;
    }

    internal static Dictionary<ScanNodeShape, ScanNodeCircleTextures> ScanNodeSprites = [];

    public void Awake()
    {
        if (Instance == null) Instance = this;


        Logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);
        Loggers.Info("Plugin " + MyPluginInfo.PLUGIN_NAME + " loaded!");

        var bootstrapConfig = ConfigUtils.CreateLocalConfigFile(this, "bootstrap", true);
        var useLocalEntry = bootstrapConfig.Bind(
            "Main",
            "Use Local Config",
            false,
            "If enabled, uses a local config file instead of the global config. Requires restart."
        );
        bool useLocal = useLocalEntry.Value;

        Config = useLocal
            ? ConfigUtils.CreateLocalConfigFile(this)
            : ConfigUtils.CreateGlobalConfigFile(this);

        ConfigEntries = new ConfigEntries();

        Patches.HUDManagerPatch.Init();
        Patches.PlayerControllerBPatch.Init();

        string pluginFolderPath = Path.GetDirectoryName(Info.Location);
        string assetBundleFilePath = Path.Combine(pluginFolderPath, "unfathomablyridiculousoriginalassetbundlenameforlethalhud");
        AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

        if (assetBundle == null)
        {
            Loggers.Error("Failed to load unfathomablyridiculousoriginalassetbundlenameforlethalhud assetbundle.");
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
                Loggers.Warning($"Texture '{name}' not found in asset bundle.");
            }
        }
        foreach (ScanNodeShape shape in System.Enum.GetValues(typeof(ScanNodeShape)))
        {
            string outerName = $"{shape}_Outer";
            string innerName = $"{shape}_Inner";

            Sprite outerTex = assetBundle.LoadAsset<Sprite>(outerName);
            Sprite innerTex = assetBundle.LoadAsset<Sprite>(innerName);

            if (outerTex == null) Logger.LogWarning($"ScanNode texture '{outerName}' not found.");
            if (innerTex == null) Logger.LogWarning($"ScanNode texture '{innerName}' not found.");

            ScanNodeSprites[shape] = new ScanNodeCircleTextures
            {
                Outer = outerTex,
                Inner = innerTex
            };
        }

        bootstrapConfig.Save();
    }
}
