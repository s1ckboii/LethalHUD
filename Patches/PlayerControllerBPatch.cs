using GameNetcodeStuff;
using LethalHUD.HUD;
using MonoDetour;
using MonoDetour.HookGen;
using Unity.Netcode;
using UnityEngine;

namespace LethalHUD.Patches;
[MonoDetourTargets(typeof(PlayerControllerB), Members = ["SwitchToItemSlot", "DiscardHeldObject" ,"GrabObject", "DamagePlayer", "LateUpdate"])]
internal static class PlayerControllerBPatch
{
    [MonoDetourHookInitialize]
    public static void Init()
    {
        // Prefix
        On.GameNetcodeStuff.PlayerControllerB.GrabObject.Prefix(OnPlayerControllerBGrabObject);

        // Postfix
        On.GameNetcodeStuff.PlayerControllerB.SwitchToItemSlot.Postfix(OnPlayerControllerBSwitchToItemSlot);
        On.GameNetcodeStuff.PlayerControllerB.DiscardHeldObject.Postfix(OnPlayerControllerBDiscardHeldObject);
        On.GameNetcodeStuff.PlayerControllerB.LateUpdate.Postfix(OnPlayerLateUpdate);
        On.GameNetcodeStuff.PlayerControllerB.DamagePlayer.Postfix(OnPlayerControllerBDamagePlayer);
    }

    private static void OnPlayerControllerBSwitchToItemSlot(PlayerControllerB self, ref int slot, ref GrabbableObject fillSlotWithItem)
    {
        if (!Plugins.ConfigEntries.ShowItemValue.Value && ScrapValueDisplay.slotTexts != null)
            ScrapValueDisplay.Hide(slot);
        if (ScrapValueDisplay.slotTexts == null || slot < 0 || slot >= ScrapValueDisplay.slotTexts.Length)
            return;

        if (self.ItemSlots[slot] != null)
        {
            int scrapValue = self.ItemSlots[slot].scrapValue;
            ScrapValueDisplay.UpdateSlot(slot, scrapValue);
        }
        else
        {
            ScrapValueDisplay.UpdateSlot(slot, 0);
        }
    }

    private static void OnPlayerControllerBDiscardHeldObject(PlayerControllerB self, ref bool placeObject, ref NetworkObject parentObjectTo, ref Vector3 placePosition, ref bool matchRotationOfParent)
    {
        ScrapValueDisplay.UpdateSlot(self.currentItemSlot, 0);
    }

    private static void OnPlayerControllerBGrabObject(PlayerControllerB self)
    {
        InventoryFrames.HandsFull();
    }
    private static void OnPlayerControllerBDamagePlayer(PlayerControllerB self, ref int damageNumber, ref bool hasDamageSFX, ref bool callRPC, ref CauseOfDeath causeOfDeath, ref int deathAnimation, ref bool fallDamage, ref Vector3 force)
    {
        PlayerHPDisplay.ShakeOnHit();
    }

    private static void OnPlayerLateUpdate(PlayerControllerB self)
    {
        if (self.isTypingChat)
            ChatController.PlayerTypingIndicator();
        if (Plugins.ConfigEntries.SprintMeterBoolean.Value)
            SprintMeter.UpdateSprintMeterColor();
    }
}