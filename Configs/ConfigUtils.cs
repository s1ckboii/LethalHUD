using BepInEx;
using BepInEx.Configuration;
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
}