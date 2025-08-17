using LethalHUD.Configs;
using Unity.Netcode;
using UnityEngine;

namespace LethalHUD.Misc;
public class FPSAndPingCounter : NetworkBehaviour
{
    private float deltaTime;
    private GUIStyle style;
    private ulong currentPing = 0;
    private float pingTimer = 0f;
    private readonly float pingInterval = 0.5f;

    private Unity.Netcode.Transports.UTP.UnityTransport transport;

    private void Start()
    {
        style = new GUIStyle
        {
            fontSize = 20
        };

        transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as Unity.Netcode.Transports.UTP.UnityTransport;
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
    }

    private void OnGUI()
    {
        if (!Plugins.ConfigEntries.ShowFPSCounter.Value && !Plugins.ConfigEntries.ShowPingCounter.Value)
            return;

        style.normal.textColor = ConfigHelper.GetSlotColor();

        float x = Plugins.ConfigEntries.FPSCounterX.Value;
        float y = Plugins.ConfigEntries.FPSCounterY.Value;
        int lineHeight = 22;

        if (Plugins.ConfigEntries.ShowFPSCounter.Value)
        {
            float fps = 1.0f / deltaTime;
            GUI.Label(new Rect(x, y, 200, 30), $"FPS: {fps:0}", style);
            y += lineHeight;
        }

        if (Plugins.ConfigEntries.ShowPingCounter.Value)
        {
            GUI.Label(new Rect(x, y, 200, 30), $"Ping: {currentPing} ms", style);
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