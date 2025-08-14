using GameNetcodeStuff;
using LethalHUD.HUD;
using MonoDetour;
using MonoDetour.HookGen;
using System;
using UnityEngine;

namespace LethalHUD.Patches;
[MonoDetourTargets(typeof(PlayerControllerB), Members = ["GrabObject", "LateUpdate"])]
internal static class PlayerControllerBPatch
{
    [MonoDetourHookInitialize]
    public static void Init()
    {
        // Prefix
        On.GameNetcodeStuff.PlayerControllerB.GrabObject.Prefix(OnPlayerControllerBBeginGrabObject);

        // Postfix
        On.GameNetcodeStuff.PlayerControllerB.LateUpdate.Postfix(OnPlayerIsTyping);
        //On.GameNetcodeStuff.PlayerControllerB.DamagePlayer.Postfix(OnPlayerControllerBDamagePlayer);
    }

    private static void OnPlayerControllerBBeginGrabObject(PlayerControllerB self)
    {
        InventoryFrames.HandsFull();
    }
    private static void OnPlayerIsTyping(PlayerControllerB self)
    {
        if (self.isTypingChat)
            ChatController.PlayerTypingIndicator();
        if (Plugins.ConfigEntries.SprintMeterBoolean.Value)
            SprintMeter.UpdateSprintMeterColor();
    }
}