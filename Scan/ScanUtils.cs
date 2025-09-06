using UnityEngine;
using System.Linq;

namespace LethalHUD.Scan;
public class ScanUtils
{
    public static void RecolorTexture(ref Texture2D texture, Color color)
    {
        var colorIntensity = color.r + color.g + color.b;
        var pixels = texture.GetPixels().ToList();

        Loggers.Debug("ScanTexture pixel count: " + pixels.Count);
        for (int i = pixels.Count - 1; i >= 0; i--)
        {
            var pixelIntensity = pixels[i].r + pixels[i].g + pixels[i].b;
            if (pixelIntensity < 0.05f || pixels[i].a < 0.05f) continue;

            var intensityDiff = colorIntensity == 0f ? 0f : (pixelIntensity / colorIntensity);
            pixels[i] = new Color(color.r * intensityDiff, color.g * intensityDiff, color.b * intensityDiff);
        }
        texture.SetPixels([.. pixels]);
    }
}