using GameNetcodeStuff;
using LethalHUD.CustomHUD;
using UnityEngine;
using UnityEngine.UI;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;

internal static class PlayerRedCanvasController
{
    private static CanvasGroup _canvas;
    private static Image _image;

    private static Image.Type _originalType;
    private static Image.FillMethod _originalFillMethod;
    private static int _originalFillOrigin;
    private static Color _originalColor;
    private static bool _cachedOriginal;
    private static Image _overhealImage;
    private static bool _cachedOverheal;
    private static CanvasGroup _overhealCanvas;

    internal static void Bind(CanvasGroup original)
    {
        if (original == null)
            return;

        _canvas = original;

        _image = original.GetComponentInChildren<Image>();
        if (_image == null)
            return;

        {
            _originalType = _image.type;
            _originalFillMethod = _image.fillMethod;
            _originalFillOrigin = _image.fillOrigin;
            _originalColor = _image.color;

            _cachedOriginal = true;
        }

        if (!_cachedOverheal)
        {
            _overhealImage = Object.Instantiate(_image, _image.transform.parent);
            _overhealImage.name = "SelfRedCanvas_Overheal";
            _overhealImage.transform.SetAsLastSibling();

            _overhealCanvas = _overhealImage.GetComponentInParent<CanvasGroup>();

            if (_overhealCanvas != null)
                _overhealCanvas.alpha = 1f;

            _overhealImage.type = Image.Type.Filled;
            _overhealImage.fillMethod = Image.FillMethod.Vertical;
            _overhealImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            _overhealImage.fillAmount = 0f;
            _overhealImage.color = HUDUtils.HP_Blue;

            _cachedOverheal = true;
        }

        _image.type = Image.Type.Filled;
        _image.fillMethod = Image.FillMethod.Vertical;
        _image.fillOrigin = (int)Image.OriginVertical.Bottom;
        _image.fillAmount = 1f;
    }

    public static void SetFill(float fill)
    {
        if (_image == null)
            return;

        _image.fillAmount = Mathf.Clamp01(fill);
    }

    public static void SetColor(Color color)
    {
        if (_image == null)
            return;

        _image.color = color;
    }

    private static void ApplyFilledBase()
    {
        _canvas.alpha = 1f;

        _image.type = Image.Type.Filled;
        _image.fillMethod = Image.FillMethod.Vertical;
        _image.fillOrigin = (int)Image.OriginVertical.Bottom;
    }

    internal static void ApplyFillAndColor(int health)
    {
        if (CustomHealthBar.UsingCustom) return;
        ApplyFilledBase();

        float hp01 = Mathf.Clamp01(health / 100f);
        
        SetFill(hp01);

        SetColor(HUDUtils.GetHPColor(Mathf.Min(health, 100)));

        ApplyOverheal(health);
    }

    internal static void ApplyFillWithRedFade(int health)
    {
        if (CustomHealthBar.UsingCustom) return;
        ApplyFilledBase();

        health = Mathf.Min(health, 100);

        float hp01 = Mathf.Clamp01(health / 100f);
        float damage01 = 1f - hp01;

        SetFill(damage01);

        SetColor(Color.Lerp(HUDUtils.HP_BrightRed, HUDUtils.HP_DarkRed, damage01));

        if (_overhealImage != null)
            _overhealImage.fillAmount = 0f;
    }

    private static void ApplyOverheal(int health)
    {
        if (_overhealImage == null)
            return;

        if (health <= 100)
        {
            _overhealImage.fillAmount = 0f;
            return;
        }

        float overheal01 = Mathf.Clamp01((health - 100f) / 100f);
        _overhealImage.fillAmount = overheal01;
        _overhealImage.color = HUDUtils.GetHPColor(health);
    }

    internal static void ResetToVanilla(int health)
    {
        if (_image == null || !_cachedOriginal)
            return;

        _image.enabled = true;
        _canvas.alpha = (float)(100 - health) / 100f;

        _image.type = _originalType;
        _image.fillMethod = _originalFillMethod;
        _image.fillOrigin = _originalFillOrigin;
        _image.color = _originalColor;

        if (_overhealImage != null)
            _overhealImage.fillAmount = 0f;
    }

    internal static void ChangeSetting()
    {
        HUDManager hud = HUDManager.Instance;
        if (hud == null || hud.selfRedCanvasGroup == null)
            return;

        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (player == null)
            return;

        if (CustomHealthBar.UsingCustom)
        {
            DisableLogicAndHide();
            return;
        }

        int health = player.health;

        switch (Plugins.ConfigEntries.SelfRedCanvasMode.Value)
        {
            case SelfRedMode.Vanilla:
                ResetToVanilla(health);
                break;

            case SelfRedMode.ColoredFilled:
                ApplyFillAndColor(health);
                break;

            case SelfRedMode.RedFillUp:
                ApplyFillWithRedFade(health);
                break;
        }
    }
    private static void DisableLogicAndHide()
    {
        if (_canvas != null) _canvas.alpha = 0f;
        if (_image != null) _image.enabled = false;
        if (_overhealImage != null) _overhealImage.enabled = false;
        if (_overhealCanvas != null) _overhealCanvas.alpha = 0f;
    }
}