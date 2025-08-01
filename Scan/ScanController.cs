using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LethalHUD.Scan;

public static class ScanController
{
    private const float ScanDuration = 1.3f;

    private static Texture2D _lastRecoloredTexture;

    private static bool _randomColorApplied = false;

    public static void RandomColoring()
    {
        if (!Configs.Instance.RandomColor.Value || !HUDManager.Instance.CanPlayerScan())
            return;

        if (!_randomColorApplied && ScanProgress <= 0f)
        {
            Color randomColor = new(Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), Random.Range(0.2f, 1f), Random.Range(0.26f, 1f));
            SetScanColor(randomColor);
            _randomColorApplied = true;
        }
    }

    public static void RandomColoringnt()
    {
        if (ScanProgress <= 0f)
            _randomColorApplied = false;
    }

    private static MeshRenderer ScanRenderer =>
        HUDManager.Instance?.scanEffectAnimator?.GetComponent<MeshRenderer>();

    private static Volume ScanVolume =>
        Object.FindObjectsByType<Volume>(FindObjectsSortMode.None)
            ?.FirstOrDefault(v => v?.profile?.name?.StartsWith("ScanVolume") ?? false);

    private static Vignette ScanVignette =>
        ScanVolume?.profile?.components?.OfType<Vignette>().FirstOrDefault();

    public static Bloom ScanBloom =>
        ScanVolume?.profile?.components?.OfType<Bloom>().FirstOrDefault();

    public static float ScanProgress =>
        1f / ScanDuration * (HUDManager.Instance.playerPingingScan + 1f);

    public static void SetScanColorAlpha(float alpha)
    {
        if (ScanRenderer?.material == null) return;
        var color = ScanRenderer.material.color;
        color.a = alpha;
        ScanRenderer.material.color = color;
    }

    public static void SetScanColor(Color? overrideColor = null)
    {
        var color = overrideColor ?? ParseScanColor();

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

    public static void UpdateVignetteIntensity()
    {
        ScanVignette?.intensity.Override(Configs.Instance.VignetteIntensity.Value);
    }

    public static void UpdateScanTexture()
    {
        if (ScanBloom == null) return;
        ScanlinesEnums.DirtIntensityHandlerByScanLine();
        Texture2D tex = GetSelectedTexture();
        if (tex == null) return;
        if (Configs.Instance.RecolorScanLines.Value)
        {
            RecolorAndApplyTexture(ParseScanColor(), tex);
        }
        else
        {
            ScanBloom.dirtTexture.Override(tex);
        }
    }

    private static void RecolorAndApplyTexture(Color color, Texture2D baseTex)
    {
        var newTex = new Texture2D(baseTex.width, baseTex.height);
        newTex.SetPixels(baseTex.GetPixels());
        newTex.Apply(false);

        Utils.RecolorTexture(ref newTex, color);

        if (_lastRecoloredTexture != null)
            Object.Destroy(_lastRecoloredTexture);

        _lastRecoloredTexture = Object.Instantiate(newTex);
        ScanBloom.dirtTexture.Override(_lastRecoloredTexture);
    }

    private static Texture2D GetSelectedTexture()
    {
        var selected = Configs.Instance.SelectedScanlineMode.Value;

        if (Plugins.ScanlineTextures.TryGetValue(selected, out var tex) && tex != null)
            return tex;

        Plugins.mls.LogWarning($"Scanline texture '{selected}' missing. Falling back to Default.");

        if (selected != ScanlinesEnums.ScanLines.Default &&
            Plugins.ScanlineTextures.TryGetValue(ScanlinesEnums.ScanLines.Default, out var fallback) && fallback != null)
            return fallback;

        Plugins.mls.LogError("No scanline textures could be applied.");
        return null;
    }

    private static Color ParseScanColor()
    {
        ColorUtility.TryParseHtmlString(Configs.Instance.ScanColor.Value, out var color);
        return new Color(color.r, color.g, color.b, Configs.Instance.Alpha.Value);
    }
}

// Issue: when RecolorScanline is enabled, Circles and Square textures are gone