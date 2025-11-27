using LethalHUD.Compats;
using LethalHUD.Configs;
using LethalHUD.HUD;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using static LethalHUD.Enums;

namespace LethalHUD.Scan;
internal static class ScanController
{
    private const float ScanDuration = 1.3f;

    private static Texture2D _lastRecoloredTexture;

    private static MeshRenderer _scanRenderer;
    private static MeshRenderer ScanRenderer
    {
        get
        {
            if (_scanRenderer == null && HUDManager.Instance != null)
            {
                Animator anim = HUDManager.Instance.scanEffectAnimator;
                if (anim != null)
                    _scanRenderer = anim.GetComponent<MeshRenderer>();
            }
            return _scanRenderer;
        }
    }

    private static Volume _cachedScanVolume;
    private static Volume ScanVolume
    {
        get
        {
            if (_cachedScanVolume == null)
            {
                GameObject scanObject = GameObject.Find("Systems/Rendering/ScanSphere/ScanVolume");
                if (scanObject != null)
                    _cachedScanVolume = scanObject.GetComponent<Volume>();
            }
            return _cachedScanVolume;
        }
    }

    private static Vignette ScanVignette
    {
        get
        {
            if (ScanVolume == null) return null;
            if (ScanVolume.profile == null) return null;
            List<VolumeComponent> comps = ScanVolume.profile.components;
            if (comps == null) return null;
            return comps.OfType<Vignette>().FirstOrDefault();
        }
    }

    internal static Bloom ScanBloom
    {
        get
        {
            if (ScanVolume == null) return null;
            if (ScanVolume.profile == null) return null;
            List<VolumeComponent> comps = ScanVolume.profile.components;
            if (comps == null) return null;
            return comps.OfType<Bloom>().FirstOrDefault();
        }
    }

    private static float ScanProgress
    {
        get
        {
            if (HUDManager.Instance == null) return 0f;
            return (HUDManager.Instance.playerPingingScan + 1f) / ScanDuration;
        }
    }

    private static bool IsInspecting
    {
        get
        {
            if (StartOfRound.Instance == null) return false;
            if (StartOfRound.Instance.localPlayerController == null) return false;
            return StartOfRound.Instance.localPlayerController.IsInspectingItem;
        }
    }

    private static Material _scanMat;
    private static Material ScanMaterial
    {
        get
        {
            if (_scanMat == null && ScanRenderer != null)
                _scanMat = ScanRenderer.material;
            return _scanMat;
        }
    }

    private static void SetScanColorAlpha(float alpha)
    {
        if (ScanMaterial == null) return;

        Color color = ScanMaterial.color;
        color.a = alpha;
        ScanMaterial.color = color;
    }

    internal static void SetScanColor(Color? overrideColor = null)
    {
        if (IsInspecting) return;

        Color color = overrideColor.HasValue ? overrideColor.Value : ConfigHelper.GetScanColor();

        if (ModCompats.IsBetterScanVisionPresent)
            BetterScanVisionProxy.OverrideNightVisionColor();

        if (ScanMaterial != null)
            ScanMaterial.color = color;

        Vignette vignette = ScanVignette;
        if (vignette != null)
        {
            vignette.color.Override(color);
            UpdateVignetteIntensity();
        }

        Bloom bloom = ScanBloom;
        if (bloom != null)
        {
            bloom.tint.Override(color);
            UpdateScanTexture();
        }
    }

    internal static void UpdateScanAlpha()
    {
        if (HUDManager.Instance == null) return;
        if (ScanRenderer == null) return;
        if (IsInspecting) return;

        float baseAlpha = Plugins.ConfigEntries.Alpha.Value;
        float finalAlpha = baseAlpha;

        if (Plugins.ConfigEntries.FadeOut.Value && HUDManager.Instance.playerPingingScan > -1f)
            finalAlpha *= ScanProgress;

        SetScanColorAlpha(finalAlpha);
    }

    internal static void UpdateVignetteIntensity()
    {
        if (IsInspecting) return;

        Vignette vignette = ScanVignette;
        if (vignette != null)
            vignette.intensity.Override(Plugins.ConfigEntries.VignetteIntensity.Value);
    }

    internal static void UpdateScanTexture()
    {
        Bloom bloom = ScanBloom;
        if (bloom == null) return;
        if (IsInspecting) return;

        Enums.DirtIntensityHandlerByScanLine();

        Texture2D tex = GetSelectedTexture();
        if (tex == null) return;

        if (Plugins.ConfigEntries.RecolorScanLines.Value)
            RecolorAndApplyTexture(ConfigHelper.GetScanColor(), tex);
        else
            bloom.dirtTexture.Override(tex);
    }

    private static void RecolorAndApplyTexture(Color color, Texture2D baseTex)
    {
        Texture2D newTex = new Texture2D(baseTex.width, baseTex.height, TextureFormat.RGBA32, true);
        newTex.SetPixels(baseTex.GetPixels());
        HUDUtils.RecolorTexture(ref newTex, color);
        newTex.Apply(true, false);

        if (_lastRecoloredTexture != null)
            Object.Destroy(_lastRecoloredTexture);

        _lastRecoloredTexture = newTex;

        Bloom bloom = ScanBloom;
        if (bloom != null)
            bloom.dirtTexture.Override(_lastRecoloredTexture);
    }

    private static Texture2D GetSelectedTexture()
    {
        ScanLines selected = Plugins.ConfigEntries.SelectedScanlineMode.Value;

        if (Plugins.ScanlineTextures.TryGetValue(selected, out Texture2D tex) && tex != null)
            return tex;

        Loggers.Warning($"Scanline texture '{selected}' missing. Falling back to Default.");

        if (selected != ScanLines.Default &&
            Plugins.ScanlineTextures.TryGetValue(ScanLines.Default, out Texture2D fallback) &&
            fallback != null)
            return fallback;

        Loggers.Error("No scanline textures could be applied.");
        return null;
    }
}