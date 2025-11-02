using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.HUD;
internal static class SignalTranslatorController
{
    private static RectTransform cachedRect;
    private static Image[] animatorImages;
    private static TMP_Text[] animatorTMPs;
    private static bool isCentered = false;

    public static void CenterText()
    {
        if (!Plugins.ConfigEntries.CenterSTText.Value)
            return;
        
        HUDManager hud = HUDManager.Instance;
        TMP_Text signalText = hud.signalTranslatorText;
        cachedRect = hud.signalTranslatorText.rectTransform;
        Animator signalAnimator = hud.signalTranslatorAnimator;
        
        if (signalAnimator != null)
        {
            animatorImages = signalAnimator.GetComponentsInChildren<Image>(true);
            animatorTMPs = signalAnimator.GetComponentsInChildren<TMP_Text>(true);
        }

        if (!isCentered)
        {
            signalText.horizontalAlignment = HorizontalAlignmentOptions.Center;
            signalText.enableAutoSizing = false;

            cachedRect.anchorMin = Vector2.zero;
            cachedRect.anchorMax = Vector2.one;
            cachedRect.offsetMin = new Vector2(-5, -230);
            cachedRect.offsetMax = new Vector2(-5, -230);

            isCentered = true;
        }
    }

    public static void ApplyColor()
    {
        HUDManager hud = HUDManager.Instance;
        TMP_Text signalText = hud.signalTranslatorText;
        if (signalText == null) return;

        Color color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SignalTextColor.Value);

        if (signalText.color != color)
            signalText.color = color;

        if (animatorTMPs != null)
        {
            foreach (TMP_Text tmp in animatorTMPs)
                if (tmp.color != color)
                    tmp.color = color;
        }

        if (animatorImages != null)
        {
            foreach (Image img in animatorImages)
                if (img.color != color)
                    img.color = color;
        }
    }

    public static void ApplyInMono()
    {
        Image signalBG = GameObject.Find("Systems/UI/Canvas/SpecialGraphics/Misc/SignalTransmission/SignalAnimContainer/SignalBG")?.GetComponent<Image>();
        TMP_Text signalText1 = GameObject.Find("Systems/UI/Canvas/SpecialGraphics/Misc/SignalTransmission/SignalAnimContainer/SignalText (1)")?.GetComponent<TMP_Text>();
        TMP_Text signalText2 = GameObject.Find("Systems/UI/Canvas/SpecialGraphics/Misc/SignalTransmission/SignalAnimContainer/SignalText (2)")?.GetComponent<TMP_Text>();

        Color color = HUDUtils.ParseHexColor(Plugins.ConfigEntries.SignalTextColor.Value);
        Color signalBGColor = new(1 - color.r, 1 - color.g, 1 - color.b, 18f / 255f);
        Color signalText1Color = new(1 - color.r, 1 - color.g, 1 - color.b, 156f / 255f);
        Color signalText2Color = new(color.r, color.g, color.b, 156f / 255f);

        if (signalBG != null && signalBG.color != signalBGColor)
            signalBG.color = signalBGColor;

        if (signalText1 != null && signalText1.color != signalText1Color)
            signalText1.color = signalText1Color;

        if (signalText2 != null && signalText2.color != signalText2Color)
            signalText2.color = signalText2Color;
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