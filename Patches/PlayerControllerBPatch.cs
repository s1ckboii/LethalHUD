using GameNetcodeStuff;
using LethalHUD.HUD;
using MonoDetour;
using MonoDetour.HookGen;
using Unity.Netcode;
using UnityEngine;

namespace LethalHUD.Patches;
[MonoDetourTargets(typeof(PlayerControllerB), Members = ["SwitchToItemSlot", "DiscardHeldObject", "NoPunctuation", "DestroyItemInSlot", "DropAllHeldItems" ,"BeginGrabObject", "DamagePlayer", "SpawnPlayerAnimation" , "LateUpdate"])]
internal static class PlayerControllerBPatch
{
    [MonoDetourHookInitialize]
    public static void Init()
    {
        // Prefix
        On.GameNetcodeStuff.PlayerControllerB.BeginGrabObject.Prefix(OnPlayerControllerBBeginGrabObject);

        // Postfix
        On.GameNetcodeStuff.PlayerControllerB.NoPunctuation.Postfix(OnPlayerControllerBNoPunctionation);
        On.GameNetcodeStuff.PlayerControllerB.SwitchToItemSlot.Postfix(OnPlayerControllerBSwitchToItemSlot);
        On.GameNetcodeStuff.PlayerControllerB.DiscardHeldObject.Postfix(OnPlayerControllerBDiscardHeldObject);
        On.GameNetcodeStuff.PlayerControllerB.LateUpdate.Postfix(OnPlayerLateUpdate);
        On.GameNetcodeStuff.PlayerControllerB.DamagePlayer.Postfix(OnPlayerControllerBDamagePlayer);
        On.GameNetcodeStuff.PlayerControllerB.SpawnPlayerAnimation.Postfix(OnPlayerControllerBSpawnPlayerAnimation);
        On.GameNetcodeStuff.PlayerControllerB.DropAllHeldItems.Postfix(OnPlayerControllerBDiscardAllHelditems);
        On.GameNetcodeStuff.PlayerControllerB.DestroyItemInSlot.Postfix(OnPlayerControllerBDestroyItemInSlotAndSync);
    }

    private static void OnPlayerControllerBNoPunctionation(PlayerControllerB self, ref string input, ref string returnValue)
    {
        returnValue = ChatController.NoPunctuation(input);

        if (string.IsNullOrEmpty(input))
            returnValue = "Nameless";
    }

    private static void OnPlayerControllerBSwitchToItemSlot(PlayerControllerB self, ref int slot, ref GrabbableObject fillSlotWithItem)
    {
        if (self != GameNetworkManager.Instance.localPlayerController)
            return;

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
    private static void OnPlayerControllerBDestroyItemInSlotAndSync(PlayerControllerB self, ref int itemSlot)
    {
        ScrapValueDisplay.UpdateSlot(self.currentItemSlot, 0);
    }
    private static void OnPlayerControllerBDiscardAllHelditems(PlayerControllerB self, ref bool itemsFall, ref bool disconnecting)
    {
        ScrapValueDisplay.UpdateSlot(self.currentItemSlot, 0);
    }

    private static void OnPlayerControllerBBeginGrabObject(PlayerControllerB self)
    {
        InventoryFrames.HandsFull();
    }

    private static void OnPlayerControllerBDamagePlayer(PlayerControllerB self, ref int damageNumber, ref bool hasDamageSFX, ref bool callRPC, ref CauseOfDeath causeOfDeath, ref int deathAnimation, ref bool fallDamage, ref Vector3 force)
    {
        PlayerHPDisplay.ShakeOnHit();
    }

    private static void OnPlayerControllerBSpawnPlayerAnimation(PlayerControllerB self)
    {
        ScrapValueDisplay.ClearItemSlots();
    }

    private static void OnPlayerLateUpdate(PlayerControllerB self)
    {
        if (self.isTypingChat)
            ChatController.PlayerTypingIndicator();
        if (Plugins.ConfigEntries.SprintMeterBoolean.Value)
            SprintMeter.UpdateSprintMeterColor();
    }
}