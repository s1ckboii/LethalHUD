using LethalHUD.Compats;
using LethalHUD.Configs;
using LethalHUD.HUD;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LethalHUD.Scan;
internal static class ScanController
{
    private const float ScanDuration = 1.3f;

    private static Texture2D _lastRecoloredTexture;

    private static MeshRenderer ScanRenderer =>
        HUDManager.Instance?.scanEffectAnimator?.GetComponent<MeshRenderer>();

    private static Volume _cachedScanVolume;
    private static Volume ScanVolume
    {
        get
        {
            if (_cachedScanVolume == null)
            {
                GameObject scanObject = GameObject.Find("Systems/Rendering/ScanSphere/ScanVolume");
                _cachedScanVolume = scanObject?.GetComponent<Volume>();
            }
            return _cachedScanVolume;
        }
    }

    private static Vignette ScanVignette => ScanVolume?.profile?.components?.OfType<Vignette>().FirstOrDefault();
    internal static Bloom ScanBloom => ScanVolume?.profile?.components?.OfType<Bloom>().FirstOrDefault();

    private static float ScanProgress =>
        1f / ScanDuration * (HUDManager.Instance.playerPingingScan + 1f);

    private static bool IsInspecting =>
        StartOfRound.Instance?.localPlayerController?.IsInspectingItem ?? false;

    private static void SetScanColorAlpha(float alpha)
    {
        if (ScanRenderer?.material == null) return;
        Color color = ScanRenderer.material.color;
        color.a = alpha;
        ScanRenderer.material.color = color;
    }

    internal static void SetScanColor(Color? overrideColor = null)
    {
        if (IsInspecting) return; // Skip overlay during inspection

        Color color = overrideColor ?? ConfigHelper.GetScanColor();

        if (ModCompats.IsBetterScanVisionPresent)
            BetterScanVisionProxy.OverrideNightVisionColor();

        if (ScanRenderer?.material != null)
            ScanRenderer.material.color = color;

        if (ScanVignette != null)
        {
            ScanVignette.color.Override(color);
            UpdateVignetteIntensity();
        }

        if (ScanBloom != null)
        {
            ScanBloom.tint.Override(color);
            UpdateScanTexture();
        }
    }

    internal static void UpdateScanAlpha()
    {
        if (HUDManager.Instance == null || ScanRenderer == null || IsInspecting) return;

        float baseAlpha = Plugins.ConfigEntries.Alpha.Value;
        float finalAlpha = baseAlpha;

        if (Plugins.ConfigEntries.FadeOut.Value && HUDManager.Instance.playerPingingScan > -1f)
            finalAlpha *= ScanProgress;

        SetScanColorAlpha(finalAlpha);
    }

    internal static void UpdateVignetteIntensity()
    {
        if (IsInspecting) return;
        ScanVignette?.intensity.Override(Plugins.ConfigEntries.VignetteIntensity.Value);
    }

    internal static void UpdateScanTexture()
    {
        if (ScanBloom == null || IsInspecting) return;

        Enums.DirtIntensityHandlerByScanLine();

        Texture2D tex = GetSelectedTexture();
        if (tex == null) return;

        if (Plugins.ConfigEntries.RecolorScanLines.Value)
            RecolorAndApplyTexture(ConfigHelper.GetScanColor(), tex);
        else
            ScanBloom.dirtTexture.Override(tex);
    }

    private static void RecolorAndApplyTexture(Color color, Texture2D baseTex)
    {
        Texture2D newTex = new(baseTex.width, baseTex.height, TextureFormat.RGBA32, true);
        newTex.SetPixels(baseTex.GetPixels());
        HUDUtils.RecolorTexture(ref newTex, color);
        newTex.Apply(true, false);

        if (_lastRecoloredTexture != null)
            Object.Destroy(_lastRecoloredTexture);

        _lastRecoloredTexture = newTex;
        ScanBloom.dirtTexture.Override(_lastRecoloredTexture);
    }

    private static Texture2D GetSelectedTexture()
    {
        var selected = Plugins.ConfigEntries.SelectedScanlineMode.Value;

        if (Plugins.ScanlineTextures.TryGetValue(selected, out Texture2D tex) && tex != null)
            return tex;

        Loggers.Warning($"Scanline texture '{selected}' missing. Falling back to Default.");

        if (selected != Enums.ScanLines.Default &&
            Plugins.ScanlineTextures.TryGetValue(Enums.ScanLines.Default, out var fallback) && fallback != null)
            return fallback;

        Loggers.Error("No scanline textures could be applied.");
        return null;
    }
}