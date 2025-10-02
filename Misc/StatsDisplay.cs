using LethalHUD.HUD;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using static LethalHUD.Enums;

namespace LethalHUD.Misc;
public class StatsDisplay : NetworkBehaviour
{
    private float deltaTime;
    private ulong currentPing = 0;
    private float pingTimer = 0f;
    private readonly float pingInterval = 0.5f;

    private TextMeshProUGUI statsText;
    private Unity.Netcode.Transports.UTP.UnityTransport transport;

    private string lastText = "";
    private MTColorMode lastMode;
    private string lastHexA = "";
    private string lastHexB = "";

    private void Start()
    {
        transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as Unity.Netcode.Transports.UTP.UnityTransport;

        GameObject go = new("StatsDisplay");
        GameObject ipHUD = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/");
        go.transform.SetParent(ipHUD.transform, false);
        statsText = go.AddComponent<TextMeshProUGUI>();

        statsText.font = HUDManager.Instance.chatText.font;
        statsText.fontSize = 12;
        statsText.richText = true;
        statsText.alignment = TextAlignmentOptions.TopLeft;

        RectTransform rt = statsText.rectTransform;
        rt.anchorMin = new(0, 1);
        rt.anchorMax = new(0, 1);
        rt.pivot = new(0, 1);
        rt.anchoredPosition = new(Plugins.ConfigEntries.FPSCounterX.Value, -Plugins.ConfigEntries.FPSCounterY.Value);
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        if (IsHost && transport != null)
        {
            pingTimer += Time.deltaTime;
            if (pingTimer >= pingInterval)
            {
                pingTimer = 0f;
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    ulong rtt = transport.GetCurrentRtt(client.ClientId);
                    SendPingClientRpc(client.ClientId, rtt);
                }
            }
        }

        if (Plugins.ConfigEntries.ShowFPSDisplay.Value ||
            Plugins.ConfigEntries.ShowPingDisplay.Value ||
            Plugins.ConfigEntries.ShowSeedDisplay.Value)
        {
            List<string> parts = [];

            if (Plugins.ConfigEntries.ShowFPSDisplay.Value)
            {
                int fps = Mathf.RoundToInt(1.0f / deltaTime);
                parts.Add($"FPS: {fps}");
            }

            if (Plugins.ConfigEntries.ShowPingDisplay.Value)
                parts.Add($"Ping: {currentPing} ms");

            if (Plugins.ConfigEntries.ShowSeedDisplay.Value &&
                StartOfRound.Instance != null &&
                !StartOfRound.Instance.inShipPhase)
            {
                parts.Add($"Seed: {StartOfRound.Instance.randomMapSeed}");
            }

            string separator = Plugins.ConfigEntries.MiscLayoutEnum.Value == FPSPingLayout.Vertical
                ? "\n─────────\n"
                : " | ";

            string currentText = string.Join(separator, parts);

            if (currentText != lastText ||
                Plugins.ConfigEntries.MTColorSelection.Value != lastMode || Plugins.ConfigEntries.MTColorGradientA.Value != lastHexA || Plugins.ConfigEntries.MTColorGradientB.Value != lastHexB)
            {
                statsText.text = currentText;
                statsText.alignment = TextAlignmentOptions.TopLeft;
                statsText.rectTransform.anchoredPosition =
                    new Vector2(Plugins.ConfigEntries.FPSCounterX.Value, -Plugins.ConfigEntries.FPSCounterY.Value);
                statsText.enableWordWrapping = false;

                ApplyTextColor(statsText);

                lastText = currentText;
                lastMode = Plugins.ConfigEntries.MTColorSelection.Value;
                lastHexA = Plugins.ConfigEntries.MTColorGradientA.Value;
                lastHexB = Plugins.ConfigEntries.MTColorGradientB.Value;
            }
        }
        else
        {
            statsText.text = "";
            lastText = "";
        }
    }

    private void ApplyTextColor(TextMeshProUGUI tmp)
    {
        string hexA = Plugins.ConfigEntries.MTColorGradientA.Value;
        string hexB = Plugins.ConfigEntries.MTColorGradientB.Value;

        switch (Plugins.ConfigEntries.MTColorSelection.Value)
        {
            case MTColorMode.Solid:
                tmp.color = HUDUtils.ParseHexColor(hexA, Color.white);
                break;

            case MTColorMode.Gradient:
                if (HUDUtils.HasCustomGradient(hexA, hexB))
                {
                    tmp.ForceMeshUpdate();
                    TMP_TextInfo textInfo = tmp.textInfo;
                    int charCount = textInfo.characterCount;
                    if (charCount == 0) return;

                    Color colorA = HUDUtils.ParseHexColor(hexA, Color.white);
                    Color colorB = HUDUtils.ParseHexColor(hexB, Color.white);

                    for (int i = 0; i < charCount; i++)
                    {
                        TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                        if (!charInfo.isVisible) continue;

                        float t = (charCount > 1) ? i / (float)(charCount - 1) : 0f;
                        Color charColor = Color.Lerp(colorA, colorB, t);

                        int vertexIndex = charInfo.vertexIndex;
                        var meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];

                        meshInfo.colors32[vertexIndex + 0] = charColor;
                        meshInfo.colors32[vertexIndex + 1] = charColor;
                        meshInfo.colors32[vertexIndex + 2] = charColor;
                        meshInfo.colors32[vertexIndex + 3] = charColor;
                    }

                    for (int i = 0; i < tmp.textInfo.meshInfo.Length; i++)
                    {
                        tmp.textInfo.meshInfo[i].mesh.colors32 = tmp.textInfo.meshInfo[i].colors32;
                        tmp.UpdateGeometry(tmp.textInfo.meshInfo[i].mesh, i);
                    }
                }
                else
                {
                    tmp.color = HUDUtils.ParseHexColor(hexA, Color.white);
                }
                break;
        }
    }

    [ClientRpc]
    private void SendPingClientRpc(ulong targetClientId, ulong ping)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
            currentPing = ping;
    }
}