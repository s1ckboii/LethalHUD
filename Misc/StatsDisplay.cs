using LethalHUD.HUD;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using static LethalHUD.Enums;

namespace LethalHUD.Misc;

public class StatsDisplay : NetworkBehaviour
{
    private float _deltaTime;
    private ulong _currentPing;

    private const float PingRefreshRate = 0.5f;
    private float _nextPingRefresh;

    private TextMeshProUGUI _statsText;

    private string _lastText = "";
    private MTColorMode _lastMode;
    private string _lastHexA = "";
    private string _lastHexB = "";
    private bool _lastSplit;
    private string _lastSeparateHex = "";

    private static readonly BindingFlags ReflectionFlags =
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic;

    private FieldInfo _connectionManagerField;
    private PropertyInfo _connectionProperty;
    private FieldInfo _connectionField;
    private MethodInfo _detailedStatusMethod;

    private void Start()
    {
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
        NetworkManager networkManager = NetworkManager.Singleton;

        if (networkManager == null || !networkManager.IsClient)
        {
            _currentPing = 0;
            return;
        }

        if (networkManager.IsHost)
        {
            _currentPing = 0;
            return;
        }

        if (Time.unscaledTime < _nextPingRefresh)
            return;

        _nextPingRefresh = Time.unscaledTime + PingRefreshRate;

        NetworkTransport transport = networkManager.NetworkConfig.NetworkTransport;

        if (transport == null)
        {
            _currentPing = 0;
            return;
        }

        if (transport.GetType().FullName == "Netcode.Transports.Facepunch.FacepunchTransport")
        {
            if (TryGetFacepunchPing(transport, out ulong ping))
                _currentPing = ping;
            else
                _currentPing = 0;

            return;
        }

        // Fallback for another transport that actually implements rtt..
        try
        {
            _currentPing = transport.GetCurrentRtt(transport.ServerClientId);
        }
        catch
        {
            _currentPing = 0;
        }
    }

    private bool TryGetFacepunchPing(NetworkTransport transport, out ulong ping)
    {
        ping = 0;

        try
        {
            Type transportType = transport.GetType();

            _connectionManagerField ??= transportType.GetField("connectionManager",ReflectionFlags);

            object connectionManager = _connectionManagerField?.GetValue(transport);

            if (connectionManager == null)
                return false;

            Type managerType = connectionManager.GetType();

            if (_connectionProperty == null && _connectionField == null)
            {
                _connectionProperty = managerType.GetProperty("Connection", ReflectionFlags);

                if (_connectionProperty == null)
                {
                    _connectionField = managerType.GetField("Connection", ReflectionFlags);
                }
            }

            object connection;

            if (_connectionProperty != null)
            {
                connection = _connectionProperty.GetValue(connectionManager);
            }
            else if (_connectionField != null)
            {
                connection = _connectionField.GetValue(connectionManager);
            }
            else
            {
                return false;
            }

            if (connection == null)
                return false;

            _detailedStatusMethod ??= connection.GetType().GetMethod("DetailedStatus", ReflectionFlags, null, Type.EmptyTypes, null);

            if (_detailedStatusMethod == null)
                return false;

            string status = _detailedStatusMethod.Invoke(connection, null) as string;

            return TryParseSteamPing(status, out ping);
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseSteamPing(string status, out ulong ping)
    {
        ping = 0;

        if (string.IsNullOrEmpty(status))
            return false;

        int index = status.IndexOf("Ping:", StringComparison.OrdinalIgnoreCase);

        if (index < 0)
            return false;

        index += "Ping:".Length;

        while (index < status.Length && char.IsWhiteSpace(status[index]))
        {
            index++;
        }

        bool foundNumber = false;

        while (index < status.Length)
        {
            char c = status[index];

            if (c < '0' || c > '9')
                break;

            foundNumber = true;
            ping = (ping * 10) + (ulong)(c - '0');
            index++;
        }

        return foundNumber;
    }

    #endregion

    #region Stats Display

    private void UpdateStatsText()
    {
        if (!Plugins.ConfigEntries.ShowFPSDisplay.Value && !Plugins.ConfigEntries.ShowPingDisplay.Value && !Plugins.ConfigEntries.ShowSeedDisplay.Value)
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

        if (Plugins.ConfigEntries.ShowSeedDisplay.Value && StartOfRound.Instance != null && !StartOfRound.Instance.inShipPhase)
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
            split != _lastSplit || separateHex != _lastSeparateHex)
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
                {
                    ApplyGradient(tmp, hexA, hexB);
                }
                else
                {
                    tmp.color = HUDUtils.ParseHexColor(hexA, Color.white);
                }

                break;
        }
    }

    private void ApplyGradient(TextMeshProUGUI tmp, string hexA, string hexB)
    {
        tmp.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmp.textInfo;
        int charCount = textInfo.characterCount;

        if (charCount == 0)
            return;

        Color colorA = HUDUtils.ParseHexColor(hexA, Color.white);
        Color colorB = HUDUtils.ParseHexColor(hexB, Color.white);

        for (int i = 0; i < charCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            float t = charCount > 1
                ? i / (float)(charCount - 1)
                : 0f;

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
}