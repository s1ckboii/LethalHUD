using BepInEx;
using LethalHUD.CustomHUD.Refs;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static LethalHUD.Plugins;

namespace LethalHUD.API;
internal static class BundleLoader
{
    internal static readonly List<AssetBundle> LoadedBundles = [];

    internal static void Init()
    {
        string pluginsFolder = Path.Combine(Paths.BepInExRootPath, "plugins");

        if (!Directory.Exists(pluginsFolder))
            return;

        string[] bundles = Directory.GetFiles(pluginsFolder, "*.lethalhudbundle", SearchOption.AllDirectories);

        foreach (string bundlePath in bundles)
        {
            LoadBundle(bundlePath);
        }
    }

    private static void LoadBundle(string bundlePath)
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

        if (bundle == null)
        {
            Loggers.Warning($"Failed to load bundle: {bundlePath}");
            return;
        }

        LoadedBundles.Add(bundle);

        string modName = GetModName(bundlePath);

        RegisterPrefabs(bundle, modName);
        RegisterScanAssets(bundle, modName);

        Loggers.Info($"Loaded LethalHUD bundle: {Path.GetFileName(bundlePath)}");
    }

    internal static string GetModName(string bundlePath)
    {
        try
        {
            string directory = Path.GetDirectoryName(bundlePath);
            string manifestPath = Path.Combine(directory, "manifest.json");

            if (!File.Exists(manifestPath))
                return Path.GetFileNameWithoutExtension(bundlePath);

            string json = File.ReadAllText(manifestPath);
            JObject obj = JObject.Parse(json);

            return obj["name"]?.ToString() ?? Path.GetFileNameWithoutExtension(bundlePath);
        }
        catch
        {
            return Path.GetFileNameWithoutExtension(bundlePath);
        }
    }

    private static void RegisterPrefabs(AssetBundle bundle, string modName)
    {
        GameObject[] prefabs = bundle.LoadAllAssets<GameObject>();

        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null)
                continue;

            string styleName = prefab.name;
            string uniqueName = $"[{modName}] {styleName}";

            if (prefab.GetComponent<LHHealthBarRefs>() != null)
            {
                if (!HealthBarPrefabs.ContainsKey(uniqueName))
                {
                    HealthBarPrefabs.Add(uniqueName, prefab);
                    Loggers.Info($"Registered HealthBar: {uniqueName}");
                }
                continue;
            }

            if (prefab.GetComponent<LHStaminaBarRefs>() != null)
            {
                if (!StaminaBarPrefabs.ContainsKey(uniqueName))
                {
                    StaminaBarPrefabs.Add(uniqueName, prefab);
                    Loggers.Info($"Registered StaminaBar: {uniqueName}");
                }
                continue;
            }

            if (prefab.GetComponent<LHSlotRefs>() != null)
            {
                if (!SlotPrefabs.ContainsKey(uniqueName))
                {
                    SlotPrefabs.Add(uniqueName, prefab);
                    Loggers.Info($"Registered InventoryFrame: {uniqueName}");
                }
                continue;
            }
        }
    }
    private static void RegisterScanAssets(AssetBundle bundle, string modName)
    {
        foreach (Texture2D tex in bundle.LoadAllAssets<Texture2D>())
        {
            if (tex.name.EndsWith("_Outer") || tex.name.EndsWith("_Inner"))
                continue;

            string key = $"[{modName}] {tex.name}";

            if (!ScanlineTextures.ContainsKey(key))
            {
                ScanlineTextures.Add(key, tex);
                Loggers.Info($"Registered Scanline: {key}");
            }
        }

        Dictionary<string, ScanNodeTextures> pairs = new();

        foreach (Sprite sprite in bundle.LoadAllAssets<Sprite>())
        {
            bool isOuter = sprite.name.EndsWith("_Outer");
            bool isInner = sprite.name.EndsWith("_Inner");

            if (!isOuter && !isInner)
                continue;

            string baseName = sprite.name
                .Replace("_Outer", "")
                .Replace("_Inner", "");

            string key = $"[{modName}] {baseName}";

            if (!pairs.ContainsKey(key))
                pairs[key] = new ScanNodeTextures();

            ScanNodeTextures node = pairs[key];

            if (isOuter)
                node.Outer = sprite;

            if (isInner)
                node.Inner = sprite;

            pairs[key] = node;
        }

        foreach (var pair in pairs)
        {
            if (pair.Value.Inner != null && pair.Value.Outer != null)
            {
                ScanNodeSprites[pair.Key] = pair.Value;
                Loggers.Info($"Registered ScanNode: {pair.Key}");
            }
        }

        foreach (Sprite sprite in bundle.LoadAllAssets<Sprite>())
        {
            if (!sprite.name.StartsWith("Loading_"))
                continue;

            string name = sprite.name.Replace("Loading_", "");
            string key = $"[{modName}] {name}";

            if (!LoadingScreens.ContainsKey(key))
            {
                LoadingScreens.Add(key, sprite);
                Loggers.Info($"Registered LoadingScreen: {key}");
            }
        }
    }
}