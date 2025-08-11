using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.HUD;
internal static class HUDUtils
{

    #region
    public static Color ParseHexColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            return new Color(color.r, color.g, color.b);
        }

        Plugins.Logger.LogWarning($"Invalid HEX color: {hex}. Defaulting to original blue.");
        return new Color(0f, 0.047f, 1f);
    }
    public static bool HasCustomGradient(string a, string b)
    {
        return !string.IsNullOrWhiteSpace(a)
            && !string.IsNullOrWhiteSpace(b)
            && !string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
    }
    #endregion

    #region ChatController Helpers
    public static string ApplyStaticGradient(string input, Color startColor, Color endColor, float minBrightness = 0.15f)
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
    public static void ApplyVertexGradient(TextMeshProUGUI tmpText, Color startColor, Color endColor, float time, float waveFrequency = 1.5f)
    {
        if (tmpText == null)
            return;

        float leftWave = Mathf.SmoothStep(0f, 1f, Mathf.Sin((time + 0f) * waveFrequency * Mathf.PI * 2f) * 0.5f + 0.5f);
        float rightWave = Mathf.SmoothStep(0f, 1f, Mathf.Sin((time + 1f) * waveFrequency * Mathf.PI * 2f) * 0.5f + 0.5f);

        Color leftColor = Color.Lerp(startColor, endColor, leftWave);
        Color rightColor = Color.Lerp(startColor, endColor, rightWave);

        VertexGradient vertexGradient = new VertexGradient(leftColor, leftColor, rightColor, rightColor);

        tmpText.colorGradient = vertexGradient;
        tmpText.ForceMeshUpdate();
    }

    public static void ApplyRainbowGradient(TextMeshProUGUI tmpText, float time)
    {
        if (tmpText == null)
            return;

        float speed = 0.5f;
        float hueShift = (time * speed) % 1f;

        Color topLeft = Color.HSVToRGB((hueShift + 0f) % 1f, 1f, 1f);
        Color topRight = Color.HSVToRGB((hueShift + 0.33f) % 1f, 1f, 1f);
        Color bottomLeft = Color.HSVToRGB((hueShift + 0.66f) % 1f, 1f, 1f);
        Color bottomRight = Color.HSVToRGB((hueShift + 0.99f) % 1f, 1f, 1f);

        tmpText.colorGradient = new VertexGradient(topLeft, topRight, bottomLeft, bottomRight);
        tmpText.ForceMeshUpdate();
    }
    #endregion
    #region Compass Helpers
    private static RawImage CompassImage => CompassController.CompassImage;
    private static float _rainbowTime = 0f;
    private static float _gradientWaveTime = 0f;
    public static void ApplyCompassRainbow()
    {
        if (CompassImage == null) return;

        _rainbowTime += Time.deltaTime * 0.15f;
        float hue = _rainbowTime % 1f;
        Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

        rainbowColor.a = CompassImage.color.a;
        CompassImage.color = rainbowColor;
    }
    public static void ApplyCompassWavyGradient(Color startColor, Color endColor)
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
    public static void ApplyRainbow(Image[] frames)
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

    public static void ApplyWavyGradient(Image[] frames, Color startColor, Color endColor, float speed = 0.15f, float waveFrequency = 2f)
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
    public static Color GetGradientColor(Color baseColor, float fillAmount)
    {
        Color.RGBToHSV(baseColor, out float h, out float s, out float v);

        float hueShift = Mathf.Lerp(0f, 120f / 360f, 1f - fillAmount);

        float newH = (h + hueShift) % 2f;

        return Color.HSVToRGB(newH, s, v);
    }
    public static Color GetShadeColor(Color baseColor, float fillAmount)
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
}