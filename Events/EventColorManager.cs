using BepInEx;
using BepInEx.Configuration;
using LethalHUD.Configs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LethalHUD.Events;

public static class EventColorManager
{
    public static EventColorConfig Config => _config;
    public static bool HasLoaded => _hasLoaded;

    private static EventColorConfig _config;
    private static bool _hasLoaded = false;
    private static string _lastEventName = null;

    private static readonly Dictionary<string, string> _originalColors = [];
    private static readonly Dictionary<string, string> _friendlyToInternalMap = [];
    private static readonly Dictionary<string, ConfigEntry<string>> _internalEntryMap = [];

    public static void Load()
    {
        string path = Path.Combine(Paths.ConfigPath, "LethalHUD.Events.json");

        if (!File.Exists(path))
        {
            EventColorGenerator.GenerateDefaults();
            Loggers.Info("Generated default LethalHUD.Events.json with Halloween and Winterfest events.");
        }

        try
        {
            string json = File.ReadAllText(path);
            _config = JsonConvert.DeserializeObject<EventColorConfig>(json);
            _hasLoaded = true;

            BuildFriendlyToInternalMap();

            Loggers.Info("EventColorManager loaded event configuration.");
        }
        catch (Exception ex)
        {
            Loggers.Error($"Failed to load EventColorConfig: {ex.Message}");
        }
    }

    private static void BuildFriendlyToInternalMap()
    {
        _friendlyToInternalMap.Clear();

        PropertyInfo[] props = typeof(ConfigJsonProperties).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo prop in props)
        {
            string internalName = prop.Name;
            string friendlyName = prop.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? internalName;

            if (ConfigUtils.HasConfigEntry(internalName))
                _friendlyToInternalMap[friendlyName] = internalName;
            else
                Loggers.Warning($"No matching ConfigEntry for JSON property '{friendlyName}' ({internalName})");
        }
    }

    public static EventColorEntry GetActiveEvent()
    {
        if (!_hasLoaded || _config == null || !Plugins.ConfigEntries.EventSystemEnabled.Value)
            return null;

        if (!string.IsNullOrEmpty(Plugins.ConfigEntries.ForcedEvent?.Value) && Plugins.ConfigEntries.ForcedEvent.Value != "None")
        {
            return _config.Events.FirstOrDefault(ev => ev.Name == Plugins.ConfigEntries.ForcedEvent.Value);
        }

        DateTime now = DateTime.Now;
        foreach (EventColorEntry ev in _config.Events)
        {
            if (!DateTime.TryParse(ev.Start, out DateTime start)) continue;
            if (!DateTime.TryParse(ev.End, out DateTime end)) continue;

            if (now >= start && now <= end)
                return ev;
        }

        return null;
    }

    public static void Update()
    {
        EventColorEntry activeEvent = GetActiveEvent();

        if (activeEvent == null)
        {
            if (_lastEventName != null)
            {
                RestoreOriginalColors();
                _lastEventName = null;
            }
            return;
        }

        if (_lastEventName != activeEvent.Name)
        {
            ApplyEventColors(activeEvent);
            _lastEventName = activeEvent.Name;
        }
    }

    private static void ApplyEventColors(EventColorEntry activeEvent)
    {
        if (activeEvent?.Overrides == null) return;

        foreach (var kvp in activeEvent.Overrides)
        {
            string friendlyName = kvp.Key;
            string newValue = kvp.Value;

            if (!_friendlyToInternalMap.ContainsKey(friendlyName)) continue;

            string internalName = _friendlyToInternalMap[friendlyName];
            var entry = _internalEntryMap[internalName];

            if (!_originalColors.ContainsKey(internalName))
                _originalColors[internalName] = entry.Value;

            entry.Value = newValue;
        }

        Loggers.Info($"Applied event colors for event: {activeEvent.Name}");
    }
    private static void RestoreOriginalColors()
    {
        foreach (var kvp in _originalColors)
        {
            string internalName = kvp.Key;
            string originalValue = kvp.Value;

            if (_internalEntryMap.TryGetValue(internalName, out var entry))
            {
                entry.Value = originalValue;
            }
        }

        _originalColors.Clear();
        Loggers.Info("Restored all color overrides to original config values.");
    }
}
