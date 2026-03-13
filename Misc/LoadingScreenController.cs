using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static LethalHUD.Enums;
using static LethalHUD.Plugins;

namespace LethalHUD.Misc;

internal static class LoadingScreenController
{
    private static List<Sprite> _screenPool = new();
    private static readonly System.Random _rng = new(Environment.TickCount);
    internal static void RefreshPool()
    {
        _screenPool.Clear();

        if (!ConfigEntries.UseCustomLoadingScreens.Value)
            return;

        var mode = ConfigEntries.LoadingScreenMode.Value;

        switch (mode)
        {
            case LoadingScreenPackMode.AllPacks:
                _screenPool.AddRange(LoadingScreens.Values);
                break;

            case LoadingScreenPackMode.SelectedPacks:

                string[] packs = ConfigEntries.SelectedLoadingScreenPacks.Value
                    .Split(',')
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToArray();

                foreach (var pair in LoadingScreens)
                {
                    foreach (string pack in packs)
                    {
                        if (pair.Key.StartsWith($"[{pack}]"))
                        {
                            _screenPool.Add(pair.Value);
                            break;
                        }
                    }
                }

                break;

            case LoadingScreenPackMode.SinglePack:

                string packName = ConfigEntries.SelectedLoadingScreenPacks.Value.Trim();

                foreach (var pair in LoadingScreens)
                {
                    if (pair.Key.StartsWith($"[{packName}]"))
                        _screenPool.Add(pair.Value);
                }

                break;
        }
    }

    internal static void ApplyLoadingScreen(HUDManager hud)
    {
        if (hud.loadingDarkenScreen == null)
            return;

        if (!ConfigEntries.UseCustomLoadingScreens.Value || _screenPool.Count == 0)
        {
            hud.loadingDarkenScreen.sprite = null;
            hud.loadingDarkenScreen.color = new Color(0f, 0f, 0f, 0.9137255f);
            hud.loadingDarkenScreen.preserveAspect = false;

            return;
        }

        int index = _rng.Next(_screenPool.Count);

        hud.loadingDarkenScreen.sprite = _screenPool[index];
        hud.loadingDarkenScreen.color = new Color(1f, 1f, 1f, 0.25f);
        hud.loadingDarkenScreen.preserveAspect = false;
    }
}