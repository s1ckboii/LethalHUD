using LethalHUD.Compats;
using LethalHUD.Misc;
using LethalHUD.Scan;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalHUD.HUD
{
    internal class LethalHUDMono : MonoBehaviour
    {
        public static LethalHUDMono Instance { get; private set; }

        private bool _togglePressed;
        private bool _validToggleKey;
        private Key _currentKey;

        private HUDManager _hud;
        private Keyboard _keyboard;

        private void Awake()
        {
            _hud = HUDManager.Instance;
            _keyboard = Keyboard.current;

            PlanetInfoDisplay.Init();

            UpdateToggleKey();
        }

        internal void UpdateToggleKey()
        {
            _currentKey = Plugins.ConfigEntries.HideHUDButton.Value;
            _validToggleKey = _currentKey != Key.None && Enum.IsDefined(typeof(Key), _currentKey);
        }

        private void Update()
        {
            if (!_validToggleKey || _keyboard == null)
                return;

            if (_keyboard[_currentKey].wasPressedThisFrame)
            {
                _togglePressed = !_togglePressed;
                _hud?.HideHUD(_togglePressed);
            }
        }

        private void LateUpdate()
        {
            InventoryFrames.SetSlotColors();
            CompassController.SoftMaskStuff();
            ChatController.ColorChatInputField(_hud?.chatTextField, Time.time * 0.25f);
            ScrapValueDisplay.Tick(Time.deltaTime);
            WeightController.RecolorWeightText();
            if (ModCompats.IsBetterScanVisionPresent)
                BetterScanVisionProxy.OverrideNightVisionColor();
            if (ModCompats.IsEladsHUDPresent)
                EladsHUDProxy.OverrideHUD();
            if (_hud.loadingText != null)
            {
                HUDUtils.ColorLoadingText(_hud.loadingText.transform.parent, Plugins.ConfigEntries.LoadingScreenColor.Value);
            }
            PlanetInfoDisplay.UpdateColors();
            ControlTipController.ApplyColor();
            PlanetInfoDisplay.HeaderAndFooterAndHazardLevel();
            SignalTranslatorController.ApplyInMono();
            ScanNodeTextureManager.Tick(_hud?.scanNodes);
        }
    }
}