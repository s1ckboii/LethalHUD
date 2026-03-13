using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LethalHUD.API;
using LethalHUD.Compats;
using LethalHUD.Configs;
using LethalHUD.Networking;
using LethalHUD.Scan;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
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
    internal static Harmony Harmony { get; private set; }
    internal static new ManualLogSource Logger { get; private set; }
    internal static new ConfigFile Config { get; private set; }
    internal static ConfigEntries ConfigEntries { get; private set; }

    internal static readonly Dictionary<string, GameObject> HealthBarPrefabs = [];
    internal static readonly Dictionary<string, GameObject> StaminaBarPrefabs = [];
    internal static readonly Dictionary<string, GameObject> SlotPrefabs = [];

    internal static Texture2D DefaultScanlineTexture;
    internal static ScanNodeTextures DefaultScanNodeSprites;

    internal static Dictionary<string, Texture2D> ScanlineTextures = [];
    internal static Dictionary<string, ScanNodeTextures> ScanNodeSprites = [];
    internal static Dictionary<string, Sprite> LoadingScreens = [];
    internal struct ScanNodeTextures
    {
        public Sprite Outer;
        public Sprite Inner;
    }

    internal static bool NetworkingDisabled;

    public void Awake()
    {
        if (Instance == null) Instance = this;

        Harmony = new(MyPluginInfo.PLUGIN_GUID);
        Logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);

        ConfigFile bootstrapConfig = ConfigUtils.CreateLocalConfigFile(this, "bootstrap", true);
        ConfigEntry<bool> useLocalEntry = bootstrapConfig.Bind("Main", "Use Local Config", false, "If enabled, uses a local config file instead of the global config. Requires restart.");
        ConfigEntry<bool> disableNetworkingEntry = bootstrapConfig.Bind("Main", "Disable Networked Features", false, "If enabled, disables all networked features. Requires restart.");
        
        bool useLocal = useLocalEntry.Value;
        NetworkingDisabled = disableNetworkingEntry.Value;

        if (!NetworkingDisabled)
        {
            SerializeNetworkVariables();
        }
        else
        {
            Loggers.Info("Networked HUD features are disabled via bootstrap config.");
        }

        Config = useLocal
            ? ConfigUtils.CreateLocalConfigFile(this)
            : ConfigUtils.CreateGlobalConfigFile(this);

        BundleLoader.Init();
        ImageLoader.LoadFromFolders();

        ConfigEntries = new ConfigEntries();

        Harmony.PatchAll();
        bootstrapConfig.Save();

        Loggers.Info("Plugin " + MyPluginInfo.PLUGIN_NAME + " loaded!");
    }

    private static void SerializeNetworkVariables()
    {
        NetworkVariableSerializationTypes.InitializeSerializer_UnmanagedByMemcpy<PlayerColorInfo>();
        NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<PlayerColorInfo>();
    }

    internal static void CacheDefaults()
    {
        CacheDefaultScanline();
        CacheDefaultScannodes();
    }

    private static void CacheDefaultScanline()
    {
        if (DefaultScanlineTexture != null)
            return;

        Bloom bloom = ScanController.ScanBloom;

        if (bloom == null)
            return;

        Texture tex = bloom.dirtTexture.value;

        if (tex is Texture2D tex2D)
        {
            DefaultScanlineTexture = tex2D;
            ScanlineTextures["Default"] = tex2D;

            Logger.LogInfo("Cached vanilla scanline texture.");
        }
    }

    private static void CacheDefaultScannodes()
    {
        if (DefaultScanNodeSprites.Inner != null)
            return;

        GameObject scanner = GameObject.Find("UI/Canvas/ObjectScanner");

        if (scanner == null)
            return;

        foreach (Transform child in scanner.transform)
        {
            if (!child.name.StartsWith("ScanObject"))
                continue;

            Image inner = child.Find("Circle/Inner")?.GetComponent<Image>();
            Image outer = child.Find("Circle/Outer")?.GetComponent<Image>();

            if (inner == null || outer == null)
                continue;

            DefaultScanNodeSprites = new ScanNodeTextures
            {
                Inner = inner.sprite,
                Outer = outer.sprite
            };

            ScanNodeSprites["Default"] = DefaultScanNodeSprites;

            Logger.LogInfo("Cached vanilla scannode sprites.");
            break;
        }
    }
}