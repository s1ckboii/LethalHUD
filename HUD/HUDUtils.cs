using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace LethalHUD.HUD;
internal static class HUDUtils
{
    #region Basic Helpers

    // Change ParseHexColor logic to the latter one but I'd need to look thru all base colors first to switch.
    // Maybe not? I remember doing something about this that backfired heavily
    internal static Color ParseHexColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return color;
        }

        Loggers.Warning($"Invalid HEX color: {hex}. Defaulting to original blue.");
        return new Color(0f, 0.047f, 1f, 1f);
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
    internal static Texture2D GetReadableTexture(Texture2D source)
    {
        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);

        Graphics.Blit(source, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readable.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        readable.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return readable;
    }
    internal static void RecolorTexture(ref Texture2D texture, Color color)
    {
        float colorIntensity = color.r + color.g + color.b;
        List<Color> pixels = [.. texture.GetPixels()];

        Loggers.Debug("ScanTexture pixel count: " + pixels.Count);
        for (int i = pixels.Count - 1; i >= 0; i--)
        {
            float pixelIntensity = pixels[i].r + pixels[i].g + pixels[i].b;
            if (pixelIntensity < 0.05f || pixels[i].a < 0.05f) continue;

            float intensityDiff = colorIntensity == 0f ? 0f : (pixelIntensity / colorIntensity);
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
                    VertexGradientLayout.Diagonal => (Mathf.InverseLerp(minX, maxX, vertices[vertIndex + v].x) + Mathf.InverseLerp(minY, maxY, vertices[vertIndex + v].y)) / 2f,
                    _ => Mathf.InverseLerp(minX, maxX, vertices[vertIndex + v].x)
                };

                float wave = Mathf.SmoothStep(0f, 1f, Mathf.Sin((t + time) * waveFrequency * Mathf.PI * 2f) * 0.5f + 0.5f);
                colors[vertIndex + v] = Color.Lerp(startColor, endColor, wave);
            }
        }

        tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }


    internal static void ApplyPulsingVertexGradient(TextMeshProUGUI tmpText, Color startColor, Color endColor, float time, VertexGradientLayout layout, float speed = 1f)
    {
        if (tmpText == null) return;

        tmpText.ForceMeshUpdate();
        TMP_TextInfo textInfo = tmpText.textInfo;
        if (textInfo.characterCount == 0) return;

        Bounds bounds = tmpText.mesh.bounds;
        float minX = bounds.min.x, maxX = bounds.max.x;
        float minY = bounds.min.y, maxY = bounds.max.y;

        float pulse = Mathf.SmoothStep(0f, 1f, Mathf.Sin(time * speed * Mathf.PI * 2f) * 0.5f + 0.5f);

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
                    VertexGradientLayout.Diagonal => (Mathf.InverseLerp(minX, maxX, vertices[vertIndex + v].x) + Mathf.InverseLerp(minY, maxY, vertices[vertIndex + v].y)) * 0.5f,
                    _ => Mathf.InverseLerp(minX, maxX, vertices[vertIndex + v].x)
                };

                float blend = Mathf.Lerp(t, 1f - t, pulse);

                colors[vertIndex + v] = Color.Lerp(startColor, endColor, blend);
            }
        }

        tmpText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
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
    internal static void ApplyRainbow(Image[] frames, Dictionary<Image, Image> fadeMap)
    {
        int count = frames.Length;
        float hueShift = Time.time * 0.15f;

        for (int i = 0; i < count; i++)
        {
            Image frame = frames[i];
            if (frame == null) continue;

            float hue = (hueShift + (float)i / count) % 1f;
            Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

            rainbowColor.a = frames[i].color.a;
            frames[i].color = rainbowColor;

            if (fadeMap != null && fadeMap.TryGetValue(frame, out Image fadeImg) && fadeImg != null)
            {
                Color fadeColor = rainbowColor;
                fadeColor.a = fadeImg.color.a;
                fadeImg.color = fadeColor;
            }
        }
    }

    internal static void ApplyWavyGradient(Image[] frames, Color startColor, Color endColor, Dictionary<Image, Image> fadeMap, float speed = 0.15f, float waveFrequency = 2f)
    {
        if (frames == null || frames.Length == 0) return;

        _gradientWaveTime = (_gradientWaveTime + Time.deltaTime * speed) % 1f;

        int count = frames.Length;
        for (int i = 0; i < count; i++)
        {
            Image frame = frames[i];
            if (frame == null) continue;

            float normalizedIndex = (float)i / (count - 1);
            float waveOffset = Mathf.SmoothStep(
                0f, 1f,
                Mathf.Sin((_gradientWaveTime + normalizedIndex * waveFrequency) * Mathf.PI * 2f) * 0.5f + 0.5f
            );

            Color interpolated = Color.Lerp(startColor, endColor, waveOffset).gamma;

            interpolated.a = frame.color.a;
            frame.color = interpolated;

            if (fadeMap != null && fadeMap.TryGetValue(frame, out Image fadeImg) && fadeImg != null)
            {
                Color fadeColor = interpolated;
                fadeColor.a = fadeImg.color.a;
                fadeImg.color = fadeColor;
            }
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
    #region LoadingScreen Helpers
    public static void ColorLoadingText(Transform root, string hexColor)
    {
        if (root == null) return;

        var loadText = root.Find("LoadText")?.GetComponent<TextMeshProUGUI>();
        var loadTextB = root.Find("LoadTextB")?.GetComponent<TextMeshProUGUI>();
        var textBG = root.Find("TextBG")?.GetComponent<Image>();

        Color baseColor = ParseHexColor(hexColor, Color.gray);

        if (loadText != null)
        {
            if (loadText.fontMaterial != null)
            {
                var mat = loadText.fontMaterial;

                Color c = baseColor;
                c.a = loadText.color.a;

                mat.SetColor("_FaceColor", c);
            }
        }

        if (loadTextB != null)
        {
            Color c = baseColor;
            c.a = loadTextB.color.a;
            loadTextB.color = c;
        }

        if (textBG != null)
        {
            Color bg = Color.Lerp(baseColor, Color.black, 0.6f);
            bg.a = textBG.color.a;
            textBG.color = bg;
        }
    }
    #endregion
    #region HP and PlayerRedCanvas Helpers
    internal static readonly Color HP_Yellow = new(1f, 1f, 0.2f);
    internal static readonly Color HP_Orange = new(1f, 0.5f, 0.1f);
    internal static readonly Color HP_BrightRed = new(1f, 0.1f, 0.1f);
    internal static readonly Color HP_DarkRed = new(0.4f, 0f, 0f);

    // Overheal stuff (100+ hp)
    internal static readonly Color HP_Blue = new(0.2f, 0.2f, 1f);

    internal static Color GetHPColor(int hp)
    {
        if (hp > 100)
        {
            float t = Mathf.Clamp01((hp - 100f) / 100f);
            return Color.Lerp(PlayerHPDisplay.FullHPColor, HP_Blue, t);
        }

        float hp01 = Mathf.Clamp01(hp / 100f);

        if (hp01 >= 0.7f)
        {
            float t = Mathf.InverseLerp(0.7f, 1f, hp01);
            return Color.Lerp(HP_Yellow, PlayerHPDisplay.FullHPColor, t);
        }
        else if (hp01 >= 0.5f)
        {
            float t = Mathf.InverseLerp(0.5f, 0.7f, hp01);
            return Color.Lerp(HP_Orange, HP_Yellow, t);
        }
        else if (hp01 >= 0.3f)
        {
            float t = Mathf.InverseLerp(0.3f, 0.5f, hp01);
            return Color.Lerp(HP_BrightRed, HP_Orange, t);
        }
        else
        {
            float t = Mathf.InverseLerp(0f, 0.3f, hp01);
            return Color.Lerp(HP_DarkRed, HP_BrightRed, t);
        }
    }
    #endregion
}