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
    private Vector2 offset = Vector2.zero;

    private Color TextColor => HUDUtils.ParseHexColor(Plugins.ConfigEntries.MiscToolsColor.Value);

    private Unity.Netcode.Transports.UTP.UnityTransport transport;
    private TextMeshProUGUI statsText;

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
        statsText.color = TextColor;
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

            // FPS
            if (Plugins.ConfigEntries.ShowFPSDisplay.Value)
            {
                float fps = 1.0f / deltaTime;
                parts.Add($"FPS: {fps:0}");
            }

            // Ping
            if (Plugins.ConfigEntries.ShowPingDisplay.Value)
            {
                parts.Add($"Ping: {currentPing} ms");
            }

            // Seed
            if (Plugins.ConfigEntries.ShowSeedDisplay.Value &&
                StartOfRound.Instance != null &&
                !StartOfRound.Instance.inShipPhase)
            {
                int seed = StartOfRound.Instance.randomMapSeed;
                parts.Add($"Seed: {seed}");
            }

            string separator = Plugins.ConfigEntries.MiscLayoutEnum.Value == FPSPingLayout.Vertical
                ? "\n─────────\n"
                : " | ";

            statsText.text = string.Join(separator, parts);
            statsText.color = TextColor;
            statsText.alignment = TextAlignmentOptions.TopLeft;
            statsText.rectTransform.anchoredPosition =
                new Vector2(Plugins.ConfigEntries.FPSCounterX.Value, -Plugins.ConfigEntries.FPSCounterY.Value);
            statsText.enableWordWrapping = false;
        }
        else
        {
            statsText.text = "";
        }
    }

    [ClientRpc]
    private void SendPingClientRpc(ulong targetClientId, ulong ping)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId)
        {
            currentPing = ping;
        }
    }
}
