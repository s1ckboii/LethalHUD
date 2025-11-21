using BepInEx;
using BepInEx.Configuration;
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
    public static string GetConfigEntryValue(string name)
    {
        ConfigEntries configEntries = Plugins.ConfigEntries;
        if (configEntries == null) return null;

        Type type = configEntries.GetType();

        MemberInfo member = (MemberInfo)type.GetField(name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (member == null) return null;

        object entry = member switch
        {
            FieldInfo f => f.GetValue(configEntries),
            PropertyInfo p => p.GetValue(configEntries),
            _ => null
        };
        if (entry == null) return null;

        Type fieldType = entry.GetType();
        if (!fieldType.IsGenericType || fieldType.GetGenericTypeDefinition() != typeof(ConfigEntry<>))
            return null;

        PropertyInfo valueProp = fieldType.GetProperty("Value");
        return valueProp?.GetValue(entry)?.ToString();
    }
    public static bool HasConfigEntry(string readableName)
    {
        string actualName = GetActualFieldName(readableName);
        return actualName != null;
    }

    public static void SetConfigEntryValueByName(string name, string value)
    {
        ConfigEntries configEntries = Plugins.ConfigEntries;
        if (configEntries == null) return;

        FieldInfo field = configEntries.GetType().GetField(name,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (field == null) return;

        Type fieldType = field.FieldType;
        if (!fieldType.IsGenericType || fieldType.GetGenericTypeDefinition() != typeof(ConfigEntry<>))
            return;

        var configEntry = field.GetValue(configEntries);
        if (configEntry == null) return;

        var valueProp = fieldType.GetProperty("Value");
        Type targetType = fieldType.GetGenericArguments()[0];

        object parsedValue = value;

        try
        {
            if (targetType == typeof(int)) parsedValue = int.Parse(value);
            else if (targetType == typeof(float)) parsedValue = float.Parse(value);
            else if (targetType == typeof(double)) parsedValue = double.Parse(value);
            else if (targetType == typeof(bool)) parsedValue = bool.Parse(value);
        }
        catch { return; }

        valueProp.SetValue(configEntry, parsedValue);
    }

    public static List<string> GetAllColorConfigKeys()
    {
        List<string> keys = [];
        ConfigEntries configEntries = Plugins.ConfigEntries;
        if (configEntries == null) return keys;

        Type cfgType = typeof(ConfigEntries);

        List<MemberInfo> members =
        [
            .. cfgType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
            .. cfgType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
        ];

        foreach (MemberInfo member in members)
        {
            Type memberType = null;

            switch (member)
            {
                case FieldInfo field:
                    memberType = field.FieldType;
                    break;

                case PropertyInfo prop:
                    memberType = prop.PropertyType;
                    break;
            }

            if (memberType == null) continue;
            if (!memberType.IsGenericType || memberType.GetGenericTypeDefinition() != typeof(ConfigEntry<>))
                continue;

            Type genericArg = memberType.GetGenericArguments()[0];
            if (genericArg != typeof(string)) continue;

            if (!member.Name.ToLower().Contains("color")) continue;

            keys.Add(member.Name);
        }

        return keys;
    }

    public static string GetActualFieldName(string readableName)
    {
        ConfigEntries configEntries = Plugins.ConfigEntries;
        if (configEntries == null) return null;

        Type type = configEntries.GetType();
        List<MemberInfo> members =
        [
            .. type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
            .. type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
        ];

        foreach (MemberInfo member in members)
        {
            string cleanName = member.Name.Replace("<", "").Replace(">k__BackingField", "");
            if (cleanName == readableName)
                return member.Name;
        }
        return null;
    }

    public static void SetConfigEntryValueByReadableName(string readableName, string value)
    {
        string actualName = GetActualFieldName(readableName);
        if (actualName != null)
            SetConfigEntryValueByName(actualName, value);
    }
}