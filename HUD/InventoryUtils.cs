using UnityEngine;

namespace LethalHUD.HUD;
public class InventoryUtils : MonoBehaviour
{
    private void LateUpdate()
    {
        InventoryFrames.ApplySlotColors();
    }
}