using LethalHUD.Configs;
using UnityEngine;

namespace LethalHUD.HUD;

public static class InventoryFrames
{
    public static void ApplySlotColors()
    {
        if (HUDManager.Instance == null || HUDManager.Instance.itemSlotIconFrames == null)
            return;

        Color color = ConfigHelper.GetSlotColor();

        foreach (var frame in HUDManager.Instance.itemSlotIconFrames)
        {
            if (frame != null)
            {
                frame.color = color;
            }
        }
    }
}