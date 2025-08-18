using GameNetcodeStuff;
using LethalHUD.HUD;
using MonoDetour;
using MonoDetour.HookGen;
using UnityEngine;

namespace LethalHUD.Patches;
[MonoDetourTargets(typeof(PlayerControllerB), Members = ["GrabObject", "DamagePlayer", "LateUpdate"])]
internal static class PlayerControllerBPatch
{
    [MonoDetourHookInitialize]
    public static void Init()
    {
        // Prefix
        On.GameNetcodeStuff.PlayerControllerB.GrabObject.Prefix(OnPlayerControllerBBeginGrabObject);

        // Postfix
        On.GameNetcodeStuff.PlayerControllerB.LateUpdate.Postfix(OnPlayerLateUpdate);
        On.GameNetcodeStuff.PlayerControllerB.DamagePlayer.Postfix(OnPlayerControllerBDamagePlayer);
    }

    private static void OnPlayerControllerBDamagePlayer(PlayerControllerB self, ref int damageNumber, ref bool hasDamageSFX, ref bool callRPC, ref CauseOfDeath causeOfDeath, ref int deathAnimation, ref bool fallDamage, ref Vector3 force)
    {
        PlayerHPDisplay.ShakeOnHit();
    }

    private static void OnPlayerControllerBBeginGrabObject(PlayerControllerB self)
    {
        InventoryFrames.HandsFull();
    }
    private static void OnPlayerLateUpdate(PlayerControllerB self)
    {
        if (self.isTypingChat)
            ChatController.PlayerTypingIndicator();
        if (Plugins.ConfigEntries.SprintMeterBoolean.Value)
            SprintMeter.UpdateSprintMeterColor();
    }
}