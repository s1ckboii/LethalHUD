using BepInEx;
using BepInEx.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
    public static bool HasConfigEntry(string internalName)
    {
        var field = typeof(ConfigEntries).GetField(internalName, BindingFlags.Public | BindingFlags.Instance);
        return field != null && field.FieldType == typeof(ConfigEntry<string>);
    }

    public static ConfigEntry<string> GetConfigEntry(string internalName)
    {
        var field = typeof(ConfigEntries).GetField(internalName, BindingFlags.Public | BindingFlags.Instance);
        if (field == null) return null;
        return field.GetValue(Plugins.ConfigEntries) as ConfigEntry<string>;
    }
}