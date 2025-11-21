using LethalHUD.Compats;
using LethalHUD.Misc;
using LethalHUD.Scan;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalHUD.HUD;
internal class LethalHUDMono : MonoBehaviour
{
    private bool _togglePressed = false;
    private HUDManager _hud;
    private Keyboard _keyboard;
    private Key ToggleKey => Plugins.ConfigEntries.HideHUDButton.Value;

    private void Awake()
    {
        _hud = HUDManager.Instance;
        _keyboard = Keyboard.current;
        PlanetInfoDisplay.Init();
    }
    private void Update()
    {
        if (_keyboard == null) return;

        Key key = ToggleKey;

        if (key == Key.None || !Enum.IsDefined(typeof(Key), key)) return;

        if (_keyboard[key].wasPressedThisFrame)
        {
            _togglePressed = !_togglePressed;
            _hud?.HideHUD(_togglePressed);
        }
    }

    // Figure out what's causing fps drops

    private void LateUpdate()
    {
        InventoryFrames.SetSlotColors();
        CompassController.SoftMaskStuff();
        ChatController.ColorChatInputField(HUDManager.Instance.chatTextField, Time.time * 0.25f);
        ScrapValueDisplay.Tick(Time.deltaTime);
        WeightController.RecolorWeightText();
        if (ModCompats.IsGoodItemScanPresent && Plugins.ConfigEntries.ScanNodeFade.Value)
            ScanNodeController.UpdateGoodItemScanNodes();
        if (ModCompats.IsBetterScanVisionPresent)
            BetterScanVisionProxy.OverrideNightVisionColor();
        if (ModCompats.IsEladsHUDPresent)
            EladsHUDProxy.OverrideHUD();
        if (_hud.loadingText != null)
        {
            HUDUtils.AnimateLoadingText(_hud.loadingText, Plugins.ConfigEntries.LoadingTextColor.Value);
        }
        PlanetInfoDisplay.UpdateColors();
        ControlTipController.ApplyColor();
        PlanetInfoDisplay.HeaderAndFooterAndHazardLevel();
        SignalTranslatorController.ApplyInMono();
    }
}