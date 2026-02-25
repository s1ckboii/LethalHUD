using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LethalHUD.CustomHUD.Refs;

public class LHHealthBarRefs : MonoBehaviour
{
    [Header("Core")]
    public Image Inner;

    [Header("Optional")]
    public TextMeshProUGUI Number;

    [HideInInspector] public Vector2 NumberBasePosition;
    private Material _innerMat;

    private void Awake()
    {
        if (Number != null)
            NumberBasePosition = Number.rectTransform.anchoredPosition;
        if (Inner != null && Inner.material != null)
            _innerMat = Inner.material = Instantiate(Inner.material);
    }

    public void SetFlowColor(Color color)
    {
        if (_innerMat != null)
            _innerMat.SetColor("_FlowColor", color);
    }

    public void UpdateHealthUI(int health, float hpFill, Color hpColor, float ohFill, Color ohColor)
    {
        if (Inner != null)
            Inner.color = hpColor;

        if (_innerMat != null)
        {
            _innerMat.SetFloat("_FillAmount", hpFill);
            _innerMat.SetFloat("_OverhealAmount", ohFill);
            _innerMat.SetColor("_OverhealColor", ohColor);
        }
    }
}