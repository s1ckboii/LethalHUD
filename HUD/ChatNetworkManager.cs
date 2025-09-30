/*using Unity.Netcode;
using System.Collections.Generic;

namespace LethalHUD.HUD;

internal class ChatNetworkManager : NetworkBehaviour
{
    private readonly Dictionary<ulong, PlayerColorInfo> hostPlayerColors = [];
    public static ChatNetworkManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public static void SendColorToServer(PlayerColorInfo info)
    {
        if (NetworkManager.Singleton.LocalClientId < 0) return;
        Instance.SetPlayerColorServerRpc(NetworkManager.Singleton.LocalClientId, info);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerColorServerRpc(ulong clientId, PlayerColorInfo info)
    {
        // Update host dictionary
        hostPlayerColors[clientId] = info;

        Loggers.Fatal($"[ChatNetworkManager] Server received color for client {clientId}: {info.colorA}, {info.colorB}");

        // Send to all clients
        SetPlayerColorClientRpc(clientId, info);
    }

    [ClientRpc]
    private void SetPlayerColorClientRpc(ulong clientId, PlayerColorInfo info)
    {
        if (!ChatController.ColoringEnabled) return;

        Loggers.Fatal($"[ChatNetworkManager] Client received color for client {clientId}: {info.colorA}, {info.colorB}");

        int baseGameId = NetworkIdToPlayerIndex(clientId);
        Loggers.Fatal($"[ChatNetworkManager] Mapped clientId {clientId} to player index {baseGameId}");
        if (baseGameId == -1) return;

        ChatController.SetPlayerColor(baseGameId, info.colorA, info.colorB);
        Loggers.Fatal($"[ChatNetworkManager] Applied color to player index {baseGameId}");
    }

    private int NetworkIdToPlayerIndex(ulong clientId)
    {
        var allPlayers = StartOfRound.Instance.allPlayerScripts;
        for (int i = 0; i < allPlayers.Length; i++)
        {
            if ((ulong)allPlayers[i].playerClientId == clientId)
                return i;
        }
        return -1;
    }
}

public struct PlayerColorInfo(string a, string b = null) : INetworkSerializable
{
    public string colorA = a;
    public string colorB = b;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref colorA);
        serializer.SerializeValue(ref colorB);
    }
}*/