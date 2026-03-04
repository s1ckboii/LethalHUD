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

    private void Awake()
    {
        if (Number != null)
            NumberBasePosition = Number.rectTransform.anchoredPosition;
        if (Frame != null && Frame.material != null)
            _frameMat = Frame.material = Instantiate(Frame.material);
        if (HealthFill != null && HealthFill.material != null)
            _fillMat = HealthFill.material = Instantiate(HealthFill.material);
    }

    public void SetFlowColor(Color color)
    {
        if (_frameMat != null)
            _frameMat.SetColor("_FlowColor", color);
        if (_fillMat != null)
            _fillMat.SetColor("_FlowColor", color);
    }

    public void UpdateHealthUI(int health, float hpFill, Color hpColor, float ohFill, Color ohColor)
    {
        if (Frame != null)
            Frame.color = hpColor;
        if (HealthFill != null)
            HealthFill.color = hpColor;

        if (_fillMat != null)
        {
            _fillMat.SetFloat("_FillAmount", hpFill);
            _fillMat.SetFloat("_OverhealAmount", ohFill);
            _fillMat.SetColor("_OverhealColor", ohColor);
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