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
    private static EventColorConfig _config;
    private static bool _hasLoaded = false;
    private static string _lastEventName = null;

    private static readonly Dictionary<string, string> _originalColors = new();
    private static readonly Dictionary<string, ConfigEntry<string>> _internalEntryMap = new();

    // Maps friendly JSON names to ConfigEntries field names
    private static readonly Dictionary<string, string> _friendlyToInternalMap = new()
    {
        { "UnifyMostColors", "UnifyMostColors" },
        { "ForcedEvent", "ForcedEvent" },
        { "ScanColor", "HUDScanColor" },
        { "SlotColor", "HUDSlotColor" },
        { "GradientColorA", "GradientColorA" },
        { "GradientColorB", "GradientColorB" },
        { "HandsFullColor", "HandsFullColor" },
        { "HealthColor", "HealthColor" },
        { "SprintMeterColor", "SprintMeterColor" },
        { "WeightStarterColor", "WeightStarterColor" },
        { "GradientNameColorA", "GradientNameColorA" },
        { "GradientNameColorB", "GradientNameColorB" },
        { "ChatInputText", "ChatInputText" },
        { "ChatMessageColor", "ChatMessageColor" },
        { "GradientMessageColorA", "GradientMessageColorA" },
        { "GradientMessageColorB", "GradientMessageColorB" },
        { "ClockNumberColor", "ClockNumberColor" },
        { "ClockBoxColor", "ClockBoxColor" },
        { "ClockIconColor", "ClockIconColor" },
        { "ClockShipLeaveColor", "ClockShipLeaveColor" },
        { "SignalTextColor", "SignalTextColor" },
        { "SignalText2Color", "SignalText2Color" },
        { "SignalBGColor", "SignalBGColor" },
        { "SignalMessageColor", "SignalMessageColor" },
        { "LoadingTextColor", "LoadingTextColor" },
        { "PlanetSummaryColor", "PlanetSummaryColor" },
        { "PlanetHeaderColor", "PlanetHeaderColor" },
        { "SpectatorTipColor", "SpectatorTipColor" },
        { "SpectatingPlayerColor", "SpectatingPlayerColor" },
        { "HoldEndGameColor", "HoldEndGameColor" },
        { "HoldEndGameVotesColor", "HoldEndGameVotesColor" },
        { "SeperateAdditionalMiscToolsColors", "SeperateAdditionalMiscToolsColors" },
        { "MTColorGradientA", "MTColorGradientA" },
        { "MTColorGradientB", "MTColorGradientB" }
        // Add any missing mappings here
    };

    public static EventColorConfig Config => _config;
    public static bool HasLoaded => _hasLoaded;

    public static void Load()
    {
        string path = Path.Combine(BepInEx.Paths.ConfigPath, "LethalHUD.Events.json");

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

            BuildInternalEntryMap();

            Loggers.Info("EventColorManager loaded event configuration.");
        }
        catch (Exception ex)
        {
            Loggers.Error($"Failed to load EventColorConfig: {ex.Message}");
        }
    }

    private static void BuildInternalEntryMap()
    {
        _internalEntryMap.Clear();

        var configFields = typeof(ConfigEntries)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(ConfigEntry<>));

        foreach (var field in configFields)
        {
            if (field.FieldType.GetGenericArguments()[0] != typeof(string))
                continue;

            if (field.GetValue(Plugins.ConfigEntries) is ConfigEntry<string> entry)
            {
                _internalEntryMap[field.Name] = entry;
            }
        }
    }

    public static EventColorEntry GetActiveEvent()
    {
        if (!_hasLoaded || _config == null || !Plugins.ConfigEntries.EventSystemEnabled.Value)
            return null;

        if (!string.IsNullOrEmpty(Plugins.ConfigEntries.ForcedEvent?.Value) && Plugins.ConfigEntries.ForcedEvent.Value != "None")
        {
            return _config.Events.FirstOrDefault(ev =>
                ev.Name.Equals(Plugins.ConfigEntries.ForcedEvent.Value, StringComparison.OrdinalIgnoreCase));
        }

        DateTime now = DateTime.Now;
        foreach (var ev in _config.Events)
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
        var activeEvent = GetActiveEvent();

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
            // Map friendly JSON key to internal field name
            if (!_friendlyToInternalMap.TryGetValue(kvp.Key, out string internalName))
            {
                Loggers.Warning($"No ConfigEntry mapping found for key '{kvp.Key}' in event '{activeEvent.Name}'");
                continue;
            }

            if (!_internalEntryMap.TryGetValue(internalName, out var entry))
                continue;

            if (!_originalColors.ContainsKey(internalName))
                _originalColors[internalName] = entry.Value;

            entry.Value = kvp.Value;
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
