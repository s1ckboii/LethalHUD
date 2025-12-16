using GameNetcodeStuff;
using LethalHUD.Configs;
using System.Linq;
using TMPro;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class ChatController
{
    private static bool ColoringEnabled => Plugins.ConfigEntries.ColoredNames.Value;

    internal static string NoPunctuation(string input)
    {
        return new string([.. input.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '.')]);
    }

    internal static string GetColoredPlayerName(string playerName, int playerId = -1)
    {
        if (string.IsNullOrEmpty(playerName))
            return playerName;

        if (ColoringEnabled && playerId < StartOfRound.Instance.allPlayerScripts.Length)
        {
            PlayerControllerB player = playerId >= 0 ? StartOfRound.Instance.allPlayerScripts[playerId] : GameNetworkManager.Instance.localPlayerController;
            if (player.TryGetComponent(out PlayerColorNetworker playerChatNetworkManager))
            {
                PlayerColorInfo info = playerChatNetworkManager.PlayerColors;
                return HUDUtils.ApplyStaticGradient(playerName, info.colorA, info.colorB);
            }
        }

        return $"<color=#FF0000>{playerName}</color>";
    }
    internal static string GetColoredChatMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        message = message.Trim();

        if (HUDUtils.HasCustomGradient(Plugins.ConfigEntries.GradientMessageColorA.Value, Plugins.ConfigEntries.GradientMessageColorB.Value))
        {
            if (ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientMessageColorA.Value, out Color colorA) &&
                ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientMessageColorB.Value, out Color colorB))
            {
                return HUDUtils.ApplyStaticGradient(message, colorA, colorB);
            }
        }

        return $"<color={Plugins.ConfigEntries.ChatMessageColor.Value}>{message}</color>";
    }
    internal static void PlayerTypingIndicator()
    {
        TextMeshProUGUI indicator = HUDManager.Instance.typingIndicator;
        indicator.enableVertexGradient = true;
        indicator.color = Color.white;
        if (indicator == null)
            return;

        if (HUDUtils.HasCustomGradient(Plugins.ConfigEntries.GradientColorA.Value, Plugins.ConfigEntries.GradientColorB.Value))
        {
            if (ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorA.Value, out Color startColor) &&
                ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorB.Value, out Color endColor))
            {
                HUDUtils.ApplyWaveVertexGradient(indicator, startColor, endColor, Time.time * 0.2f, VertexGradientLayout.Vertical);
                return;
            }
        }

        switch (InventoryFrames.CurrentSlotColorMode)
        {
            case SlotEnums.Rainbow:
                HUDUtils.ApplyRainbowGradient(indicator, Time.time * 0.25f);
                break;

            case SlotEnums.None:
                indicator.colorGradient = new VertexGradient(ConfigHelper.GetSlotColor());
                break;

            default:
                HUDUtils.ApplyWaveVertexGradient(indicator, InventoryFrames.CurrentGradientStartColor, InventoryFrames.CurrentGradientEndColor, Time.time * 0.25f, VertexGradientLayout.Vertical);
                break;
        }
    }

    internal static void ColorChatInputField(TMP_InputField inputField, float time)
    {
        Color targetColor;

        if (HUDUtils.HasCustomGradient(Plugins.ConfigEntries.GradientColorA.Value, Plugins.ConfigEntries.GradientColorB.Value))
        {
            if (ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorA.Value, out Color startColor) &&
                ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientColorB.Value, out Color endColor))
            {
                float wave = Mathf.SmoothStep(0f, 1f, Mathf.Sin(time * Mathf.PI * 2f) * 0.5f + 0.5f);
                targetColor = Color.Lerp(startColor, endColor, wave);
            }
            else
            {
                targetColor = ConfigHelper.GetSlotColor();
            }
        }
        else
        {
            switch (InventoryFrames.CurrentSlotColorMode)
            {
                case SlotEnums.Rainbow:
                    float speed = 0.5f;
                    float hueShift = (time * speed) % 1f;
                    targetColor = Color.HSVToRGB(hueShift, 1f, 1f);
                    break;

                case SlotEnums.None:
                    targetColor = ConfigHelper.GetSlotColor();
                    break;
                default:
                    float wave = Mathf.SmoothStep(0f, 1f, Mathf.Sin(time * Mathf.PI * 2f) * 0.5f + 0.5f);
                    targetColor = Color.Lerp(InventoryFrames.CurrentGradientStartColor, InventoryFrames.CurrentGradientEndColor, wave);
                    break;
            }
        }

        inputField.caretColor = targetColor;

        if (inputField.textComponent != null)
        {
            Color textColor = InventoryFrames.CurrentSlotColorMode switch
            {
                SlotEnums.Rainbow => new Color(1f - targetColor.r, 1f - targetColor.g, 1f - targetColor.b, targetColor.a),
                SlotEnums.None => HUDUtils.ParseHexColor(Plugins.ConfigEntries.ChatInputText.Value),
                _ => new Color(1f - targetColor.r, 1f - targetColor.g, 1f - targetColor.b, targetColor.a),
            };
            inputField.textComponent.color = textColor;
        }
        if (inputField.placeholder is TMP_Text placeholderText)
        {
            placeholderText.color = targetColor;
        }
    }
}