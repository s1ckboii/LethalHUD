using LethalHUD.Compats;
using LethalHUD.Misc;
using LethalHUD.Scan;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalHUD.HUD;
internal class LethalHUDMono : MonoBehaviour
{
    private bool togglePressed = false;
    private HUDManager hud;
    private Keyboard keyboard;
    private Key ToggleKey => Plugins.ConfigEntries.HideHUDButton.Value;

    private void Awake()
    {
        hud = HUDManager.Instance;
        keyboard = Keyboard.current;
        PlanetInfoDisplay.Init();
    }
    private void Update()
    {
        if (keyboard == null) return;

        Key key = ToggleKey;

        if (key == Key.None || !Enum.IsDefined(typeof(Key), key)) return;

        if (keyboard[key].wasPressedThisFrame)
        {
            togglePressed = !togglePressed;
            hud?.HideHUD(togglePressed);
        }
    }

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
        if (hud.loadingText != null)
        {
            HUDUtils.AnimateLoadingText(hud.loadingText, Plugins.ConfigEntries.LoadingTextColor.Value);
        }
        PlanetInfoDisplay.UpdateColors();
        ControlTipController.ApplyColor();
        PlanetInfoDisplay.HeaderAndFooterAndHazardLevel();
        SignalTranslatorController.ApplyInMono();
    }
}