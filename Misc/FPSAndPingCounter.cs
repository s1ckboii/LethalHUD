using LethalHUD.Configs;
using UnityEngine;
using Unity.Netcode;

namespace LethalHUD.Misc;
public class FPSAndPingCounter : MonoBehaviour
{
    private float deltaTime;
    private GUIStyle style;

    private void Start()
    {
        style = new GUIStyle
        {
            fontSize = 20
        };
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        if (!ConfigEntries.ShowFPSCounter.Value && !ConfigEntries.ShowPingCounter.Value)
            return;

        style.normal.textColor = ConfigHelper.GetSlotColor();

        float x = ConfigEntries.FPSCounterX.Value;
        float y = ConfigEntries.FPSCounterY.Value;
        int lineHeight = 22;

        if (ConfigEntries.ShowFPSCounter.Value)
        {
            float fps = 1.0f / deltaTime;
            GUI.Label(new Rect(x, y, 200, 30), $"FPS: {fps:0}", style);
            y += lineHeight;
        }

        if (ConfigEntries.ShowPingCounter.Value)
        {
            string pingText = "Ping: N/A";
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient)
            {
                var transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as Unity.Netcode.Transports.UTP.UnityTransport;
                if (transport != null)
                    pingText = $"Ping: {transport.GetCurrentRtt(NetworkManager.Singleton.LocalClientId)} ms";
            }
            GUI.Label(new Rect(x, y, 200, 30), pingText, style);
        }
    }
}