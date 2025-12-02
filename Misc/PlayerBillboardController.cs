using TMPro;
using UnityEngine;
using GameNetcodeStuff;
using LethalHUD.HUD;

[DisallowMultipleComponent]
internal class PlayerBillboardGradient : MonoBehaviour
{
    private PlayerControllerB _player;
    private TextMeshProUGUI _text;
    private CanvasGroup _canvasAlpha;
    private PlayerColorNetworker _networker;

    [Header("Performance Options")]
    public float maxUpdateDistance = 30f;
    public float updateInterval = 0.033f;   // ~30 FPS

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
        if (_networker == null) return;

        _currentColors = _networker._syncedPlayerColors.Value;
        _hasColors = true;

        _networker._syncedPlayerColors.OnValueChanged += OnPlayerColorsChanged;

        HUDUtils.ApplyVertexGradient(_text, _currentColors.colorA, _currentColors.colorB, Time.time);
    }

    private void OnPlayerColorsChanged(PlayerColorInfo previous, PlayerColorInfo current)
    {
        _currentColors = current;
        _hasColors = true;
        HUDUtils.ApplyVertexGradient(_text, _currentColors.colorA, _currentColors.colorB, Time.time);
    }

    private void LateUpdate()
    {
        if (_player == null || _text == null || _canvasAlpha == null) return;
        if (!_player.usernameCanvas.gameObject.activeSelf || _canvasAlpha.alpha < 1f) return;

        float distSqr = (_player.transform.position - GameNetworkManager.Instance.localPlayerController.transform.position).sqrMagnitude;
        if (distSqr > maxUpdateDistance * maxUpdateDistance) return;

        _timeSinceLastUpdate += Time.deltaTime;
        if (_timeSinceLastUpdate < updateInterval) return;
        _timeSinceLastUpdate = 0f;

        _currentColors = _networker._syncedPlayerColors.Value;

        HUDUtils.ApplyVertexGradient(_text, _currentColors.colorA, _currentColors.colorB, Time.time);
    }
}
