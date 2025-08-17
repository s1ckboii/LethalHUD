using GameNetcodeStuff;
using LethalHUD.Configs;
using TMPro;
using UnityEngine;

namespace LethalHUD.HUD
{
    public static class PlayerHPDisplay
    {
        private static TextMeshProUGUI hpText;
        private static PlayerControllerB localPlayer;

        private static float shakeTimer = 0f;
        private static readonly float shakeDuration = 0.3f;
        private static readonly float hitShakeIntensity = 5f;
        private static readonly float criticalMaxShakeIntensity = 1.5f;

        private static readonly float sizeBump = 1.2f;
        private static readonly float sizeLerpSpeed = 3f;
        private static Vector2 basePosition;
        private static float baseFontSize;

        private static Color fullHPColor = Color.green;
        private static Color midHPColor = new(1f, 0.3f, 0.3f);
        private static Color lowHPColor = new(0.6f, 0f, 0f);

        public static void Init()
        {
            if (!Plugins.ConfigEntries.HealthIndicator.Value) return;
            HUDManager hud = HUDManager.Instance;
            if (hpText != null) return;

            localPlayer = GameNetworkManager.Instance.localPlayerController;

            GameObject hpObj = new("PlayerHPDisplay");
            hpObj.transform.SetParent(hud.HUDContainer.transform, false);

            hpText = hpObj.AddComponent<TextMeshProUGUI>();
            hpText.font = hud.HUDQuotaNumerator.font;
            hpText.fontSize = 24f;
            hpText.alignment = TextAlignmentOptions.Left;
            hpText.color = Plugins.ConfigEntries.HealthStarterColor.Value ? ConfigHelper.GetSlotColor() : fullHPColor;

            hpText.enableVertexGradient = false;
            hpText.enableWordWrapping = false;
            hpText.enableKerning = false;
            hpText.fontSharedMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
            hpText.fontSharedMaterial.SetFloat(ShaderUtilities.ID_GlowPower, 0f);

            RectTransform rect = hpText.rectTransform;
            rect.anchoredPosition = new Vector2(Plugins.ConfigEntries.HPIndicatorX.Value, Plugins.ConfigEntries.HPIndicatorY.Value);
            rect.sizeDelta = new Vector2(200, 50);

            basePosition = rect.anchoredPosition;
            baseFontSize = hpText.fontSize;
        }

        public static void UpdateNumber()
        {
            if (hpText == null) return;
            if (localPlayer == null) localPlayer = GameNetworkManager.Instance.localPlayerController;
            if (localPlayer == null) return;

            basePosition = new Vector2(Plugins.ConfigEntries.HPIndicatorX.Value, Plugins.ConfigEntries.HPIndicatorY.Value);

            int hp = localPlayer.health;
            hpText.gameObject.SetActive(hp > 0);
            if (hp <= 0) return;

            hpText.text = $"{hp}";

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

            float targetSize = baseFontSize;
            if (shakeTimer > 0f)
            {
                targetSize = baseFontSize * sizeBump;
            }
            else if (hp < 20)
            {
                targetSize = baseFontSize * (1f + 0.1f * (20 - hp) / 20f);
            }
            hpText.fontSize = Mathf.Lerp(hpText.fontSize, targetSize, Time.deltaTime * sizeLerpSpeed);

            if (Plugins.ConfigEntries.HealthStarterColor.Value && hp >= 30)
            {
                hpText.color = ConfigHelper.GetSlotColor();
            }
            else if (hp >= 30)
            {
                hpText.color = fullHPColor;
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
}
