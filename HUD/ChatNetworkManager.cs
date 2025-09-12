/*
using LethalHUD.HUD;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

namespace LethalHUD.HUD;

internal class ChatNetworkManager : NetworkBehaviour
{
    private readonly Dictionary<ulong, ChatController.PlayerColorInfo> hostPlayerColors = [];
    public static ChatNetworkManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public static void SendColorToServer(string colorA, string colorB = null)
    {
        if (NetworkManager.Singleton.LocalClientId < 0) return;
        Instance.SetPlayerColorServerRpc(NetworkManager.Singleton.LocalClientId, colorA, colorB);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerColorServerRpc(ulong clientId, string colorA, string colorB = null)
    {
        bool gradient = !string.IsNullOrEmpty(colorB) && !string.Equals(colorA, colorB, System.StringComparison.OrdinalIgnoreCase);
        var info = new ChatController.PlayerColorInfo(colorA, colorB, gradient);

        // Update host dictionary
        hostPlayerColors[clientId] = info;

        // Send to all clients
        SetPlayerColorClientRpc(clientId, colorA, colorB);
    }

    [ClientRpc]
    private void SetPlayerColorClientRpc(ulong clientId, string colorA, string colorB = null)
    {
        if (!ChatController.ColoringEnabled) return;

        int id = (int)clientId; // convert ulong -> int for ChatController
        ChatController.SetPlayerColor(id, colorA, colorB);
    }
}
*/