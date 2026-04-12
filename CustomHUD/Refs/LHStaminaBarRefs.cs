using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.CustomHUD.Refs;
public class LHStaminaBarRefs : MonoBehaviour
{
    [Header("Images")]
    public Image Frame;
    public Image FrameB;
    public Image Fill;

    private Material _frameMat;
    private Material _fillMat;
    private bool _usesShaderFill;

    private void Awake()
    {
        if (Frame != null && Frame.material != null)
            _frameMat = Frame.material = Instantiate(Frame.material);

        if (Fill != null && Fill.material != null)
        {
            _fillMat = Fill.material = Instantiate(Fill.material);

            _usesShaderFill = _fillMat.HasProperty("_FillAmount");
        }
        else
        {
            _usesShaderFill = false;
        }
    }

    public void UpdateStaminaUI(float fillAmount, Color currentColor)
    {
        if (Fill == null) return;

        Fill.color = currentColor;

        if (_usesShaderFill && _fillMat != null)
        {
            _fillMat.SetFloat("_FillAmount", fillAmount);
        }
        else
        {
            Fill.fillAmount = fillAmount;
        }
    }

    public void SetFlowColor(Color color)
    {
        if (_frameMat != null && _frameMat.HasProperty("_FlowColor"))
            _frameMat.SetColor("_FlowColor", color);
    }

    public void SetBarColor(Color color)
    {
        if (Frame != null) Frame.color = color;
        if (FrameB != null) FrameB.color = color;

        if (Fill != null)
        {
            Fill.color = color;
            if (_fillMat != null)
            {
                Fill.canvasRenderer.SetColor(color);
            }
        }
    }
}