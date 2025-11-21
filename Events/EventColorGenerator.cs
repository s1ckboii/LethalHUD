using BepInEx;
using BepInEx.Configuration;
using LethalHUD.Configs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LethalHUD.Events;

public static class EventColorGenerator
{
    public static void GenerateDefaults()
    {
        if (Plugins.ConfigEntries == null)
        {
            Loggers.Warning("Plugins.ConfigEntries is null. Cannot generate default event colors.");
            return;
        }

        Dictionary<string, string> defaultColors = [];

        PropertyInfo[] configProps = typeof(ConfigEntries).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo prop in configProps)
        {
            if (prop.PropertyType != typeof(ConfigEntry<string>))
                continue;

            var entryObj = prop.GetValue(Plugins.ConfigEntries);
            if (entryObj is ConfigEntry<string> entry && entry != null)
            {
                defaultColors[prop.Name] = entry.Value;
            }
            else
            {
                Loggers.Warning($"ConfigEntry '{prop.Name}' not initialized, defaulting to '#FFFFFF'.");
                defaultColors[prop.Name] = "#FFFFFF";
            }
        }

        EventColorEntry halloween = new()
        {
            Name = "Halloween",
            Start = "2025-10-01",
            End = "2025-11-01",
            Overrides = []
        };

        EventColorEntry winterfest = new()
        {
            Name = "Winterfest",
            Start = "2025-12-01",
            End = "2026-02-01",
            Overrides = []
        };

        int i = 0;
        foreach (var kvp in defaultColors)
        {
            halloween.Overrides[kvp.Key] = (i % 2 == 0) ? "#FF7518" : "#6A0DAD";
            winterfest.Overrides[kvp.Key] = (i % 2 == 0) ? "#00BFFF" : "#74C0FF";
            i++;
        }

        EventColorConfig config = new()
        {
            Events = [halloween, winterfest]
        };

        string path = Path.Combine(Paths.ConfigPath, "LethalHUD.Events.json");
        File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));

        Loggers.Info("Generated default LethalHUD.Events.json safely.");
    }
}