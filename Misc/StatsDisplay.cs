using LethalHUD.HUD;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using static LethalHUD.Enums;
using Unity.Netcode.Transports.UTP;

namespace LethalHUD.Misc;
public class StatsDisplay : NetworkBehaviour
{
    private float _deltaTime;
    private ulong _currentPing;
    private float _pingTimer;

    private TextMeshProUGUI _statsText;
    private UnityTransport _transport;

    private string _lastText = "";
    private MTColorMode _lastMode;
    private string _lastHexA = "";
    private string _lastHexB = "";
    private bool _lastSplit;
    private string _lastSeparateHex = "";

    private void Start()
    {
        _transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as Unity.Netcode.Transports.UTP.UnityTransport;

        GameObject go = new("StatsDisplay");
        GameObject ipHUD = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/");
        go.transform.SetParent(ipHUD.transform, false);
        _statsText = go.AddComponent<TextMeshProUGUI>();

        _statsText.font = HUDManager.Instance.chatText.font;
        _statsText.fontSize = 12;
        _statsText.richText = true;
        _statsText.alignment = TextAlignmentOptions.TopLeft;

        Material mat = Instantiate(_statsText.fontMaterial);
        mat.EnableKeyword("UNDERLAY_ON");
        mat.SetColor("_UnderlayColor", Color.black);
        mat.SetFloat("_UnderlayOffsetX", 1.2f);
        mat.SetFloat("_UnderlayOffsetY", -1.2f);
        mat.SetFloat("_UnderlaySoftness", 0.35f);
        _statsText.fontMaterial = mat;

        RectTransform rt = _statsText.rectTransform;
        rt.anchorMin = new(0, 1);
        rt.anchorMax = new(0, 1);
        rt.pivot = new(0, 1);
        rt.anchoredPosition = new(Plugins.ConfigEntries.FPSCounterX.Value, -Plugins.ConfigEntries.FPSCounterY.Value);
    }

    private void Update()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;

        HandlePing();
        UpdateStatsText();
    }

    #region Ping Handling
    private void HandlePing()
    {
        if (!IsOwner || _transport == null) return;

        _pingTimer += Time.deltaTime;
        if (_pingTimer >= 0.5f)
        {
            _pingTimer = 0f;
            RequestPingServerRpc();
        }
    }
    #endregion

    #region Stats Display
    private void UpdateStatsText()
    {
        if (!Plugins.ConfigEntries.ShowFPSDisplay.Value &&
            !Plugins.ConfigEntries.ShowPingDisplay.Value &&
            !Plugins.ConfigEntries.ShowSeedDisplay.Value)
        {
            _statsText.text = "";
            _lastText = "";
            return;
        }

        List<string> parts = [];

        if (Plugins.ConfigEntries.ShowFPSDisplay.Value)
        {
            int fps = Mathf.RoundToInt(1f / _deltaTime);
            parts.Add($"FPS: {fps}");
        }

        if (Plugins.ConfigEntries.ShowPingDisplay.Value)
            parts.Add($"Ping: {_currentPing} ms");

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

        bool split = Plugins.ConfigEntries.SplitAdditionalMTFromToolTips.Value;
        string separateHex = Plugins.ConfigEntries.SeperateAdditionalMiscToolsColors.Value;

        if (currentText != _lastText ||
            Plugins.ConfigEntries.MTColorSelection.Value != _lastMode ||
            Plugins.ConfigEntries.MTColorGradientA.Value != _lastHexA ||
            Plugins.ConfigEntries.MTColorGradientB.Value != _lastHexB ||
            split != _lastSplit ||
            separateHex != _lastSeparateHex)
        {
            _statsText.text = currentText;
            _statsText.alignment = TextAlignmentOptions.TopLeft;
            _statsText.rectTransform.anchoredPosition = new(Plugins.ConfigEntries.FPSCounterX.Value, -Plugins.ConfigEntries.FPSCounterY.Value);
            _statsText.enableWordWrapping = false;

            ApplyTextColor(_statsText);

            _lastText = currentText;
            _lastMode = Plugins.ConfigEntries.MTColorSelection.Value;
            _lastHexA = Plugins.ConfigEntries.MTColorGradientA.Value;
            _lastHexB = Plugins.ConfigEntries.MTColorGradientB.Value;
            _lastSplit = split;
            _lastSeparateHex = separateHex;
        }
    }

    private void ApplyTextColor(TextMeshProUGUI tmp)
    {
        bool useSeparate = Plugins.ConfigEntries.SplitAdditionalMTFromToolTips.Value;

        string hexA = useSeparate
            ? Plugins.ConfigEntries.SeperateAdditionalMiscToolsColors.Value
            : Plugins.ConfigEntries.MTColorGradientA.Value;
        string hexB = useSeparate
            ? Plugins.ConfigEntries.SeperateAdditionalMiscToolsColors.Value
            : Plugins.ConfigEntries.MTColorGradientB.Value;

        switch (Plugins.ConfigEntries.MTColorSelection.Value)
        {
            case MTColorMode.Solid:
                tmp.color = HUDUtils.ParseHexColor(hexA, Color.white);
                break;

            case MTColorMode.Gradient:
                if (HUDUtils.HasCustomGradient(hexA, hexB) && !useSeparate)
                    ApplyGradient(tmp, hexA, hexB);
                else
                    tmp.color = HUDUtils.ParseHexColor(hexA, Color.white);
                break;
        }
    }

    private void ApplyGradient(TextMeshProUGUI tmp, string hexA, string hexB)
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

            TMP_MeshInfo meshInfo = textInfo.meshInfo[charInfo.materialReferenceIndex];
            int vertexIndex = charInfo.vertexIndex;

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
    #endregion

    [ServerRpc(RequireOwnership = false)]
    private void RequestPingServerRpc(ServerRpcParams rpcParams = default)
    {
        if (_transport == null) return;

        ulong clientId = rpcParams.Receive.SenderClientId;
        ulong rtt = _transport.GetCurrentRtt(clientId);
        SendPingClientRpc(rtt, new ClientRpcParams
        {
            Send = new ClientRpcSendParams { TargetClientIds = [clientId] }
        });
    }

    [ClientRpc]
    private void SendPingClientRpc(ulong ping, ClientRpcParams rpcParams = default)
    {
        _currentPing = ping;
    }
}
