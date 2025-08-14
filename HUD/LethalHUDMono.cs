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
    }
}