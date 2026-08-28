using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.CustomHUD.Refs;

public class LHStaminaBarRefs : MonoBehaviour
{
    [Header("Images")]
    [Tooltip("Main frame image for the custom stamina bar. If its material has a _FlowColor property, it will be affected by the Custom Stamina Bar Color config.")]
    public Image frame;

    [Tooltip("Optional secondary frame layer. This is recolored together with the main Frame by SetBarColor.")]
    public Image frameB;

    [Tooltip("Image used as the stamina fill. Supports normal Image.fillAmount or a material with a _FillAmount shader property.")]
    public Image fill;

    private Material _frameMat;
    private Material _fillMat;
    private bool _usesShaderFill;

    private void Awake()
    {
        if (frame != null && frame.material != null)
            _frameMat = frame.material = Instantiate(frame.material);

        if (fill != null && fill.material != null)
        {
            _fillMat = fill.material = Instantiate(fill.material);
            _usesShaderFill = _fillMat.HasProperty("_FillAmount");
        }
        else
        {
            _usesShaderFill = false;
        }
    }

    public void UpdateStaminaUI(float fillAmount, Color currentColor)
    {
        if (fill == null) return;

        fillAmount = Mathf.Clamp01(fillAmount);

        fill.color = currentColor;

        if (_usesShaderFill && _fillMat != null)
        {
            _fillMat.SetFloat("_FillAmount", fillAmount);
        }
        else
        {
            fill.fillAmount = fillAmount;
        }
    }

    public void SetFlowColor(Color color)
    {
        if (_frameMat != null && _frameMat.HasProperty("_FlowColor"))
            _frameMat.SetColor("_FlowColor", color);
    }

    public void SetBarColor(Color color)
    {
        if (frame != null)
            frame.color = color;

        if (frameB != null)
            frameB.color = color;

        if (fill != null)
        {
            fill.color = color;

            if (_fillMat != null)
                fill.canvasRenderer.SetColor(color);
        }
    }
}