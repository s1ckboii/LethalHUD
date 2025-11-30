using GameNetcodeStuff;
using System;
using Unity.Netcode;
using UnityEngine;

namespace LethalHUD.HUD;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerControllerB))]
internal class PlayerColorNetworker : NetworkBehaviour
{
    /// <summary>
    ///     Each player possesses a <c>PlayerColorInfo</c> <c>NetworkVariable</c> that they can write to, which is then synced with all other clients.
    /// </summary>
    private readonly NetworkVariable<PlayerColorInfo> _syncedPlayerColors = new(writePerm: NetworkVariableWritePermission.Owner);
    public PlayerColorInfo PlayerColors { get; private set; } // Local color information for the player.

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Update local color information when the synced value changes.
        _syncedPlayerColors.OnValueChanged += (previousValue, newValue) => PlayerColors = newValue;

        // Refresh player colors upon spawning.
        RefreshColors();
    }

    protected override void OnOwnershipChanged(ulong previous, ulong current)
    {
        base.OnOwnershipChanged(previous, current);

        // Refresh player colors upon changing ownership (player joining lobby).
        RefreshColors();
    }

    private void RefreshColors()
    {
        if (IsOwner)
        {
            string colorAHex = Plugins.ConfigEntries.GradientNameColorA.Value,
                colorBHex = Plugins.ConfigEntries.GradientNameColorB.Value;

            PlayerColors = new(ColorUtility.TryParseHtmlString(colorAHex, out Color colorA) ? colorA : Color.red,
                ColorUtility.TryParseHtmlString(colorBHex, out Color colorB) ? colorB : null);

            _syncedPlayerColors.Value = PlayerColors;
        }

        PlayerColors = _syncedPlayerColors.Value;
    }

    internal static void RefreshColors(object obj, EventArgs args)
    {
        if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.localPlayerController != null
            && GameNetworkManager.Instance.localPlayerController.TryGetComponent(out PlayerColorNetworker playerNetworkManager))
        {
            playerNetworkManager.RefreshColors();
        }
    }
}

public struct PlayerColorInfo(Color32 a, Color32? b = null) : INetworkSerializable, IEquatable<PlayerColorInfo>
{
    public Color32 colorA = a;
    public Color32 colorB = b ?? a;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref colorA);
        serializer.SerializeValue(ref colorB);
    }

    public readonly bool Equals(PlayerColorInfo other)
    {
        return colorA.Equals(other.colorA) && colorB.Equals(other.colorB);
    }

    public override readonly bool Equals(object obj)
    {
        return obj is PlayerColorInfo info && Equals(info);
    }

    public static bool operator ==(PlayerColorInfo left, PlayerColorInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PlayerColorInfo left, PlayerColorInfo right)
    {
        return !(left == right);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(colorA, colorB);
    }
}