using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.CustomHUD.Refs;

public class LHBatteryRefs : MonoBehaviour
{
    [Header("Images")]
    [Tooltip("Image used as the battery fill. Supports normal Image.fillAmount and materials with a _FillAmount shader property.")]
    public Image fill;

    [Tooltip("Optional frame image for the battery UI. This is recolored by the custom battery color config.")]
    public Image frame;

    [Tooltip("Optional battery icon image. This is recolored by the custom battery color config and hidden when the battery UI is hidden.")]
    public Image icon;

    [Header("Text")]
    [Tooltip("Optional TextMeshPro text used to display the battery charge amount. Leave empty if this HUD style does not show battery text.")]
    public TMP_Text text;

    [Tooltip("Controls whether the battery charge text is shown as a number or as a percent.")]
    public BatteryTextDisplayMode textDisplayMode = BatteryTextDisplayMode.Percent;

    private Material _fillMat;

    private void Awake()
    {
        if (fill != null && fill.material != null)
            _fillMat = fill.material = Instantiate(fill.material);
    }

    public void UpdateBattery(float fillAmount)
    {
        fillAmount = Mathf.Clamp01(fillAmount);

        if (_fillMat != null && _fillMat.HasProperty("_FillAmount"))
            _fillMat.SetFloat("_FillAmount", fillAmount);

        if (fill != null)
            fill.fillAmount = fillAmount;

        UpdateText(fillAmount);
    }

    public void SetColor(Color color)
    {
        if (fill != null)
            fill.color = color;

        if (frame != null)
            frame.color = color;

        if (icon != null)
            icon.color = color;

        if (text != null)
            text.color = color;
    }

    private void UpdateText(float fillAmount)
    {
        if (text == null)
            return;

        int percent = Mathf.RoundToInt(fillAmount * 100f);

        text.enabled = true;

        text.text = textDisplayMode switch
        {
            BatteryTextDisplayMode.Percent => percent + "%",
            _ => percent.ToString(),
        };
    }
}