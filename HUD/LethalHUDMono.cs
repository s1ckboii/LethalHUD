using LethalHUD.Compats;
using LethalHUD.Scan;
using UnityEngine;

namespace LethalHUD.HUD;
internal class LethalHUDMono : MonoBehaviour
{
    private void LateUpdate()
    {
        InventoryFrames.SetSlotColors();
        ChatController.ColorChatInputField(HUDManager.Instance.chatTextField, Time.time * 0.25f);
        if (Plugins.ConfigEntries.WeightCounterBoolean.Value)
            WeightController.RecolorWeightText();
        if (ModCompats.IsGoodItemScanPresent && Plugins.ConfigEntries.ScanNodeFade.Value)
            ScanNodeController.UpdateGoodItemScanNodes();
        if (ModCompats.IsBetterScanVisionPresent)
            BetterScanVisionProxy.OverrideNightVisionColor();
    }
}