using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LethalHUD.CustomHUD.Refs;
public class LHBatteryRefs : MonoBehaviour
{
    public Image Fill;
    public Image Frame;
    public Image Icon;
    public TMP_Text Text;

    private Material _fillMat;

    private void Awake()
    {
        if (Fill != null && Fill.material != null)
            _fillMat = Fill.material = Instantiate(Fill.material);
    }

    public void UpdateBattery(float fill)
    {
        if (_fillMat != null)
            _fillMat.SetFloat("_FillAmount", fill);

        if (Fill != null)
            Fill.fillAmount = fill;
    }

    public void SetColor(Color color)
    {
        if (Fill != null) Fill.color = color;
        if (Frame != null) Frame.color = color;
        if (Icon != null) Icon.color = color;
        if (Text != null) Text.color = color;
    }
}