using GameNetcodeStuff;
using HarmonyLib;
using LethalHUD.CustomHUD;
using LethalHUD.HUD;
using LethalHUD.Misc;
using LethalHUD.Networking;
using System;
using UnityEngine;

namespace LethalHUD.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
internal static class PlayerControllerBPatch
{
    private static int _lastHealth = int.MinValue;

    private static bool IsLocalPlayer(PlayerControllerB player)
    {
        GameNetworkManager gameNetworkManager = GameNetworkManager.Instance;

        return player != null && gameNetworkManager != null && player == gameNetworkManager.localPlayerController;
    }

    [HarmonyPrefix]
    [HarmonyPatch("Awake")]
    private static void OnPlayerControllerBAwake_Prefix(PlayerControllerB __instance)
    {
        if (!Plugins.NetworkingDisabled && !__instance.TryGetComponent(out PlayerColorNetworker _))
            __instance.gameObject.AddComponent<PlayerColorNetworker>();

        if (!__instance.TryGetComponent(out PlayerBillboardGradient _))
            __instance.gameObject.AddComponent<PlayerBillboardGradient>();
    }

    [HarmonyPrefix]
    [HarmonyPatch("BeginGrabObject")]
    private static void OnPlayerControllerBBeginGrabObject(PlayerControllerB __instance)
    {
        if (!IsLocalPlayer(__instance))
            return;

        InventoryFrames.HandsFull();
    }

    [HarmonyPrefix]
    [HarmonyPatch("NoPunctuation")]
    private static bool NoPunctuation_Prefix(string input, ref string __result)
    {
        __result = ChatController.NoPunctuation(input);
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    private static void OnPlayerControllerBAwake_Postfix(PlayerControllerB __instance)
    {
        CustomStaminaMeter.Init(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("SwitchToItemSlot")]
    private static void OnPlayerControllerBSwitchToItemSlot(PlayerControllerB __instance, int slot, GrabbableObject fillSlotWithItem, bool __runOriginal)
    {
        if (!IsLocalPlayer(__instance))
            return;
            
        if (__runOriginal && __instance.IsOwner)
        {
            var frames = HUDManager.Instance?.itemSlotIconFrames;
            if (frames != null)
            {
                for (int i = 0; i < frames.Length; i++)
                {
                    Animator animator = frames[i] != null ? frames[i].GetComponent<Animator>() : null;
                    if (animator == null) continue;

                    CustomFrames.ForwardSlotBool(animator, "selectedSlot", i == __instance.currentItemSlot);
                    if (fillSlotWithItem != null && i == slot)
                    {
                        CustomFrames.ForwardSlotTrigger(animator, "GetItem", true);
                        CustomFrames.ForwardSlotTrigger(animator, "GetItem", false);
                    }
                }
            }
        }

        if (!Plugins.ConfigEntries.ShowItemValue.Value && ScrapValueDisplay.slotTexts != null)
            ScrapValueDisplay.Hide(slot);

        if (ScrapValueDisplay.slotTexts == null || slot < 0 || slot >= ScrapValueDisplay.slotTexts.Length)
            return;

        if (__instance.ItemSlots != null && slot < __instance.ItemSlots.Length && __instance.ItemSlots[slot] != null)
        {
            int scrapValue =__instance.ItemSlots[slot].scrapValue;

            ScrapValueDisplay.UpdateSlot(slot, scrapValue);
        }
        else
        {
            ScrapValueDisplay.UpdateSlot(slot, 0);
        }

        if (__instance.twoHanded)
        {
            HUDManager.Instance.PingHUDElement(
                HUDManager.Instance.Inventory,
                Plugins.ConfigEntries.SlotFadeDelayTime.Value / 2f,
                Math.Clamp(Plugins.ConfigEntries.SlotFade.Value + 0.25f, 0f, 1f),
                Plugins.ConfigEntries.SlotFade.Value
            );
        }
        else
        {
            HUDManager.Instance.PingHUDElement(
                HUDManager.Instance.Inventory,
                Plugins.ConfigEntries.SlotFadeDelayTime.Value,
                1f,
                Plugins.ConfigEntries.SlotFade.Value
            );
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("DespawnHeldObject")]
    private static void OnPlayerControllerBDespawnHeldObject(PlayerControllerB __instance)
    {
        if (!IsLocalPlayer(__instance))
            return;

        ScrapValueDisplay.UpdateSlot(__instance.currentItemSlot, 0);
    }

    [HarmonyPostfix]
    [HarmonyPatch("DiscardHeldObject")]
    private static void OnPlayerControllerBDiscardHeldObject(PlayerControllerB __instance)
    {
        if (!IsLocalPlayer(__instance))
            return;

        ScrapValueDisplay.UpdateSlot(__instance.currentItemSlot, 0);
    }

    [HarmonyPostfix]
    [HarmonyPatch("DestroyItemInSlot")]
    private static void OnPlayerControllerBDestroyItemInSlot(PlayerControllerB __instance)
    {
        if (!IsLocalPlayer(__instance))
            return;

        ScrapValueDisplay.SyncFromLocalInventory(true);
    }

    [HarmonyPostfix]
    [HarmonyPatch("DropAllHeldItems")]
    private static void OnPlayerControllerBDiscardAllHelditems(PlayerControllerB __instance)
    {
        if (!IsLocalPlayer(__instance))
            return;

        ScrapValueDisplay.SyncFromLocalInventory(true);
    }

    [HarmonyPostfix]
    [HarmonyPatch("DamagePlayer")]
    private static void OnPlayerControllerBDamagePlayer(PlayerControllerB __instance)
    {
        PlayerHPDisplay.ShakeOnHit(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnPlayerAnimation")]
    private static void OnPlayerControllerBSpawnPlayerAnimation(PlayerControllerB __instance)
    {
        if (!IsLocalPlayer(__instance))
            return;

        ScrapValueDisplay.ClearItemSlots();
    }

    [HarmonyPostfix]
    [HarmonyPatch("LateUpdate")]
    private static void OnPlayerLateUpdate(PlayerControllerB __instance)
    {
        if (__instance.isTypingChat)
            ChatController.PlayerTypingIndicator();

        SprintMeterController.UpdateSprintMeterColor();
        CustomStaminaMeter.UpdateFromPlayer(__instance);
        BatteryController.Update(__instance);

        int health = __instance.health;

        if (health == _lastHealth)
            return;

        _lastHealth = health;
        PlayerRedCanvasController.ChangeSetting();
    }
}