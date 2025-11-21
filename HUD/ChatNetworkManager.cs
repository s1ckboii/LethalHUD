using Unity.Netcode;
using System.Collections.Generic;

namespace LethalHUD.HUD;

// Potentially using the new rpc params later for targeting specific clients
internal class ChatNetworkManager : NetworkBehaviour
{
    private readonly Dictionary<ulong, PlayerColorInfo> _hostPlayerColors = [];
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
        _hostPlayerColors[clientId] = info;

        // Using fatal cuz it's much more visible
        Loggers.Fatal($"[ChatNetworkManager] Server received color for client {clientId}: {info.colorA}, {info.colorB}");

        SetPlayerColorClientRpc(clientId, info);
    }

    [ClientRpc]
    private void SetPlayerColorClientRpc(ulong clientId, PlayerColorInfo info)
    {
        if (!ChatController.ColoringEnabled) return;

        if (StartOfRound.Instance == null || StartOfRound.Instance.allPlayerScripts == null || HUDManager.Instance == null)
        {
            StartCoroutine(DelayedApply(clientId, info));
            return;
        }

        ApplyColor(clientId, info);
    }

    private System.Collections.IEnumerator DelayedApply(ulong clientId, PlayerColorInfo info)
    {
        while (StartOfRound.Instance == null || StartOfRound.Instance.allPlayerScripts == null || HUDManager.Instance == null)
            yield return null;

        ApplyColor(clientId, info);
    }

    private void ApplyColor(ulong clientId, PlayerColorInfo info)
    {
        int baseGameId = NetworkIdToPlayerIndex(clientId);
        if (baseGameId == -1) return;

        ChatController.SetPlayerColor(baseGameId, info.colorA, info.colorB);
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
}