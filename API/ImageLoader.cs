using BepInEx;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static LethalHUD.Plugins;

namespace LethalHUD.API;
internal static class ImageLoader
{
    internal static void LoadFromFolders()
    {
        string pluginsFolder = Path.Combine(Paths.BepInExRootPath, "plugins");

        if (!Directory.Exists(pluginsFolder))
            return;

        foreach (string dir in Directory.GetDirectories(pluginsFolder, "*", SearchOption.AllDirectories))
        {
            string name = Path.GetFileName(dir);

            if (name.Equals("Scanlines", System.StringComparison.OrdinalIgnoreCase))
                LoadScanlines(dir);

            if (name.Equals("Scannodes", System.StringComparison.OrdinalIgnoreCase))
                LoadScannodes(dir);
        }
    }

    private static void LoadScanlines(string folder)
    {
        string modName = BundleLoader.GetModName(folder);

        foreach (string file in Directory.GetFiles(folder))
        {
            if (!file.EndsWith(".png") && !file.EndsWith(".jpg"))
                continue;

            byte[] data = File.ReadAllBytes(file);

            Texture2D tex = new(2, 2);
            tex.LoadImage(data);

            string style = Path.GetFileNameWithoutExtension(file);

            if (style.EndsWith("_Outer") || style.EndsWith("_Inner"))
                continue;

            string uniqueName = $"[{modName}] {style}";

            if (!ScanlineTextures.ContainsKey(uniqueName))
            {
                ScanlineTextures.Add(uniqueName, tex);
                Loggers.Info($"Loaded Scanline (Folder): {uniqueName}");
            }
        }
    }

    private static void LoadScannodes(string folder)
    {
        string modName = BundleLoader.GetModName(folder);

        Dictionary<string, ScanNodeTextures> pairs = new();

        foreach (string file in Directory.GetFiles(folder))
        {
            if (!file.EndsWith(".png") && !file.EndsWith(".jpg"))
                continue;

            byte[] data = File.ReadAllBytes(file);

            Texture2D tex = new(2, 2);
            tex.LoadImage(data);

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            string name = Path.GetFileNameWithoutExtension(file);

            bool isOuter = name.EndsWith("_Outer");
            bool isInner = name.EndsWith("_Inner");

            if (!isOuter && !isInner)
                continue;

            string baseName = name.Replace("_Outer", "").Replace("_Inner", "");

            if (!pairs.ContainsKey(baseName))
                pairs[baseName] = new ScanNodeTextures();

            ScanNodeTextures pair = pairs[baseName];

            if (isOuter)
                pair.Outer = sprite;

            if (isInner)
                pair.Inner = sprite;

            pairs[baseName] = pair;
        }

        foreach (var pair in pairs)
        {
            if (pair.Value.Inner != null && pair.Value.Outer != null)
            {
                string uniqueName = $"[{modName}] {pair.Key}";

                ScanNodeSprites[uniqueName] = pair.Value;

                Loggers.Info($"Loaded Scannode (Folder): {uniqueName}");
            }
        }
    }
}