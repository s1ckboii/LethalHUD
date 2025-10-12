using GameNetcodeStuff;
using LethalHUD.HUD;
using HarmonyLib;
using System;

namespace LethalHUD.Patches;
[HarmonyPatch(typeof(PlayerControllerB))]
internal static class PlayerControllerBPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("BeginGrabObject")]
    private static void OnPlayerControllerBBeginGrabObject()
    {
        InventoryFrames.HandsFull();
    }


    [HarmonyPrefix]
    [HarmonyPatch("NoPunctuation")]
    private static bool OnPlayerControllerBNoPunctuation(string input, ref string __result)
    {
        if (string.IsNullOrEmpty(input))
        {
            __result = "Nameless";
        }
        else
        {
            __result = ChatController.NoPunctuation(input);
        }

        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("SwitchToItemSlot")]
    private static void OnPlayerControllerBSwitchToItemSlot(PlayerControllerB __instance, int slot)
    {
        if (__instance != GameNetworkManager.Instance.localPlayerController)
            return;

        if (!Plugins.ConfigEntries.ShowItemValue.Value && ScrapValueDisplay.slotTexts != null)
            ScrapValueDisplay.Hide(slot);
        if (ScrapValueDisplay.slotTexts == null || slot < 0 || slot >= ScrapValueDisplay.slotTexts.Length)
            return;

        if (__instance.ItemSlots[slot] != null)
        {
            int scrapValue = __instance.ItemSlots[slot].scrapValue;
            ScrapValueDisplay.UpdateSlot(slot, scrapValue);
        }
        else
        {
            ScrapValueDisplay.UpdateSlot(slot, 0);
        }
        if (__instance.twoHanded)
        {
            HUDManager.Instance.PingHUDElement(HUDManager.Instance.Inventory, Plugins.ConfigEntries.SlotFadeDelayTime.Value / 2f, Math.Clamp(Plugins.ConfigEntries.SlotFade.Value + 0.25f, 0f, 1f), Plugins.ConfigEntries.SlotFade.Value);
        }
        else
        {
            HUDManager.Instance.PingHUDElement(HUDManager.Instance.Inventory, Plugins.ConfigEntries.SlotFadeDelayTime.Value, 1f, Plugins.ConfigEntries.SlotFade.Value);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("DespawnHeldObject")]
    private static void OnPlayerControllerBDespawnHeldObject(PlayerControllerB __instance)
    {
        ScrapValueDisplay.UpdateSlot(__instance.currentItemSlot, 0);
    }

    [HarmonyPostfix]
    [HarmonyPatch("DiscardHeldObject")]
    private static void OnPlayerControllerBDiscardHeldObject(PlayerControllerB __instance)
    {
        ScrapValueDisplay.UpdateSlot(__instance.currentItemSlot, 0);
    }

    [HarmonyPostfix]
    [HarmonyPatch("DestroyItemInSlot")]
    private static void OnPlayerControllerBDestroyItemInSlot(PlayerControllerB __instance)
    {
        ScrapValueDisplay.UpdateSlot(__instance.currentItemSlot, 0);
    }

    [HarmonyPostfix]
    [HarmonyPatch("DropAllHeldItems")]
    private static void OnPlayerControllerBDiscardAllHelditems(PlayerControllerB __instance)
    {
        ScrapValueDisplay.UpdateSlot(__instance.currentItemSlot, 0);
    }

    [HarmonyPostfix]
    [HarmonyPatch("DamagePlayer")]
    private static void OnPlayerControllerBDamagePlayer()
    {
        PlayerHPDisplay.ShakeOnHit();
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnPlayerAnimation")]
    private static void OnPlayerControllerBSpawnPlayerAnimation()
    {
        ScrapValueDisplay.ClearItemSlots();
    }

    [HarmonyPostfix]
    [HarmonyPatch("LateUpdate")]
    private static void OnPlayerLateUpdate(PlayerControllerB __instance)
    {
        if (__instance.isTypingChat)
            ChatController.PlayerTypingIndicator();
        SprintMeterController.UpdateSprintMeterColor();
    }
}