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
public class Plugins : BaseUnityPlugin
{

    private readonly Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);

    private static Plugins instance;

    internal static ManualLogSource mls;

    internal static Dictionary<ScanLines, Texture2D> ScanlineTextures = [];

    public static ConfigFile BepInExConfig() { return instance.Config; }

    public void Awake()
    {
        instance ??= this;

        mls = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);
        ConfigEntries.Instance.Setup();

        mls.LogMessage("Plugin " + MyPluginInfo.PLUGIN_NAME + " loaded!");

        harmony.PatchAll(typeof(Patches.HUDManagerPatch));

        string pluginFolderPath = Path.GetDirectoryName(Info.Location);
        string assetBundleFilePath = Path.Combine(pluginFolderPath, "incrediblyoriginalassetbundlenameforlethalhud");
        AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

        if (assetBundle == null)
        {
            Logger.LogError("Failed to load incrediblyoriginalassetbundlenameforlethalhud assetbundle.");
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
                mls.LogWarning($"Texture '{name}' not found in asset bundle.");
            }
        }
    }
}
