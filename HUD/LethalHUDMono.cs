using UnityEngine;

namespace LethalHUD.HUD
{
    public class LethalHUDMono : MonoBehaviour
    {
        private void LateUpdate()
        {
            InventoryFrames.SetSlotColors();
            ChatController.ColorChatInputField(HUDManager.Instance.chatTextField, Time.time * 0.25f);
        }
    }
}
