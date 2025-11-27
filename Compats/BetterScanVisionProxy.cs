using LethalHUD.Configs;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace LethalHUD.Compats;
internal class BetterScanVisionProxy
{
    private static FieldInfo _nightVisionField;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void OverrideNightVisionColor()
    {
        if (!ModCompats.IsBetterScanVisionPresent) return;

        if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
            return;

        if (_nightVisionField == null)
        {
            Type t = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType("BetterScanVision.PlayerControllerBPatches"))
                .FirstOrDefault(x => x != null);

            if (t == null)
            {
                Loggers.Warning("[BetterScanVisionProxy] Failed to find PlayerControllerBPatches type.");
                return;
            }

            _nightVisionField = t.GetField("nightVision", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        if (_nightVisionField != null)
        {
            Light light = _nightVisionField.GetValue(null) as Light;
            if (light != null)
            {
                light.color = ConfigHelper.GetScanColor();
            }
        }
    }
}