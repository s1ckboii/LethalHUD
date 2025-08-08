using LethalHUD.Configs;
using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace LethalHUD.HUD
{
    public class ChatController : NetworkBehaviour
    {
        private static readonly Dictionary<int, string> playerColors = new();
        private static readonly Dictionary<string, int> nameToId = new();

        public static ChatController Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            if (IsLocalPlayer)
            {
                int localId = (int)NetworkManager.Singleton.LocalClientId;
                SetPlayerColor(localId, Plugins.ConfigEntries.LocalNameColor.Value);
                SendColorToServer(Plugins.ConfigEntries.LocalNameColor.Value);
            }
        }
        public void UpdateLocalColor()
        {
            if (IsLocalPlayer)
            {
                int localId = (int)NetworkManager.Singleton.LocalClientId;
                SetPlayerColor(localId, Plugins.ConfigEntries.LocalNameColor.Value);
                SendColorToServer(Plugins.ConfigEntries.LocalNameColor.Value);
            }
        }

        public void SendColorToServer(string hexColor)
        {
            if (IsClient && !IsServer)
            {
                SendColorServerRpc(hexColor);
            }
            else if (IsServer)
            {
                int localId = (int)NetworkManager.Singleton.LocalClientId;
                SetPlayerColor(localId, hexColor);
                BroadcastColorClientRpc(localId, hexColor);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendColorServerRpc(string hexColor, ServerRpcParams rpcParams = default)
        {
            int senderId = (int)rpcParams.Receive.SenderClientId;
            SetPlayerColor(senderId, hexColor);
            BroadcastColorClientRpc(senderId, hexColor);
        }

        [ClientRpc]
        private void BroadcastColorClientRpc(int playerId, string hexColor)
        {
            SetPlayerColor(playerId, hexColor);
        }
        public static void RefreshPlayerCache()
        {
            nameToId.Clear();
            var players = StartOfRound.Instance.allPlayerScripts;
            for (int i = 0; i < players.Length; i++)
            {
                if (!string.IsNullOrEmpty(players[i].playerUsername))
                {
                    int id = (int)players[i].playerClientId;
                    nameToId[players[i].playerUsername] = id;
                }
            }
        }

        public static void SetPlayerColor(int playerId, string hexColor)
        {
            playerColors[playerId] = hexColor;
        }

        public static string GetPlayerColor(int playerId)
        {
            return playerColors.TryGetValue(playerId, out string color) ? color : null;
        }

        public static string GetColoredPlayerName(string playerName)
        {
            if (!Plugins.ConfigEntries.NameColors.Value)
                return playerName;

            if (!nameToId.TryGetValue(playerName, out var playerId))
                return playerName;

            var colorHex = GetPlayerColor(playerId);
            if (string.IsNullOrEmpty(colorHex))
                return playerName;

            return $"<color={colorHex}>{playerName}</color>";
        }
    }
}
