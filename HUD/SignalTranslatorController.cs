using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.HUD;
internal static class SignalTranslatorController
{
    private static RectTransform _cachedRect;
    private static Image[] _animatorImages;
    private static TMP_Text[] _animatorTMPs;
    private static bool _isCentered = false;

    private static Image _signalBG;
    private static TMP_Text _signalText1;
    private static TMP_Text _signalText2;

    public static void CenterText()
    {
        if (!Plugins.ConfigEntries.CenterSTText.Value)
            return;
        
        HUDManager hud = HUDManager.Instance;
        TMP_Text signalText = hud.signalTranslatorText;
        _cachedRect = hud.signalTranslatorText.rectTransform;
        Animator signalAnimator = hud.signalTranslatorAnimator;
        
        if (signalAnimator != null)
        {
            _animatorImages = signalAnimator.GetComponentsInChildren<Image>(true);
            _animatorTMPs = signalAnimator.GetComponentsInChildren<TMP_Text>(true);
        }

        if (!_isCentered)
        {
            signalText.horizontalAlignment = HorizontalAlignmentOptions.Center;
            signalText.enableAutoSizing = false;

            _cachedRect.anchorMin = Vector2.zero;
            _cachedRect.anchorMax = Vector2.one;
            _cachedRect.offsetMin = new Vector2(-5, -230);
            _cachedRect.offsetMax = new Vector2(-5, -230);

            _isCentered = true;
        }
    }

    public static void ApplyColor()
    {
        HUDManager hud = HUDManager.Instance;
        TMP_Text signalText = hud.signalTranslatorText;
        if (signalText == null) return;

        Color color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SignalMessageColor.Value);

        if (signalText.color != color)
            signalText.color = color;

        if (_animatorTMPs != null)
        {
            foreach (TMP_Text tmp in _animatorTMPs)
                if (tmp.color != color)
                    tmp.color = color;
        }

        if (_animatorImages != null)
        {
            foreach (Image img in _animatorImages)
                if (img.color != color)
                    img.color = color;
        }
    }

    public static void ApplyInMono()
    {
        if (_signalBG == null || _signalText1 == null || _signalText2 == null)
        {
            _signalBG = GameObject.Find("Systems/UI/Canvas/SpecialGraphics/Misc/SignalTransmission/SignalAnimContainer/SignalBG")?.GetComponent<Image>();
            _signalText1 = GameObject.Find("Systems/UI/Canvas/SpecialGraphics/Misc/SignalTransmission/SignalAnimContainer/SignalText (1)")?.GetComponent<TMP_Text>();
            _signalText2 = GameObject.Find("Systems/UI/Canvas/SpecialGraphics/Misc/SignalTransmission/SignalAnimContainer/SignalText (2)")?.GetComponent<TMP_Text>();
        }

        Color color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SignalTextColor.Value);
        Color color2 = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SignalText2Color.Value);
        Color color3 = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SignalBGColor.Value);

        Color signalText1Color = new(color.r, color.g, color.b, 156f / 255f);
        Color signalText2Color = new(color2.r, color2.g, color2.b, 156f / 255f);
        Color signalBGColor = new(color3.r, color3.g, color3.b, 18f / 255f);

        if (_signalBG != null && _signalBG.color != signalBGColor)
            _signalBG.color = signalBGColor;

        if (_signalText1 != null && _signalText1.color != signalText1Color)
            _signalText1.color = signalText1Color;

        if (_signalText2 != null && _signalText2.color != signalText2Color)
            _signalText2.color = signalText2Color;
    }


    public static void SetSignalText(string message)
    {
        HUDManager hud = HUDManager.Instance;
        TMP_Text signalText = hud.signalTranslatorText;
        if (signalText == null) return;

        string trimmed = message.Length > 12 ? message[..12] : message;
        trimmed = trimmed.Trim();

        signalText.text = trimmed;

        ApplyColor();

        CenterText();
    }
    public static IEnumerator DisplaySignalTranslatorMessage(string signalMessage, int seed, SignalTranslator signalTranslator)
    {
        if (signalTranslator == null) yield break;

        System.Random signalMessageRandom = new(seed + StartOfRound.Instance.randomMapSeed);

        HUDManager hud = HUDManager.Instance;
        hud.signalTranslatorAnimator.SetBool("transmitting", true);
        signalTranslator.localAudio.Play();
        hud.UIAudio.PlayOneShot(signalTranslator.startTransmissionSFX, 1f);

        SetSignalText("");

        yield return new WaitForSeconds(1.21f);

        for (int i = 0; i < signalMessage.Length; i++)
        {
            if (signalTranslator == null || !signalTranslator.gameObject.activeSelf)
                break;

            hud.UIAudio.PlayOneShot(signalTranslator.typeTextClips[Random.Range(0, signalTranslator.typeTextClips.Length)]);

            SetSignalText(hud.signalTranslatorText.text + signalMessage[i]);

            float num = Mathf.Min(signalMessageRandom.Next(-1, 4) * 0.05f, 0f);
            yield return new WaitForSeconds(Plugins.ConfigEntries.SignalLetterDisplay.Value + num);
        }

        if (signalTranslator != null)
        {
            hud.UIAudio.PlayOneShot(signalTranslator.finishTypingSFX);
            signalTranslator.localAudio.Stop();
        }

        yield return new WaitForSeconds(0.5f);
        hud.signalTranslatorAnimator.SetBool("transmitting", false);
    }
}