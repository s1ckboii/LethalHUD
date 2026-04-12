using HarmonyLib;
using LethalConfig.ConfigItems;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace LethalHUD.Patches;

[HarmonyPatch(typeof(TextDropDownConfigItem), "CreateGameObjectForConfig")]
internal static class LethalConfigPatch
{
    internal static readonly Dictionary<TextDropDownConfigItem, List<TextMeshProUGUI>> Bindings = [];

    private static void Postfix(TextDropDownConfigItem __instance, ref GameObject __result)
    {
        if (__result == null) return;

        var dropdown = __result.GetComponentInChildren<TMP_Dropdown>();
        if (dropdown == null) return;

        var texts = __result.GetComponentsInChildren<TextMeshProUGUI>(true);
        if (texts.Length == 0) return;

        TextMeshProUGUI description = null;

        foreach (var t in texts)
        {
            if (!string.IsNullOrEmpty(t.text) && t.text.Contains("Default"))
            {
                description = t;
                break;
            }
        }

        if (description == null)
            description = texts[0];

        var existing = description.transform.parent.Find("StyleSourceText");
        if (existing != null) return;

        var clone = Object.Instantiate(description, description.transform.parent);
        clone.name = "StyleSourceText";
        clone.fontSize *= 0.85f;
        clone.color = new Color(0.7f, 0.7f, 0.7f, 1f);

        if (!Bindings.ContainsKey(__instance))
            Bindings[__instance] = [];

        Bindings[__instance].Add(clone);

        void Refresh()
        {
            string selected = GetValue(__instance);

            if (string.IsNullOrEmpty(selected) || selected == "Default")
            {
                clone.gameObject.SetActive(false);
                return;
            }

            clone.gameObject.SetActive(true);
            clone.text = $"From: {GetMod(selected)}";
        }

        Refresh();

        dropdown.onValueChanged.AddListener(_ => Refresh());
    }

    private static string GetValue(object item)
    {
        if (item == null) return "Default";

        var type = item.GetType();

        var prop = type.GetProperty("CurrentValue", BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop != null)
        {
            var val = prop.GetValue(item);
            if (val != null)
                return val.ToString();
        }

        var baseProp = type.GetProperty("BaseConfigEntry", BindingFlags.NonPublic | BindingFlags.Instance);
        if (baseProp != null)
        {
            var entry = baseProp.GetValue(item);
            if (entry != null)
            {
                var boxed = entry.GetType().GetProperty("BoxedValue")?.GetValue(entry);
                if (boxed != null)
                    return boxed.ToString();
            }
        }

        return "Default";
    }

    private static string GetMod(string value)
    {
        if (value == "Default") return "Vanilla";

        string key = value;

        if (!Plugins.ScanlineTextures.ContainsKey(key))
        {
            if (Plugins.ScanlineDisplayToKey.TryGetValue(key, out var mapped))
                key = mapped;
        }

        if (Plugins.ScanlineTextures.TryGetValue(key, out var scan)) return scan.ModName;
        if (Plugins.HealthBarPrefabs.TryGetValue(key, out var hp)) return hp.ModName;
        if (Plugins.StaminaBarPrefabs.TryGetValue(key, out var stamina)) return stamina.ModName;
        if (Plugins.BatteryPrefabs.TryGetValue(key, out var battery)) return battery.ModName;
        if (Plugins.SlotPrefabs.TryGetValue(key, out var slot)) return slot.ModName;
        if (Plugins.ScanNodeSprites.TryGetValue(key, out var node)) return node.ModName;

        return "Unknown";
    }
}

[HarmonyPatch(typeof(BaseConfigItem))]
internal static class ConfigValueChangedPatch
{
    static MethodBase TargetMethod()
    {
        return AccessTools.PropertySetter(typeof(BaseConfigItem), "CurrentBoxedValue");
    }

    static void Postfix(BaseConfigItem __instance)
    {
        if (__instance is not TextDropDownConfigItem item)
            return;

        if (!LethalConfigPatch.Bindings.TryGetValue(item, out var texts))
            return;

        texts.RemoveAll(t => t == null);

        foreach (var txt in texts)
        {
            string selected = GetValue(item);

            if (string.IsNullOrEmpty(selected) || selected == "Default")
            {
                txt.gameObject.SetActive(false);
            }
            else
            {
                txt.gameObject.SetActive(true);
                txt.text = $"From: {GetMod(selected)}";
            }
        }
    }

    private static string GetValue(object item)
    {
        var prop = item.GetType().GetProperty("CurrentValue", BindingFlags.NonPublic | BindingFlags.Instance);
        return prop?.GetValue(item)?.ToString() ?? "Default";
    }

    private static string GetMod(string value)
    {
        if (value == "Default") return "Vanilla";

        string key = value;
        if (!Plugins.ScanlineTextures.ContainsKey(key))
        {
            if (Plugins.ScanlineDisplayToKey.TryGetValue(key, out var mapped))
                key = mapped;
        }

        if (Plugins.ScanlineTextures.TryGetValue(key, out var scan)) return scan.ModName;
        if (Plugins.HealthBarPrefabs.TryGetValue(key, out var hp)) return hp.ModName;
        if (Plugins.StaminaBarPrefabs.TryGetValue(key, out var stamina)) return stamina.ModName;
        if (Plugins.BatteryPrefabs.TryGetValue(key, out var battery)) return battery.ModName;
        if (Plugins.SlotPrefabs.TryGetValue(key, out var slot)) return slot.ModName;
        if (Plugins.ScanNodeSprites.TryGetValue(key, out var node)) return node.ModName;

        return "Unknown";
    }
}