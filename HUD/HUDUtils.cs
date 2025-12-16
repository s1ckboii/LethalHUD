using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class HUDUtils
{
    #region Basic Helpers

    // Change ParseHexColor logic to the latter one but I'd need to look thru all base colors first to switch.

    internal static Color ParseHexColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return new Color(color.r, color.g, color.b);
        }

        Loggers.Warning($"Invalid HEX color: {hex}. Defaulting to original blue.");
        return new Color(0f, 0.047f, 1f);
    }

    internal static Color ParseHexColor(string hex, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return fallback;

        if (ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;

        Loggers.Warning($"Invalid HEX color: {hex}. Defaulting to original blue.");
        return fallback;
    }

    internal static bool HasCustomGradient(string a, string b)
    {
        return !string.IsNullOrWhiteSpace(a)
            && !string.IsNullOrWhiteSpace(b)
            && !string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
    }
    #endregion

    #region Scan Helpers
    internal static void RecolorTexture(ref Texture2D texture, Color color)
    {
        Single colorIntensity = color.r + color.g + color.b;
        List<Color> pixels = [.. texture.GetPixels()];

        Loggers.Debug("ScanTexture pixel count: " + pixels.Count);
        for (int i = pixels.Count - 1; i >= 0; i--)
        {
            Single pixelIntensity = pixels[i].r + pixels[i].g + pixels[i].b;
            if (pixelIntensity < 0.05f || pixels[i].a < 0.05f) continue;

            Single intensityDiff = colorIntensity == 0f ? 0f : (pixelIntensity / colorIntensity);
            pixels[i] = new Color(color.r * intensityDiff, color.g * intensityDiff, color.b * intensityDiff);
        }
        texture.SetPixels([.. pixels]);
    }
    #endregion
    #region ChatController Helpers
    internal static string ApplyStaticGradient(string input, Color startColor, Color endColor, float minBrightness = 0.15f)
    {
        string result = "";
        int len = input.Length;

        for (int i = 0; i < len; i++)
        {
            float t = (float)i / Mathf.Max(1, len - 1);
            Color lerped = Color.Lerp(startColor, endColor, t);

            float brightness = 0.299f * lerped.r + 0.587f * lerped.g + 0.114f * lerped.b;
            if (brightness < minBrightness)
            {
                float boost = Mathf.Clamp01((minBrightness - brightness) * 0.5f);
                lerped = Color.Lerp(lerped, Color.white, boost);
            }

            string hex = ColorUtility.ToHtmlStringRGB(lerped);
            result += $"<color=#{hex}>{input[i]}</color>";
        }

        return result;
    }

    internal static void ApplyStaticVertexGradient(TextMeshProUGUI tmpText, Color startColor, Color endColor, VertexGradientLayout layout)
    {
        if (tmpText == null) return;

        tmpText.ForceMeshUpdate();
        TMP_TextInfo textInfo = tmpText.textInfo;
        if (textInfo.characterCount == 0) return;

        Bounds bounds = tmpText.mesh.bounds;
        float minX = bounds.min.x, maxX = bounds.max.x;
        float minY = bounds.min.y, maxY = bounds.max.y;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vertIndex = charInfo.vertexIndex;

            Vector3[] vertices = textInfo.meshInfo[matIndex].vertices;
            Color32[] colors = textInfo.meshInfo[matIndex].colors32;

            for (int v = 0; v < 4; v++)
            {
                float t = layout switch
                {
                    VertexGradientLayout.Horizontal => Mathf.InverseLerp(minX, maxX, vertices[vertIndex + v].x),
                    VertexGradientLayout.Vertical => Mathf.InverseLerp(minY, maxY, vertices[vertIndex + v].y),
                    VertexGradientLayout.Diagonal => (Mathf.InverseLerp(minX, maxX, vertices[vertIndex + v].x) +
                                                       Mathf.InverseLerp(minY, maxY, vertices[vertIndex + v].y)) / 2f,
                    _ => Mathf.InverseLerp(minX, maxX, vertices[vertIndex + v].x)
                };

                colors[vertIndex + v] = Color.Lerp(startColor, endColor, t);
            }
        }

        tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    internal static void ApplyWaveVertexGradient(TextMeshProUGUI tmpText, Color startColor, Color endColor, float time, VertexGradientLayout layout, float waveFrequency = 1.5f)
    {
        if (tmpText == null) return;

        tmpText.ForceMeshUpdate();
        TMP_TextInfo textInfo = tmpText.textInfo;
        if (textInfo.characterCount == 0) return;

        Bounds bounds = tmpText.mesh.bounds;
        float minX = bounds.min.x, maxX = bounds.max.x;
        float minY = bounds.min.y, maxY = bounds.max.y;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            int vertIndex = charInfo.vertexIndex;

            Vector3[] vertices = textInfo.meshInfo[matIndex].vertices;
            Color32[] colors = textInfo.meshInfo[matIndex].colors32;

            for (int v = 0; v < 4; v++)
            {
                float t = layout switch
                {
                    VertexGradientLayout.Horizontal => Mathf.InverseLerp(minX, maxX, vertices[vertIndex + v].x),
                    VertexGradientLayout.Vertical => Mathf.InverseLerp(minY, maxY, vertices[vertIndex + v].y),
                    VertexGradientLayout.Diagonal => (Mathf.InverseLerp(minX, maxX, vertices[vertIndex + v].x) +
                                                       Mathf.InverseLerp(minY, maxY, vertices[vertIndex + v].y)) / 2f,
                    _ => Mathf.InverseLerp(minX, maxX, vertices[vertIndex + v].x)
                };

                float wave = Mathf.SmoothStep(0f, 1f, Mathf.Sin((t + time) * waveFrequency * Mathf.PI * 2f) * 0.5f + 0.5f);
                colors[vertIndex + v] = Color.Lerp(startColor, endColor, wave);
            }
        }

        tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }


    internal static void ApplyPulsingVertexGradient(TextMeshProUGUI tmpText, Color startColor, Color endColor, float time, float speed = 1f)
    {
        if (tmpText == null)
            return;

        float phaseOffset = 0.2f;

        float pulseTop = Mathf.SmoothStep(0f, 1f, Mathf.Sin((time * speed + 0f) * Mathf.PI * 2f) * 0.5f + 0.5f);
        float pulseBottom = Mathf.SmoothStep(0f, 1f, Mathf.Sin((time * speed + phaseOffset) * Mathf.PI * 2f) * 0.5f + 0.5f);

        Color topColor = Color.Lerp(startColor, endColor, pulseTop);
        Color bottomColor = Color.Lerp(startColor, endColor, pulseBottom);

        tmpText.colorGradient = new VertexGradient(topColor, topColor, bottomColor, bottomColor);
        tmpText.ForceMeshUpdate();
    }

    internal static void ApplyRainbowGradient(TextMeshProUGUI tmpText, float time)
    {
        if (tmpText == null)
            return;

        float speed = 0.5f;
        float hueShift = (time * speed) % 1f;

        Color topLeft = Color.HSVToRGB((hueShift + 0f) % 1f, 1f, 1f);
        Color topRight = Color.HSVToRGB((hueShift + 0.33f) % 1f, 1f, 1f);
        Color bottomLeft = Color.HSVToRGB((hueShift + 0.66f) % 1f, 1f, 1f);
        Color bottomRight = Color.HSVToRGB((hueShift + 0.99f) % 1f, 1f, 1f);

        tmpText.colorGradient = new(topLeft, topRight, bottomLeft, bottomRight);
        tmpText.ForceMeshUpdate();
    }
    #endregion
    #region Compass Helpers
    private static RawImage CompassImage => CompassController.CompassImage;
    private static float _rainbowTime = 0f;
    private static float _gradientWaveTime = 0f;
    internal static void ApplyCompassRainbow()
    {
        if (CompassImage == null) return;

        _rainbowTime += Time.deltaTime * 0.15f;
        float hue = _rainbowTime % 1f;
        Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

        rainbowColor.a = CompassImage.color.a;
        CompassImage.color = rainbowColor;
    }
    internal static void ApplyCompassWavyGradient(Color startColor, Color endColor)
    {
        if (CompassImage == null) return;

        _gradientWaveTime += Time.deltaTime * 0.15f;
        float waveOffset = Mathf.SmoothStep(0f, 1f, Mathf.Sin(_gradientWaveTime * Mathf.PI * 2f) * 0.5f + 0.5f); // *magic numbers*

        Color interpolated = Color.Lerp(startColor, endColor, waveOffset).gamma;
        interpolated.a = CompassImage.color.a;
        CompassImage.color = interpolated;
    }
    #endregion
    #region InventoryFrames Helpers
    internal static void ApplyRainbow(Image[] frames)
    {
        int count = frames.Length;
        float hueShift = Time.time * 0.15f;

        for (int i = 0; i < count; i++)
        {
            float hue = (hueShift + (float)i / count) % 1f;
            Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

            if (frames[i] != null)
                frames[i].color = rainbowColor;
        }
    }

    internal static void ApplyWavyGradient(Image[] frames, Color startColor, Color endColor, float speed = 0.15f, float waveFrequency = 2f)
    {
        if (frames == null || frames.Length == 0)
            return;

        _gradientWaveTime = (_gradientWaveTime + Time.deltaTime * speed) % 1f;

        int count = frames.Length;
        for (int i = 0; i < count; i++)
        {
            float normalizedIndex = (float)i / (count - 1);
            float waveOffset = Mathf.SmoothStep(0f, 1f, Mathf.Sin((_gradientWaveTime + normalizedIndex * waveFrequency) * Mathf.PI * 2f) * 0.5f + 0.5f);

            Color interpolated = Color.Lerp(startColor, endColor, waveOffset).gamma;
            frames[i].color = interpolated;
        }
    }
    #endregion
    #region SprintMeter Helpers
    internal static Color GetGradientColor(Color baseColor, float fillAmount)
    {
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);

        float hueShift = Mathf.Lerp(0f, 120f / 360f, 1f - fillAmount);

        float newH = (h + hueShift) % 2f;

        return Color.HSVToRGB(newH, s, v);
    }
    internal static Color GetShadeColor(Color baseColor, float fillAmount)
    {
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);

        if (v < 0.4f)
        {
            if (fillAmount > 0.65f)
            {
                return baseColor;
            }
            else if (fillAmount > 0.55f)
            {
                float phaseT = Mathf.InverseLerp(0.65f, 0.55f, fillAmount);
                float newV = Mathf.Lerp(v, 1f, phaseT);
                return Color.HSVToRGB(h, s, Mathf.Clamp01(newV));
            }
            else if (fillAmount > 0.4f)
            {
                float phaseT = Mathf.InverseLerp(0.55f, 0.4f, fillAmount);
                Color brightBase = Color.HSVToRGB(h, s, 1f);
                return Color.Lerp(brightBase, Color.red, phaseT);
            }
            else
            {
                return Color.red;
            }
        }
        else
        {
            v = Mathf.Lerp(0.5f, v, fillAmount);
            return Color.HSVToRGB(h, s, v);
        }
    }
    #endregion
    #region Weight Helper
    internal static Color GetWeightColor(float normalizedWeight)
    {
        ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.WeightStarterColor.Value, out Color starter);

        Color brightRed = new(1f, 0f, 0f);
        Color darkRed = new(0.5f, 0f, 0f);

        if (normalizedWeight <= 0.5f)
        {
            float t = normalizedWeight / 0.5f;
            return Color.Lerp(starter, brightRed, t);
        }
        else
        {
            float t = (normalizedWeight - 0.5f) / 0.5f;
            return Color.Lerp(brightRed, darkRed, t);
        }
    }

    internal static VertexGradient GetWeightGradient(float normalizedWeight)
    {
        ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.WeightStarterColor.Value, out Color starter);

        Color brightRed = new(1f, 0f, 0f);
        Color darkRed = new(0.5f, 0f, 0f);

        Color color;

        if (normalizedWeight <= 0.5f)
        {
            float t = normalizedWeight / 0.5f;
            color = Color.Lerp(starter, brightRed, t);
        }
        else
        {
            float t = (normalizedWeight - 0.5f) / 0.5f;
            color = Color.Lerp(brightRed, darkRed, t);
        }

        return new VertexGradient(color);
    }

    internal static string ApplyRichTextGradient(string text, Color startColor, Color endColor)
    {
        char[] chars = text.ToCharArray();
        int length = chars.Length;
        System.Text.StringBuilder sb = new();

        for (int i = 0; i < length; i++)
        {
            float t = length > 1 ? i / (float)(length - 1) : 0f;
            Color c = Color.Lerp(startColor, endColor, t);
            string hex = ColorUtility.ToHtmlStringRGB(c);
            sb.Append($"<color=#{hex}>{chars[i]}</color>");
        }

        return sb.ToString();
    }
    #endregion
    #region PlanetInfo Helpers
    private static float _loadingOffset;
    public static void AnimateLoadingText(TextMeshProUGUI text, string hexColor)
    {
        if (text == null) return;

        Color baseColor = ParseHexColor(hexColor, Color.grey);

        text.ForceMeshUpdate();
        TMP_TextInfo textInfo = text.textInfo;
        int characterCount = textInfo.characterCount;
        if (characterCount == 0) return;

        _loadingOffset += Time.deltaTime * 2.5f;
        if (_loadingOffset > 1f) _loadingOffset -= 1f;

        for (int i = 0; i < characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int meshIndex = charInfo.materialReferenceIndex;
            Color32[] vertexColors = textInfo.meshInfo[meshIndex].colors32;

            float t = ((float)i / characterCount + _loadingOffset) % 1f;
            Color color;

            color = Color.Lerp(Color.gray, baseColor, Mathf.PingPong(t * 2f, 1f));

            vertexColors[vertexIndex + 0] = color;
            vertexColors[vertexIndex + 1] = color;
            vertexColors[vertexIndex + 2] = color;
            vertexColors[vertexIndex + 3] = color;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
    #endregion
}