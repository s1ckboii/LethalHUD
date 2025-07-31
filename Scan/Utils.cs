using UnityEngine;
using System.Linq;

namespace LethalHUD.Scan;
public class Utils
{// https://support.unity.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures
    public static Texture MakeTextureReadable(Texture original)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(
                original.width,
                original.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);


        Graphics.Blit(original, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;
        Texture2D textureCopy = new(original.width, original.height);
        textureCopy.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        textureCopy.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);
        return textureCopy;
    }
    public static void RecolorTexture(ref Texture2D texture, Color color)
    {
        var colorIntensity = color.r + color.g + color.b;
        var pixels = texture.GetPixels().ToList();

        Plugins.mls.LogDebug("ScanTexture pixel count: " + pixels.Count);
        for (int i = pixels.Count - 1; i >= 0; i--)
        {
            var pixelIntensity = pixels[i].r + pixels[i].g + pixels[i].b;
            if (pixelIntensity < 0.05f || pixels[i].a < 0.05f) continue;

            var intensityDiff = colorIntensity == 0f ? 0f : (pixelIntensity / colorIntensity);
            pixels[i] = new Color(color.r * intensityDiff, color.g * intensityDiff, color.b * intensityDiff);
        }

        texture.SetPixels(pixels.ToArray());
    }
}
