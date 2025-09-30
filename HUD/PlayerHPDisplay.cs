using GameNetcodeStuff;
using LethalHUD.Compats;
using LethalHUD.Configs;
using TMPro;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
public static class PlayerHPDisplay
{
    private static TextMeshProUGUI hpText;
    private static PlayerControllerB localPlayer;
    private static GameObject hpObj;

    private static float shakeTimer = 0f;
    private static readonly float shakeDuration = 0.3f;
    private static readonly float hitShakeIntensity = 5f;
    private static readonly float criticalMaxShakeIntensity = 1.5f;

    private static readonly float sizeBump = 1.2f;
    private static readonly float sizeLerpSpeed = 3f;
    private static Vector2 basePosition;
    private static float BaseFontSize => Plugins.ConfigEntries.HealthSize.Value;
    private static float HealthRotation => Plugins.ConfigEntries.HealthRotation.Value;

    private static readonly float plainSizeMult = 1f;
    private static readonly float percentSizeMult = 0.7f;
    private static readonly float labelSizeMult = 0.65f;

    internal static Color FullHPColor => HUDUtils.ParseHexColor(Plugins.ConfigEntries.HealthColor.Value);
    private static Color midHPColor = new(1f, 0.3f, 0.3f);
    private static Color lowHPColor = new(0.6f, 0f, 0f);

    public static void Init()
    {
        if (ModCompats.IsEladsHUDPresent) return;
        HUDManager hud = HUDManager.Instance;
        if (hpText != null) return;

        localPlayer = GameNetworkManager.Instance.localPlayerController;

        hpObj = new("PlayerHPDisplay");
        GameObject tpc = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/");
        hpObj.transform.SetParent(tpc.transform, false);
        hpObj.transform.localRotation = Quaternion.Euler(0f, 0f, HealthRotation);

        hpText = hpObj.AddComponent<TextMeshProUGUI>();
        hpText.font = hud.HUDQuotaNumerator.font;
        hpText.fontSize = BaseFontSize;
        hpText.alignment = TextAlignmentOptions.Left;
        hpText.color = FullHPColor;

        hpText.enableVertexGradient = false;
        hpText.enableWordWrapping = false;
        hpText.enableKerning = false;
        hpText.fontSharedMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
        hpText.fontSharedMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0f);

        RectTransform rect = hpText.rectTransform;
        rect.anchoredPosition = new Vector2(Plugins.ConfigEntries.HPIndicatorX.Value, Plugins.ConfigEntries.HPIndicatorY.Value);
        rect.sizeDelta = new Vector2(200, 50);

        basePosition = rect.anchoredPosition;
    }

    public static void UpdateNumber()
    {
        if (hpObj == null || hpText == null) return;

        if (hpObj.activeSelf)
        {
            if (!Plugins.ConfigEntries.HealthIndicator.Value)
            {
                hpObj.SetActive(false);
            }
        }
        else
        {
            if (Plugins.ConfigEntries.HealthIndicator.Value)
            {
                hpObj.SetActive(true);
            }
        }

        if (ModCompats.IsEladsHUDPresent) return;
        if (hpText == null) return;
        if (localPlayer == null) localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (localPlayer == null) return;

        hpObj.transform.localRotation = Quaternion.Euler(0f, 0f, HealthRotation);
        basePosition = new Vector2(Plugins.ConfigEntries.HPIndicatorX.Value, Plugins.ConfigEntries.HPIndicatorY.Value);

        int hp = localPlayer.health;

        hpText.text = Plugins.ConfigEntries.HealthFormat.Value switch
        {
            HPDisplayMode.Percent => $"{hp} %",
            HPDisplayMode.Label => $"{hp} HP",
            _ => $"{hp}",
        };

        float intensity = 0f;
        if (shakeTimer > 0f)
        {
            intensity = hitShakeIntensity;
            shakeTimer -= Time.deltaTime;
        }
        else if (hp < 20)
        {
            intensity = criticalMaxShakeIntensity * (20 - hp) / 20f;
        }

        RectTransform rect = hpText.rectTransform;
        rect.anchoredPosition = basePosition + Random.insideUnitCircle * intensity;

        float formatMult = Plugins.ConfigEntries.HealthFormat.Value switch
        {
            HPDisplayMode.Percent => percentSizeMult,
            HPDisplayMode.Label => labelSizeMult,
            _ => plainSizeMult
        };

        float targetSize = BaseFontSize * formatMult;

        if (shakeTimer > 0f)
        {
            targetSize *= sizeBump;
        }
        else if (hp < 20)
        {
            targetSize *= (1f + 0.1f * (20 - hp) / 20f);
        }

        hpText.fontSize = Mathf.Lerp(hpText.fontSize, targetSize, Time.deltaTime * sizeLerpSpeed);
        
        if (hp >= 30)
        {
            float t = (hp - 30f) / 70f;
            hpText.color = Color.Lerp(midHPColor, FullHPColor, t);
        }
        else if (hp >= 20)
        {
            float t = (hp - 20f) / 10f;
            hpText.color = Color.Lerp(midHPColor, lowHPColor, t);
        }
        else
        {
            float t = hp / 20f;
            hpText.color = Color.Lerp(lowHPColor, midHPColor, t);
        }
    }

    public static void ShakeOnHit()
    {
        shakeTimer = shakeDuration;
    }
}