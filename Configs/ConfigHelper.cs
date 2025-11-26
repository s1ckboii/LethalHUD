using BepInEx.Configuration;
using LethalHUD.Compats;
using LethalHUD.HUD;
using System;
using UnityEngine;

namespace LethalHUD.Configs;
internal static class ConfigHelper
{
    internal static void SkipAutoGen()
    {
        if (ModCompats.IsLethalConfigPresent)
            LethalConfigProxy.SkipAutoGen();
    }
    internal static ConfigEntry<string> Bind(bool isHexColor, string section, string key, string defaultValue, string description, bool requiresRestart = false, AcceptableValueBase acceptableValues = null, Action<string> settingChanged = null, ConfigFile configFile = null)
    {
        configFile ??= Plugins.Config;

        ConfigEntry<string> configEntry = acceptableValues == null
            ? configFile.Bind(section, key, defaultValue, description)
            : configFile.Bind(section, key, defaultValue, new ConfigDescription(description, acceptableValues));

        if (settingChanged != null)
        {
            configEntry.SettingChanged += (sender, e) => settingChanged?.Invoke(configEntry.Value);
        }

        if (ModCompats.IsLethalConfigPresent)
        {
            LethalConfigProxy.AddConfig(configEntry, requiresRestart, isHexColor);
        }

        return configEntry;
    }
    internal static ConfigEntry<T> Bind<T>(string section, string key, T defaultValue, string description, bool requiresRestart = false, AcceptableValueBase acceptableValues = null, Action<T> settingChanged = null, ConfigFile configFile = null)
    {
        configFile ??= Plugins.Config;

        ConfigEntry<T> configEntry = acceptableValues == null
            ? configFile.Bind(section, key, defaultValue, description)
            : configFile.Bind(section, key, defaultValue, new ConfigDescription(description, acceptableValues));

        if (settingChanged != null)
        {
            configEntry.SettingChanged += (sender, e) => settingChanged?.Invoke(configEntry.Value);
        }

        if (ModCompats.IsLethalConfigPresent)
        {
            LethalConfigProxy.AddConfig(configEntry, requiresRestart);
        }

        return configEntry;
    }

    internal static void SetConfigEntryValue<T>(ConfigEntry<T> configEntry, string value)
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
    #region DevTool
#if DEBUG
    internal static void ResetMyConfigs()
    {
        ConfigEntries cfg = Plugins.ConfigEntries;
        if (cfg == null) return;

        FieldInfo[] fields = typeof(ConfigEntries).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            Type fieldType = field.FieldType;

            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(ConfigEntry<>))
            {
                object configEntry = field.GetValue(cfg);
                if (configEntry == null) continue;

                PropertyInfo defaultProp = fieldType.GetProperty("DefaultValue");
                PropertyInfo valueProp = fieldType.GetProperty("Value");
                if (defaultProp != null && valueProp != null)
                {
                    object defaultValue = defaultProp.GetValue(configEntry);
                    valueProp.SetValue(configEntry, defaultValue);
                }
            }
        }

        Plugins.Config.OrphanedEntries.Clear();
        Plugins.Logger.LogInfo("LethalHUD configs have been reset to default values.");
    }
#endif
    #endregion
    internal static Color GetScanColor()
    {
        ConfigEntries cfg = Plugins.ConfigEntries;

        string hex = cfg._lastScanColorChange >= cfg._lastUnifyMostColorsChange
            ? cfg.ScanColor.Value
            : cfg.UnifyMostColors.Value;

        return HUDUtils.ParseHexColor(hex);
    }

    internal static Color GetSlotColor()
    {
        ConfigEntries cfg = Plugins.ConfigEntries;

        string hex = cfg._lastSlotColorChange >= cfg._lastUnifyMostColorsChange
            ? cfg.SlotColor.Value
            : cfg.UnifyMostColors.Value;

        return HUDUtils.ParseHexColor(hex);
    }
}