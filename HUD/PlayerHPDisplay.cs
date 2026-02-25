using GameNetcodeStuff;
using LethalHUD.Compats;
using LethalHUD.CustomHUD;
using TMPro;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
public static class PlayerHPDisplay
{
    private static TextMeshProUGUI _hpText;
    private static PlayerControllerB _localPlayer;
    private static GameObject _hpObj;

    private static float _shakeTimer = 0f;
    private static readonly float _shakeDuration = 0.3f;
    private static readonly float _hitShakeIntensity = 5f;
    private static readonly float _criticalMaxShakeIntensity = 1.5f;

    private static readonly float _sizeBump = 1.2f;
    private static readonly float _sizeLerpSpeed = 3f;
    private static Vector2 _basePosition;
    private static float BaseFontSize => Plugins.ConfigEntries.HealthSize.Value;
    private static float HealthRotation => Plugins.ConfigEntries.HealthRotation.Value;

    private static readonly float _plainSizeMult = 1f;
    private static readonly float _percentSizeMult = 0.7f;
    private static readonly float _labelSizeMult = 0.65f;

    internal static Color FullHPColor => HUDUtils.ParseHexColor(Plugins.ConfigEntries.HealthColor.Value);

    public static void Init()
    {
        if (ModCompats.IsEladsHUDPresent) return;
        HUDManager hud = HUDManager.Instance;
        if (_hpText != null) return;

        _localPlayer = GameNetworkManager.Instance.localPlayerController;

        _hpObj = new("PlayerHPDisplay");
        GameObject tpc = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner/");
        _hpObj.transform.SetParent(tpc.transform, false);
        _hpObj.transform.localRotation = Quaternion.Euler(0f, 0f, HealthRotation);

        _hpText = _hpObj.AddComponent<TextMeshProUGUI>();
        _hpText.font = hud.HUDQuotaNumerator.font;
        _hpText.fontSize = BaseFontSize;
        _hpText.alignment = TextAlignmentOptions.Left;
        _hpText.color = FullHPColor;

        _hpText.enableVertexGradient = false;
        _hpText.enableWordWrapping = false;
        _hpText.enableKerning = false;
        _hpText.fontSharedMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
        _hpText.fontSharedMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0f);
        _hpText.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
        _hpText.fontSharedMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, Color.black);
        _hpText.fontSharedMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 1.2f);
        _hpText.fontSharedMaterial.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -1.2f);

        RectTransform rect = _hpText.rectTransform;
        rect.anchoredPosition = new Vector2(Plugins.ConfigEntries.HPIndicatorX.Value, Plugins.ConfigEntries.HPIndicatorY.Value);
        rect.sizeDelta = new Vector2(200, 50);

        _basePosition = rect.anchoredPosition;
    }

    public static void UpdateNumber(TextMeshProUGUI externalText = null, Vector2? customBasePos = null)
    {
        bool isCustom = externalText != null;

        if (_hpObj != null)
        {
            _hpObj.SetActive(!CustomHealthBar.UsingCustom && Plugins.ConfigEntries.HealthIndicator.Value);
        }

        TextMeshProUGUI targetText = isCustom ? externalText : _hpText;
        if (targetText == null) return;

        if (_localPlayer == null) _localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (_localPlayer == null) return;

        int hp = _localPlayer.health;

        targetText.text = Plugins.ConfigEntries.HealthFormat.Value switch
        {
            HPDisplayMode.Percent => $"{hp} %",
            HPDisplayMode.Label => $"{hp} HP",
            _ => $"{hp}",
        };

        float intensity = 0f;
        if (_shakeTimer > 0f)
        {
            intensity = _hitShakeIntensity;
            _shakeTimer -= Time.deltaTime;
        }
        else if (hp < 20)
        {
            intensity = _criticalMaxShakeIntensity * (20 - hp) / 20f;
        }

        Vector2 basePos;
        if (isCustom)
        {
            basePos = customBasePos ?? Vector2.zero;
        }
        else
        {
            basePos = _basePosition;
        }

        RectTransform rect = targetText.rectTransform;
        rect.anchoredPosition = basePos + Random.insideUnitCircle * intensity;

        float formatMult = Plugins.ConfigEntries.HealthFormat.Value switch
        {
            HPDisplayMode.Percent => _percentSizeMult,
            HPDisplayMode.Label => _labelSizeMult,
            _ => _plainSizeMult
        };

        float targetSize = BaseFontSize * formatMult;
        if (_shakeTimer > 0f) targetSize *= _sizeBump;
        else if (hp < 20) targetSize *= (1f + 0.1f * (20 - hp) / 20f);

        targetText.fontSize = Mathf.Lerp(targetText.fontSize, targetSize, Time.deltaTime * _sizeLerpSpeed);

        targetText.color = HUDUtils.GetHPColor(hp);
    }

    public static void ShakeOnHit(PlayerControllerB player)
    {
        if (player != null && player != GameNetworkManager.Instance.localPlayerController)
            return;

        _shakeTimer = _shakeDuration;
    }
}