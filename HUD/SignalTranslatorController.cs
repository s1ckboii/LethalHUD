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
    private static float _nextSignalLookupTime;

    private static string _lastTextHex1;
    private static string _lastTextHex2;
    private static string _lastBGHex;
    private static Color _cachedText1Color;
    private static Color _cachedText2Color;
    private static Color _cachedBGColor;

    public static void CenterText()
    {
        if (!Plugins.ConfigEntries.CenterSTText.Value)
            return;

        HUDManager hud = HUDManager.Instance;
        if (hud?.signalTranslatorText == null)
            return;

        TMP_Text signalText = hud.signalTranslatorText;
        _cachedRect = signalText.rectTransform;
        Animator signalAnimator = hud.signalTranslatorAnimator;

        if (signalAnimator != null && (_animatorImages == null || _animatorTMPs == null))
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
        TMP_Text signalText = hud?.signalTranslatorText;
        if (signalText == null)
            return;

        Color color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SignalMessageColor.Value);

        if (signalText.color != color)
            signalText.color = color;

        if (_animatorTMPs != null)
        {
            for (int i = 0; i < _animatorTMPs.Length; i++)
            {
                TMP_Text tmp = _animatorTMPs[i];
                if (tmp != null && tmp.color != color)
                    tmp.color = color;
            }
        }

        if (_animatorImages != null)
        {
            for (int i = 0; i < _animatorImages.Length; i++)
            {
                Image img = _animatorImages[i];
                if (img != null && img.color != color)
                    img.color = color;
            }
        }
    }

    public static void ApplyInMono()
    {
        EnsureSignalRefs();
        RefreshCachedColors();

        if (_signalBG != null && _signalBG.color != _cachedBGColor)
            _signalBG.color = _cachedBGColor;

        if (_signalText1 != null && _signalText1.color != _cachedText1Color)
            _signalText1.color = _cachedText1Color;

        if (_signalText2 != null && _signalText2.color != _cachedText2Color)
            _signalText2.color = _cachedText2Color;
    }

    private static void EnsureSignalRefs()
    {
        if (_signalBG != null && _signalText1 != null && _signalText2 != null)
            return;

        if (Time.unscaledTime < _nextSignalLookupTime)
            return;
    
        _nextSignalLookupTime = Time.unscaledTime + 0.25f;

        if (_signalBG == null)
        {
            _signalBG = GameObject.Find("Systems/UI/Canvas/SpecialGraphics/Misc/SignalTransmission/SignalAnimContainer/SignalBG")
                ?.GetComponent<Image>();
        }

        if (_signalText1 == null)
        {
            _signalText1 = GameObject.Find("Systems/UI/Canvas/SpecialGraphics/Misc/SignalTransmission/SignalAnimContainer/SignalText (1)")
                ?.GetComponent<TMP_Text>();
        }

        if (_signalText2 == null)
        {
            _signalText2 = GameObject.Find("Systems/UI/Canvas/SpecialGraphics/Misc/SignalTransmission/SignalAnimContainer/SignalText (2)")
                ?.GetComponent<TMP_Text>();
        }
    }

    private static void RefreshCachedColors()
    {
        string textHex1 = Plugins.ConfigEntries.SignalTextColor.Value;
        string textHex2 = Plugins.ConfigEntries.SignalText2Color.Value;
        string bgHex = Plugins.ConfigEntries.SignalBGColor.Value;

        if (_lastTextHex1 != textHex1)
        {
            _lastTextHex1 = textHex1;
            Color color = HUDUtils.ParseHexColor(textHex1);
            _cachedText1Color = new Color(color.r, color.g, color.b, 156f / 255f);
        }

        if (_lastTextHex2 != textHex2)
        {
            _lastTextHex2 = textHex2;
            Color color = HUDUtils.ParseHexColor(textHex2);
            _cachedText2Color = new Color(color.r, color.g, color.b, 156f / 255f);
        }

        if (_lastBGHex != bgHex)
        {
            _lastBGHex = bgHex;
            Color color = HUDUtils.ParseHexColor(bgHex);
            _cachedBGColor = new Color(color.r, color.g, color.b, 18f / 255f);
        }
    }

    public static void SetSignalText(string message, bool isTyping = true)
    {
        HUDManager hud = HUDManager.Instance;
        TMP_Text signalText = hud?.signalTranslatorText;
        if (signalText == null)
            return;

        const int maxLimit = 32;
        string processed = message.Length > maxLimit ? message[..maxLimit] : message;

        signalText.text = processed;

        if (!isTyping)
        {
            ApplyColor();
            CenterText();
            ApplyInMono();
        }
    }

    public static IEnumerator DisplaySignalTranslatorMessage(string signalMessage, int seed, SignalTranslator signalTranslator)
    {
        if (signalTranslator == null)
            yield break;

        System.Random signalMessageRandom = new(seed + StartOfRound.Instance.randomMapSeed);
        HUDManager hud = HUDManager.Instance;

        hud.signalTranslatorAnimator.SetBool("transmitting", true);
        signalTranslator.localAudio.Play();
        hud.UIAudio.PlayOneShot(signalTranslator.startTransmissionSFX, 1f);

        SetSignalText("", false);

        yield return new WaitForSeconds(1.21f);

        for (int i = 0; i < signalMessage.Length; i++)
        {
            if (signalTranslator == null || !signalTranslator.gameObject.activeSelf)
                break;

            char currentChar = signalMessage[i];

            if (currentChar != ' ')
            {
                hud.UIAudio.PlayOneShot(signalTranslator.typeTextClips[UnityEngine.Random.Range(0, signalTranslator.typeTextClips.Length)]);
            }

            SetSignalText(hud.signalTranslatorText.text + currentChar, true);

            float variance = Mathf.Min(signalMessageRandom.Next(-1, 4) * 0.05f, 0f);
            yield return new WaitForSeconds(Plugins.ConfigEntries.SignalLetterDisplay.Value + variance);
        }

        if (signalTranslator != null)
        {
            hud.UIAudio.PlayOneShot(signalTranslator.finishTypingSFX);
            signalTranslator.localAudio.Stop();
        }

        yield return new WaitForSeconds(0.5f);
        hud.signalTranslatorAnimator.SetBool("transmitting", false);
    }

    internal static void ResetCache()
    {
        _cachedRect = null;
        _animatorImages = null;
        _animatorTMPs = null;
        _isCentered = false;

        _signalBG = null;
        _signalText1 = null;
        _signalText2 = null;
        _nextSignalLookupTime = 0f;

        _lastTextHex1 = null;
        _lastTextHex2 = null;
        _lastBGHex = null;
    }
}