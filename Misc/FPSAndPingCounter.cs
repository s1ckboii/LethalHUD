using LethalHUD.Configs;
using LethalHUD.HUD;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.Misc;
public class FPSAndPingCounter : NetworkBehaviour
{
    private float deltaTime;
    private ulong currentPing = 0;
    private float pingTimer = 0f;
    private readonly float pingInterval = 0.5f;
    private Vector2 offset = Vector2.zero;

    private Color TextColor => HUDUtils.ParseHexColor(Plugins.ConfigEntries.MiscToolsColor.Value);

    private Unity.Netcode.Transports.UTP.UnityTransport transport;
    private TextMeshProUGUI fpsPingText;

    private void Start()
    {
        transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as Unity.Netcode.Transports.UTP.UnityTransport;

        GameObject go = new("FPSAndPingCounter");
        GameObject ipHUD = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/");
        go.transform.SetParent(ipHUD.transform, false);
        fpsPingText = go.AddComponent<TextMeshProUGUI>();

        fpsPingText.font = HUDManager.Instance.chatText.font;
        fpsPingText.fontSize = 12;
        fpsPingText.richText = true;
        fpsPingText.color = TextColor;
        fpsPingText.alignment = TextAlignmentOptions.TopLeft;

        RectTransform rt = fpsPingText.rectTransform;
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

        if (Plugins.ConfigEntries.ShowFPSCounter.Value || Plugins.ConfigEntries.ShowPingCounter.Value)
        {
            float fps = 1.0f / deltaTime;
            string text = "";
            string separator = Plugins.ConfigEntries.MiscLayoutEnum.Value == FPSPingLayout.Vertical
                ? "\n─────────\n"
                : " | ";

            if (Plugins.ConfigEntries.ShowFPSCounter.Value)
                text += $"FPS: {fps:0}";

            if (Plugins.ConfigEntries.ShowPingCounter.Value)
            {
                if (!string.IsNullOrEmpty(text))
                    text += separator;
                text += $"Ping: {currentPing} ms";
            }

            fpsPingText.alignment = TextAlignmentOptions.TopLeft;
            offset = new Vector2(Plugins.ConfigEntries.FPSCounterX.Value, -Plugins.ConfigEntries.FPSCounterY.Value);

            fpsPingText.text = text;
            fpsPingText.color = TextColor;
            fpsPingText.rectTransform.anchoredPosition = offset;
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