using BepInEx;
using BepInEx.Configuration;
using System;
using System.IO;
using UnityEngine;

namespace LethalHUD.Configs;
internal static class ConfigUtils
{
    private static string GetPluginPersistentDataPath()
    {
        return Path.Combine(Application.persistentDataPath, MyPluginInfo.PLUGIN_NAME);
    }

    private static ConfigFile CreateConfigFile(BaseUnityPlugin plugin, string path, string name = null, bool saveOnInit = false)
    {
        BepInPlugin metadata = MetadataHelper.GetMetadata(plugin);
        name ??= metadata.GUID;
        name += ".cfg";
        return new ConfigFile(Path.Combine(path, name), saveOnInit, metadata);
    }

    internal static ConfigFile CreateLocalConfigFile(BaseUnityPlugin plugin, string name = null, bool saveOnInit = false)
    {
        return CreateConfigFile(plugin, Paths.ConfigPath, name, saveOnInit);
    }

    internal static ConfigFile CreateGlobalConfigFile(BaseUnityPlugin plugin, string name = null, bool saveOnInit = false)
    {
        string path = GetPluginPersistentDataPath();
        name ??= "global";
        return CreateConfigFile(plugin, path, name, saveOnInit);
    }
    /*
    private const string JsonFileName = "OriginalColors.json";

    private static string GetPluginFolder()
    {
        string folder = Path.Combine(Application.persistentDataPath, MyPluginInfo.PLUGIN_NAME);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        return folder;
    }

    private static string GetJsonPath(string fileName = JsonFileName)
    {
        return Path.Combine(GetPluginFolder(), fileName);
    }

    public static void SaveStoredValues(object values, string name = "Default")
    {
        try
        {
            string path = GetJsonPath(name + ".json");
            string json = JsonUtility.ToJson(values, true);
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Plugins.Logger.LogError($"Failed to save stored values '{name}': {e}");
        }
    }

    public static T LoadStoredValues<T>(string name = "Default") where T : class
    {
        try
        {
            string path = GetJsonPath(name + ".json");
            if (!File.Exists(path)) return null;
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception e)
        {
            Plugins.Logger.LogError($"Failed to load stored values '{name}': {e}");
            return null;
        }
    }
    */
}