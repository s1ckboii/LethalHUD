using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.CustomHUD.Refs;

public class LHHealthBarRefs : MonoBehaviour
{
    [Header("Images")]
    [Tooltip("Main frame image for the custom health bar. This image is recolored based on the player's current health.")]
    public Image frame;

    [Tooltip("Optional secondary frame image. Use this for extra decorative frame layers if your prefab needs one.")]
    public Image frameB;

    [Tooltip("Image used as the health fill. Supports normal Image.fillAmount or a material with _FillAmount and _OverhealAmount shader properties.")]
    public Image healthFill;

    [Header("Health")]
    [Tooltip("Optional TextMeshProUGUI used as the custom health number. Leave empty if this HUD style does not show a health number.")]
    public TextMeshProUGUI healthNumber;

    [Tooltip("When this HUD style is selected, apply the recommended health format below to the player's Health Format config. Players can still change the config afterwards.")]
    public bool setHealthFormatOnLoad = true;

    [Tooltip("Health format that gets applied when this HUD style is selected, if Set Health Format On Load is enabled.")]
    public HPDisplayMode recommendedHealthFormat = HPDisplayMode.Plain;

    [Header("Weight")]
    [Tooltip("Optional TextMeshProUGUI used as the custom weight counter for this HUD style. Leave empty to keep the vanilla weight counter.")]
    public TextMeshProUGUI weightNumber;

    [Tooltip("Preferred weight text layout for this HUD style. Config keeps the player's current weight display behavior.")]
    public WeightDisplayLayout weightLayout = WeightDisplayLayout.Config;

    [HideInInspector] public Vector2 healthNumberBasePos;

    private Material _frameMat;
    private Material _fillMat;
    private bool _usesShaderFill;

    private void Awake()
    {
        if (healthNumber != null)
            healthNumberBasePos = healthNumber.rectTransform.anchoredPosition;

        if (frame != null && frame.material != null)
            _frameMat = frame.material = Instantiate(frame.material);

        if (healthFill != null && healthFill.material != null)
        {
            _fillMat = healthFill.material = Instantiate(healthFill.material);
            _usesShaderFill = _fillMat != null && _fillMat.HasProperty("_FillAmount");
        }
        else
        {
            _usesShaderFill = false;
            _fillMat = null;
        }
    }

    public void SetFlowColor(Color color)
    {
        if (_frameMat != null && _frameMat.HasProperty("_FlowColor"))
            _frameMat.SetColor("_FlowColor", color);

        if (_fillMat != null && _fillMat.HasProperty("_FlowColor"))
            _fillMat.SetColor("_FlowColor", color);
    }

    public void UpdateHealthUI(int health, float hpFill, Color hpColor, float ohFill, Color ohColor)
    {
        if (frame != null)
            frame.color = hpColor;

        if (healthFill != null)
            healthFill.color = hpColor;

        if (_usesShaderFill && _fillMat != null)
        {
            _fillMat.SetFloat("_FillAmount", hpFill);
            _fillMat.SetFloat("_OverhealAmount", ohFill);

            if (_fillMat.HasProperty("_OverhealColor"))
                _fillMat.SetColor("_OverhealColor", ohColor);
        }
        else
        {
            if (healthFill != null)
                healthFill.fillAmount = hpFill;
        }

        if (healthNumber != null)
        {
            healthNumber.text = Plugins.ConfigEntries.HealthFormat.Value switch
            {
                HPDisplayMode.Percent => $"{health} %",
                HPDisplayMode.Label => $"{health} HP",
                _ => $"{health}",
            };
        }
    }
}