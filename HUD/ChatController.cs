using LethalHUD.Configs;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.HUD;
internal static class ChatController
{
    //private static readonly Dictionary<int, PlayerColorInfo> playerColors = [];
    private static readonly Dictionary<string, string> localPlayerColors = [];
    internal static bool ColoringEnabled => Plugins.ConfigEntries.ColoredNames.Value;

    internal static string NoPunctuation(string input)
    {
        return new string([.. input.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '.')]);
    }

    /*internal static void SetPlayerColor(int playerId, string colorA, string colorB = null)
    {
        playerColors[playerId] = new PlayerColorInfo(colorA, colorB);
    }*/

    internal static void ApplyLocalPlayerColor(string colorA, string colorB = null)
    {
        const string localKey = "LocalPlayer";

        if (string.IsNullOrEmpty(colorA) || string.IsNullOrEmpty(colorB))
            return;

        localPlayerColors[localKey] = colorA;
        localPlayerColors[localKey + "_B"] = colorB;

        /*var info = new PlayerColorInfo(colorA, colorB);
        int localId = (int)NetworkManager.Singleton.LocalClientId;
        SetPlayerColor(localId, colorA, colorB);

        ChatNetworkManager.SendColorToServer(info);*/
    }

    /*internal static string GetColoredPlayerName(string playerName, int playerId = -1)
    {
        if (!ColoringEnabled || string.IsNullOrEmpty(playerName))
            return playerName;

        if (playerId != -1 && playerColors.TryGetValue(playerId, out PlayerColorInfo info))
        {
            ColorUtility.TryParseHtmlString(info.colorA, out Color colorA);
            ColorUtility.TryParseHtmlString(info.colorB, out Color colorB);
            return HUDUtils.ApplyStaticGradient(playerName, colorA, colorB);
        }
        return $"<color=#FF0000>{playerName}</color>";
    }*/

    internal static string GetColoredPlayerName(string playerName)
    {
        if (!ColoringEnabled || string.IsNullOrEmpty(playerName))
            return $"<color=#FF0000>{playerName}</color>";

        const string localKey = "LocalPlayer";

        if (localPlayerColors.TryGetValue(localKey, out string colorA) &&
            localPlayerColors.TryGetValue(localKey + "_B", out string colorB))
        {
            ColorUtility.TryParseHtmlString(colorA, out Color startColor);
            ColorUtility.TryParseHtmlString(colorB, out Color endColor);
            return HUDUtils.ApplyStaticGradient(playerName, startColor, endColor);
        }

        ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientNameColorA.Value, out Color globalStart);
        ColorUtility.TryParseHtmlString(Plugins.ConfigEntries.GradientNameColorB.Value, out Color globalEnd);

        return HUDUtils.ApplyStaticGradient(playerName, globalStart, globalEnd);
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
                HUDUtils.ApplyVertexGradient(indicator, startColor, endColor, Time.time * 0.2f);
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
                HUDUtils.ApplyVertexGradient(indicator, InventoryFrames.CurrentGradientStartColor, InventoryFrames.CurrentGradientEndColor, Time.time * 0.25f);
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