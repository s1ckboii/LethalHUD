using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LethalHUD.HUD
{
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
                foreach (var tmp in animatorTMPs)
                    if (tmp.color != color)
                        tmp.color = color;
            }

            if (animatorImages != null)
            {
                foreach (var img in animatorImages)
                    if (img.color != color)
                        img.color = color;
            }
        }

        public static void SetSignalText(string message)
        {
            HUDManager hud = HUDManager.Instance;
            TMP_Text signalText = hud.signalTranslatorText;
            if (signalText == null) return;

            string trimmed = message.Length > 10 ? message[..10] : message;
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
}
