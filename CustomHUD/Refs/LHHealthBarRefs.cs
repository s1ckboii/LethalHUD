using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.CustomHUD.Refs;
public class LHHealthBarRefs : MonoBehaviour
{
    [Header("Images")]
    public Image Frame;
    public Image FrameB;
    public Image HealthFill;

    [Header("Optional")]
    public TextMeshProUGUI Number;

    [HideInInspector] public Vector2 NumberBasePosition;

    private Material _frameMat;
    private Material _fillMat;
    private bool _usesShaderFill;

    private void Awake()
    {
        if (Number != null)
            NumberBasePosition = Number.rectTransform.anchoredPosition;
        if (Frame != null && Frame.material != null)
            _frameMat = Frame.material = Instantiate(Frame.material);
        if (HealthFill != null && HealthFill.material != null)
        {
            _fillMat = HealthFill.material = Instantiate(HealthFill.material);
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
        if (Frame != null)
            Frame.color = hpColor;

        if (HealthFill != null)
            HealthFill.color = hpColor;

        if (_usesShaderFill && _fillMat != null)
        {
            _fillMat.SetFloat("_FillAmount", hpFill);
            _fillMat.SetFloat("_OverhealAmount", ohFill);

            if (_fillMat.HasProperty("_OverhealColor"))
                _fillMat.SetColor("_OverhealColor", ohColor);
        }
        else
        {
            if (HealthFill != null)
                HealthFill.fillAmount = hpFill;
        }

        if (Number != null)
        {
            Number.text = Plugins.ConfigEntries.HealthFormat.Value switch
            {
                HPDisplayMode.Percent => $"{health} %",
                HPDisplayMode.Label => $"{health} HP",
                _ => $"{health}",
            };
        }
    }
}