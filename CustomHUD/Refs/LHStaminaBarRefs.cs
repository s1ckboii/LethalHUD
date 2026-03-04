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

    private void Awake()
    {
        if (Frame != null && Frame.material != null)
            _frameMat = Frame.material = Instantiate(Frame.material);

        if (Fill != null && Fill.material != null)
            _fillMat = Fill.material = Instantiate(Fill.material);
    }

    public void UpdateStaminaUI(float fillAmount, Color currentColor)
    {
        if (_fillMat != null)
        {
            _fillMat.SetFloat("_FillAmount", fillAmount);
            Fill.color = currentColor;
        }
    }

    public void SetFlowColor(Color color)
    {
        if (_frameMat != null)
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