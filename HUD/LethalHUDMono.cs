using LethalHUD.Compats;
using LethalHUD.Scan;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LethalHUD.HUD;
internal class LethalHUDMono : MonoBehaviour
{
    private bool togglePressed = false;
    private HUDManager hud;
    private Keyboard keyboard;

    private void Awake()
    {
        hud = HUDManager.Instance;
        keyboard = Keyboard.current;
    }
    private void Update()
    {
        if (keyboard == null) return;
        if (keyboard.numpad5Key.wasPressedThisFrame)
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
        if (Plugins.ConfigEntries.WeightCounterBoolean.Value)
            WeightController.RecolorWeightText();
        if (ModCompats.IsGoodItemScanPresent && Plugins.ConfigEntries.ScanNodeFade.Value)
            ScanNodeController.UpdateGoodItemScanNodes();
        if (ModCompats.IsBetterScanVisionPresent)
            BetterScanVisionProxy.OverrideNightVisionColor();
    }
}