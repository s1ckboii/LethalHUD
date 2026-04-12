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

    internal static string GetModName(string path)
    {
        try
        {
            string currentDir = Directory.Exists(path)
                ? path
                : Path.GetDirectoryName(path);

            while (!string.IsNullOrEmpty(currentDir))
            {
                string manifestPath = Path.Combine(currentDir, "manifest.json");

                if (File.Exists(manifestPath))
                {
                    string json = File.ReadAllText(manifestPath);
                    JObject obj = JObject.Parse(json);

                    return obj["name"]?.ToString() ?? Path.GetFileNameWithoutExtension(path);
                }

                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            return Path.GetFileNameWithoutExtension(path);
        }
        catch
        {
            return Path.GetFileNameWithoutExtension(path);
        }
    }

    private static void RegisterPrefabs(AssetBundle bundle, string modName)
    {
        GameObject[] prefabs = bundle.LoadAllAssets<GameObject>();

        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null)
                continue;

            string key = prefab.name;

            if (prefab.GetComponent<LHHealthBarRefs>() != null)
            {
                if (!HealthBarPrefabs.ContainsKey(key))
                {
                    HealthBarPrefabs[key] = new StyleEntry<GameObject>()
                    {
                        Name = key,
                        ModName = modName,
                        Asset = prefab
                    };

                    Loggers.Info($"Registered HealthBar: {key} (from {modName})");
                }
                continue;
            }

            if (prefab.GetComponent<LHStaminaBarRefs>() != null)
            {
                if (!StaminaBarPrefabs.ContainsKey(key))
                {
                    StaminaBarPrefabs[key] = new StyleEntry<GameObject>()
                    {
                        Name = key,
                        ModName = modName,
                        Asset = prefab
                    };

                    Loggers.Info($"Registered HealthBar: {key} (from {modName})");
                }
                continue;
            }

            if (prefab.GetComponent<LHBatteryRefs>() != null)
            {
                if (!BatteryPrefabs.ContainsKey(key))
                {
                    BatteryPrefabs[key] = new StyleEntry<GameObject>()
                    {
                        Name = key,
                        ModName = modName,
                        Asset = prefab
                    };

                    Loggers.Info($"Registered HealthBar: {key} (from {modName})");
                }
                continue;
            }

            if (prefab.GetComponent<LHSlotRefs>() != null)
            {
                if (!SlotPrefabs.ContainsKey(key))
                {
                    SlotPrefabs[key] = new StyleEntry<GameObject>()
                    {
                        Name = key,
                        ModName = modName,
                        Asset = prefab
                    };

                    Loggers.Info($"Registered HealthBar: {key} (from {modName})");
                }
                continue;
            }
        }
    }
    private static void RegisterScanAssets(AssetBundle bundle, string modName)
    {
        foreach (Texture2D tex in bundle.LoadAllAssets<Texture2D>())
        {
            if (!tex.name.StartsWith("Scanline_", System.StringComparison.OrdinalIgnoreCase))
                continue;

            string key = tex.name;

            string display = key.StartsWith("Scanline_", System.StringComparison.OrdinalIgnoreCase) ? key["Scanline_".Length..] : key;

            if (!ScanlineTextures.ContainsKey(key))
            {
                ScanlineTextures[key] = new StyleEntry<Texture2D>()
                {
                    Name = key,
                    DisplayName = display,
                    ModName = modName,
                    Asset = tex
                };

                ScanlineDisplayToKey[display] = key;

                Loggers.Info($"Registered Scanline: {key} (from {modName})");
            }
        }

        Dictionary<string, ScanNodeTextures> pairs = [];

        foreach (Sprite sprite in bundle.LoadAllAssets<Sprite>())
        {
            bool isOuter = sprite.name.EndsWith("_Outer");
            bool isInner = sprite.name.EndsWith("_Inner");

            if (!isOuter && !isInner)
                continue;

            string baseName = sprite.name
                .Replace("_Outer", "")
                .Replace("_Inner", "");

            string key = baseName;

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
                ScanNodeSprites[pair.Key] = new StyleEntry<ScanNodeTextures>()
                {
                    Name = pair.Key,
                    ModName = modName,
                    Asset = pair.Value
                };

                Loggers.Info($"Registered ScanNode: {pair.Key} (from {modName})");
            }
        }
    }
}