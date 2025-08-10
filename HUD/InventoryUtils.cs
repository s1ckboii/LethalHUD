using UnityEngine;

namespace LethalHUD.HUD;
public class InventoryUtils : MonoBehaviour
{
    public static Color solarFlare = new Color(1f, 0.8f, 0.3f);
    public static Color moltenCore = new Color(1f, 0.2f, 0f);
    public static Color skyWave = new Color(0.4f, 0.65f, 1f);
    public static Color moonlitMist = new Color(0.8f, 0.8f, 1f);
    public static Color pinkPrism = new Color(1f, 0.4f, 1f);
    public static Color aquaPulse = new Color(0.4f, 1f, 1f);
    public static Color mintWave = new Color(0.5647f, 0.8706f, 0.7843f);
    public static Color deepTeal = new Color(0.1608f, 0.2863f, 0.2549f);
    public static Color neonLime = new Color(0.443f, 0.780f, 0f);
    public static Color lemonGlow = new Color(0.941f, 1f, 0.471f);
    public static Color crimsonSpark = new Color(1f, 0.27f, 0f);
    public static Color deepOcean = new Color(0f, 0.48f, 0.54f);

    private void LateUpdate()
    {
        InventoryFrames.SetSlotColors();
        ChatController.ColorChatInputField(HUDManager.Instance.chatTextField, Time.time * 0.25f);
    }
}