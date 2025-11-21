using GameNetcodeStuff;
using LethalHUD.Compats;
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
    private static Color _midHPColor = new(1f, 0.3f, 0.3f);
    private static Color _lowHPColor = new(0.6f, 0f, 0f);

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

    public static void UpdateNumber()
    {
        if (_hpObj == null || _hpText == null) return;

        if (_hpObj.activeSelf)
        {
            if (!Plugins.ConfigEntries.HealthIndicator.Value)
            {
                _hpObj.SetActive(false);
            }
        }
        else
        {
            if (Plugins.ConfigEntries.HealthIndicator.Value)
            {
                _hpObj.SetActive(true);
            }
        }

        if (ModCompats.IsEladsHUDPresent) return;
        if (_hpText == null) return;
        if (_localPlayer == null) _localPlayer = GameNetworkManager.Instance.localPlayerController;
        if (_localPlayer == null) return;

        _hpObj.transform.localRotation = Quaternion.Euler(0f, 0f, HealthRotation);
        _basePosition = new Vector2(Plugins.ConfigEntries.HPIndicatorX.Value, Plugins.ConfigEntries.HPIndicatorY.Value);

        int hp = _localPlayer.health;

        _hpText.text = Plugins.ConfigEntries.HealthFormat.Value switch
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

        RectTransform rect = _hpText.rectTransform;
        rect.anchoredPosition = _basePosition + Random.insideUnitCircle * intensity;

        float formatMult = Plugins.ConfigEntries.HealthFormat.Value switch
        {
            HPDisplayMode.Percent => _percentSizeMult,
            HPDisplayMode.Label => _labelSizeMult,
            _ => _plainSizeMult
        };

        float targetSize = BaseFontSize * formatMult;

        if (_shakeTimer > 0f)
        {
            targetSize *= _sizeBump;
        }
        else if (hp < 20)
        {
            targetSize *= (1f + 0.1f * (20 - hp) / 20f);
        }

        _hpText.fontSize = Mathf.Lerp(_hpText.fontSize, targetSize, Time.deltaTime * _sizeLerpSpeed);
        
        if (hp >= 30)
        {
            float t = (hp - 30f) / 70f;
            _hpText.color = Color.Lerp(_midHPColor, FullHPColor, t);
        }
        else if (hp >= 20)
        {
            float t = (hp - 20f) / 10f;
            _hpText.color = Color.Lerp(_midHPColor, _lowHPColor, t);
        }
        else
        {
            float t = hp / 20f;
            _hpText.color = Color.Lerp(_lowHPColor, _midHPColor, t);
        }
    }

    public static void ShakeOnHit(PlayerControllerB player)
    {
        if (player != null && player != GameNetworkManager.Instance.localPlayerController)
            return;

        _shakeTimer = _shakeDuration;
    }
}