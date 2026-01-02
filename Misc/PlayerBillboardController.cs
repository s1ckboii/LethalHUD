using GameNetcodeStuff;
using LethalHUD.HUD;
using TMPro;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.Misc;

[DisallowMultipleComponent]
internal class PlayerBillboardGradient : MonoBehaviour
{
    private static bool ColoringEnabled => Plugins.ConfigEntries.BillboardColor.Value;

    private PlayerControllerB _player;
    private TextMeshProUGUI _text;
    private CanvasGroup _canvasAlpha;
    private PlayerColorNetworker _networker;

    [Header("Performance Options")]
    public float maxUpdateDistance = 30f;
    public float updateInterval = 0.033f;

    private float _timeSinceLastUpdate;
    private PlayerColorInfo _currentColors;
    private bool _hasColors;

    private void Awake()
    {
        _player = GetComponent<PlayerControllerB>();
        if (_player == null) return;

        _text = _player.usernameBillboardText;
        _canvasAlpha = _player.usernameAlpha;
        _networker = _player.GetComponent<PlayerColorNetworker>();

        if (_text != null)
            _text.enableVertexGradient = true;
    }

    private void Start()
    {
        if (_networker == null)
            return;

        _currentColors = _networker._syncedPlayerColors.Value;
        _hasColors = true;

        _networker._syncedPlayerColors.OnValueChanged += OnPlayerColorsChanged;
    }

    private void OnPlayerColorsChanged(PlayerColorInfo previous, PlayerColorInfo current)
    {
        _currentColors = current;
        _hasColors = true;
    }

    private void LateUpdate()
    {
        if (!ColoringEnabled)
        {
            ResetBillboardToDefault();
            return;
        }

        if (!_text.enableVertexGradient)
            _text.enableVertexGradient = true;

        if (!_hasColors) return;
        if (_player == null || _text == null || _canvasAlpha == null) return;
        if (!_player.usernameCanvas.gameObject.activeSelf) return;
        if (_canvasAlpha.alpha < 0.95f) return;
        if (GameNetworkManager.Instance?.localPlayerController == null) return;

        float distSqr = (_player.transform.position - GameNetworkManager.Instance.localPlayerController.transform.position).sqrMagnitude;

        if (distSqr > maxUpdateDistance * maxUpdateDistance)
            return;

        _timeSinceLastUpdate += Time.deltaTime;
        if (_timeSinceLastUpdate < updateInterval)
            return;

        _timeSinceLastUpdate = 0f;

        _currentColors = _networker._syncedPlayerColors.Value;

        switch (_currentColors.billboardMode)
        {
            case BillboardGradientMode.Static:
                HUDUtils.ApplyStaticVertexGradient(_text, _currentColors.colorA, _currentColors.colorB, _currentColors.billboardLayout);
                break;

            case BillboardGradientMode.Wave:
                HUDUtils.ApplyWaveVertexGradient(_text, _currentColors.colorA, _currentColors.colorB, Time.time / 2f, _currentColors.billboardLayout);
                break;

            case BillboardGradientMode.Pulse:
                HUDUtils.ApplyPulsingVertexGradient(_text, _currentColors.colorA, _currentColors.colorB, Time.time / 2f, _currentColors.billboardLayout);
                break;
        }
    }
    private void ResetBillboardToDefault()
    {
        if (_text == null) return;

        _text.enableVertexGradient = false;

        _text.color = Color.white;
    }
}
