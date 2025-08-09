using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace LethalHUD.Configs;

public static class ConfigHelper
{
    public static void SkipAutoGen()
    {
        if (LethalConfigProxy.Enabled)
        {
            LethalConfigProxy.SkipAutoGen();
        }
    }
    public static ConfigEntry<string> Bind(bool isHexColor, string section, string key, string defaultValue, string description, bool requiresRestart = false, AcceptableValueBase acceptableValues = null, Action<string> settingChanged = null, ConfigFile configFile = null)
    {
        configFile ??= Plugins.Config;

        var configEntry = acceptableValues == null
            ? configFile.Bind(section, key, defaultValue, description)
            : configFile.Bind(section, key, defaultValue, new ConfigDescription(description, acceptableValues));

        if (settingChanged != null)
        {
            configEntry.SettingChanged += (sender, e) => settingChanged?.Invoke(configEntry.Value);
        }

        if (LethalConfigProxy.Enabled)
        {
            LethalConfigProxy.AddConfig(configEntry, requiresRestart, isHexColor);
        }

        return configEntry;
    }
    public static ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string description, bool requiresRestart = false, AcceptableValueBase acceptableValues = null, Action<T> settingChanged = null, ConfigFile configFile = null)
    {
        configFile ??= Plugins.Config;

        var configEntry = acceptableValues == null
            ? configFile.Bind(section, key, defaultValue, description)
            : configFile.Bind(section, key, defaultValue, new ConfigDescription(description, acceptableValues));

        if (settingChanged != null)
        {
            configEntry.SettingChanged += (sender, e) => settingChanged?.Invoke(configEntry.Value);
        }

        if (LethalConfigProxy.Enabled)
        {
            LethalConfigProxy.AddConfig(configEntry, requiresRestart);
        }

        return configEntry;
    }
    public static Dictionary<ConfigDefinition, string> GetOrphanedConfigEntries(ConfigFile configFile = null)
    {
        configFile ??= Plugins.Config;

        PropertyInfo orphanedEntriesProp = configFile.GetType().GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);
        return (Dictionary<ConfigDefinition, string>)orphanedEntriesProp.GetValue(configFile, null);
    }

    public static void SetConfigEntryValue<T>(ConfigEntry<T> configEntry, string value)
    {
        // Check if T is int
        if (typeof(T) == typeof(int) && int.TryParse(value, out int parsedInt))
            configEntry.Value = (T)(object)parsedInt;
        // Check if T is float
        else if (typeof(T) == typeof(float) && float.TryParse(value, out float parsedFloat))
            configEntry.Value = (T)(object)parsedFloat;
        // Check if T is double
        else if (typeof(T) == typeof(double) && double.TryParse(value, out double parsedDouble))
            configEntry.Value = (T)(object)parsedDouble;
        // Check if T is bool
        else if (typeof(T) == typeof(bool) && bool.TryParse(value, out bool parsedBool))
            configEntry.Value = (T)(object)parsedBool;
        // Check if T is string (no parsing needed)
        else if (typeof(T) == typeof(string))
            configEntry.Value = (T)(object)value;
        else
            // Optionally handle unsupported types
            throw new InvalidOperationException($"Unsupported type: {typeof(T)}");
    }

    // Credit to Kittenji. <- from Zehs but from me too!
    public static void ClearUnusedEntries(ConfigFile configFile = null)
    {
        configFile ??= Plugins.Config;

        var orphanedEntries = GetOrphanedConfigEntries(configFile);

        if (orphanedEntries == null)
        {
            return;
        }

        orphanedEntries.Clear();
        configFile.Save();
    }


    public static Color GetScanColor()
    {
        var cfg = Plugins.ConfigEntries;

        string hex = cfg._lastScanColorChange >= cfg._lastUnifyMostColorsChange
            ? cfg.ScanColor.Value
            : cfg.UnifyMostColors.Value;

        return ParseHexColor(hex);
    }

    public static Color GetSlotColor()
    {
        var cfg = Plugins.ConfigEntries;

        string hex = cfg._lastSlotColorChange >= cfg._lastUnifyMostColorsChange
            ? cfg.SlotColor.Value
            : cfg.UnifyMostColors.Value;

        return ParseHexColor(hex);
    }

    public static Color ParseHexColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return new Color(color.r, color.g, color.b);
        }

        Plugins.Logger.LogWarning($"Invalid HEX color: {hex}. Defaulting to original blue.");
        return new Color(0f, 0.047f, 1f);
    }
}
