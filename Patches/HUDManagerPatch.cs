using GameNetcodeStuff;
using HarmonyLib;
using LethalHUD.Compats;
using LethalHUD.Events;
using LethalHUD.HUD;
using LethalHUD.Misc;
using LethalHUD.Scan;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using static LethalHUD.Enums;
using static UnityEngine.InputSystem.InputAction;

namespace LethalHUD.Patches;
[HarmonyPatch(typeof(HUDManager))]
internal static class HUDManagerPatch
{
    private static CallbackContext _pingScan;
    private static bool _isScanToggled = false;
    private static Coroutine _toggleCoroutine;

    private static int lastSlotCount = 0;

    [HarmonyPrefix]
    [HarmonyPatch("PingScan_performed")]
    private static void OnScanTriggered(ref CallbackContext context)
    {
        _pingScan = context;

        if (ModCompats.IsGoodItemScanPresent)
            ScanNodeController.ResetGoodItemScanNodes();
        //LootInfoManager.LootScan();
    }

    [HarmonyPrefix]
    [HarmonyPatch("UseSignalTranslatorClientRpc")]
    private static void OnHUDManagerUseSignalTranslatorClientRpc(ref string signalMessage)
    {
        SignalTranslatorController.SetSignalText(signalMessage);
    }

    [HarmonyPrefix]
    [HarmonyPatch("DisableAllScanElements")]
    private static void OnHUDManagerDisabledAllScanElements()
    {
        if (ModCompats.IsGoodItemScanPresent)
            ScanNodeController.ResetGoodItemScanNodes();
    }

    [HarmonyPrefix]
    [HarmonyPatch("SetClockVisible")]
    private static void OnHUDManagerSetClockVisible(ref bool visible)
    {
        ClockController.UpdateClockVisibility(ref visible);
        ClockController.ApplyClockAlpha();
    }

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    private static void OnHUDManagerStart(HUDManager __instance)
    {
        ScanController.SetScanColor();
        ScanController.UpdateScanTexture();
        PlayerHPDisplay.Init();
        ScrapValueDisplay.Init();
        ClockController.ApplyClockAppearance();
        PlanetInfoDisplay.ApplyColors();
        SpectatorHUDController.ApplyColors();
        if (ModCompats.IsBetterScanVisionPresent)
            BetterScanVisionProxy.OverrideNightVisionColor();

        __instance.StartCoroutine(ScanTextureRoutine());
    }

    [HarmonyPostfix]
    [HarmonyPatch("OnEnable")]
    private static void OnHUDManagerEnable(HUDManager __instance)
    {
        if (__instance.gameObject.GetComponent<LethalHUDMono>() == null)
        {
            __instance.gameObject.AddComponent<LethalHUDMono>();
        }
        if (__instance.gameObject.GetComponent<StatsDisplay>() == null)
        {
            __instance.gameObject.AddComponent<StatsDisplay>();
        }
        if (__instance.gameObject.GetComponent<EventColorUpdater>() == null)
        {
            __instance.gameObject.AddComponent<EventColorUpdater>();
        }
        ChatController.ColorChatInputField(HUDManager.Instance.chatTextField, Time.time * 0.25f);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HUDManager), "DisplaySignalTranslatorMessage")]
    private static bool OnHUDManagerDisplaySignalTranslatorMessage(string signalMessage, int seed, SignalTranslator signalTranslator, ref IEnumerator __result)
    {
        if (signalTranslator != null && !string.IsNullOrEmpty(signalMessage))
        {
            __result = SignalTranslatorController.DisplaySignalTranslatorMessage(signalMessage, seed, signalTranslator);
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("DisplayNewScrapFound")]
    private static void OnHUDManagerDisplayNewScrapFound()
    {
        InventoryFrames.SetSlotColors();
    }

    [HarmonyPostfix]
    [HarmonyPatch("SetClock")]
    private static void OnHUDManagerSetClock(HUDManager __instance, float timeNormalized, float numberOfHours)
    {
        ClockController.TryOverrideClock(__instance, timeNormalized, numberOfHours);
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    private static void OnHUDManagerUpdate(HUDManager __instance)
    {
        ScanController.UpdateScanAlpha();
        PlayerHPDisplay.UpdateNumber();
        WeightController.UpdateWeightDisplay();

        InputAction scanAction = IngamePlayerSettings.Instance.playerInput.actions.FindAction("PingScan");
        ScanMode scanMode = Plugins.ConfigEntries.ScanModeType.Value;

        switch (scanMode)
        {
            case ScanMode.Default:
                StopToggleScan(__instance);
                break;

            case ScanMode.Hold:
                StopToggleScan(__instance);
                if (scanAction.IsPressed())
                    __instance.PingScan_performed(_pingScan);
                break;

            case ScanMode.Toggle:
                if (scanAction.WasPressedThisFrame())
                {
                    if (_isScanToggled)
                        StopToggleScan(__instance);
                    else
                        StartToggleScan(__instance);
                }
                break;
        }

        int currentCount = __instance.itemSlotIconFrames.Length;
        if (currentCount != lastSlotCount)
        {
            ScrapValueDisplay.RefreshSlots();
            lastSlotCount = currentCount;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("UpdateScanNodes")]
    private static void OnHUDManagerUpdateScanNodes(HUDManager __instance)
    {
        if (Plugins.ConfigEntries.ScanNodeFade.Value)
            ScanNodeController.UpdateTimers(__instance.scanElements, __instance.scanNodes);
    }

    [HarmonyPostfix]
    [HarmonyPatch("AddChatMessage")]
    private static void OnHUDManagerAddChatMessage(HUDManager __instance, string chatMessage, string nameOfUserWhoTyped, int playerWhoSent)
    {
        if (string.IsNullOrEmpty(chatMessage))
            return;

        string last;

        Match tagMatch = Regex.Match(chatMessage, @"^(<[^>]+>)+");
        string preTags = "";
        string innerText = chatMessage;

        if (tagMatch.Success)
        {
            preTags = tagMatch.Value;
            innerText = chatMessage[tagMatch.Length..];
        }

        bool alreadyColored = chatMessage.Contains("<color=") || chatMessage.Contains("<gradient=");

        string coloredMessage;
        if (alreadyColored)
        {
            coloredMessage = chatMessage;
        }
        else
        {
            string recoloredText = ChatController.GetColoredChatMessage(innerText);
            coloredMessage = preTags + recoloredText;
        }

        if (!string.IsNullOrEmpty(nameOfUserWhoTyped))
        {
            string coloredName = ChatController.GetColoredPlayerName(nameOfUserWhoTyped, playerWhoSent);
            last = $"{coloredName}: {coloredMessage}";
        }
        else
        {
            last = coloredMessage;
        }

        if (__instance.ChatMessageHistory.Count > 0)
        {
            __instance.ChatMessageHistory[^1] = last;
            __instance.chatText.text = string.Join("\n", __instance.ChatMessageHistory);
        }

        __instance.PingHUDElement(__instance.Chat, Plugins.ConfigEntries.ChatFadeDelayTime.Value, 1f, 0f);
    }

    [HarmonyPostfix]
    [HarmonyPatch("OpenMenu_performed")]
    private static void OnHUDManagerOpenMenu_performed(HUDManager __instance)
    {
        __instance.PingHUDElement(__instance.Chat, Plugins.ConfigEntries.ChatFadeDelayTime.Value, 1f, 0f);
    }
    
    /*
    [HarmonyPostfix]
    [HarmonyPatch("SubmitChat_performed")]
    private static void OnHUDManagerSubmitChat_performed(HUDManager __instance)
    {
        __instance.PingHUDElement(__instance.Chat, Plugins.ConfigEntries.ChatFadeDelayTime.Value, 1f, 0f);
    }
    */

    [HarmonyPostfix]
    [HarmonyPatch("ChangeControlTipMultiple")]
    private static void AfterChangeControlTipMultiple()
    {
        ControlTipController.ApplyColor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ClearControlTips")]
    private static void AfterClearControlTips()
    {
        ControlTipController.ApplyColor();
    }

    private static IEnumerator ScanTextureRoutine()
    {
        while (true)
        {
            ScanNodeTextureManager.Tick();
            ScanNodeTextureManager.ClearDestroyedObjects();
            yield return new WaitForSeconds(1f);
        }
    }

    private static void StartToggleScan(HUDManager __instance)
    {
        if (_toggleCoroutine != null) return;
        _isScanToggled = true;

        _toggleCoroutine = __instance.StartCoroutine(ToggleScanRoutine(__instance));
    }

    private static void StopToggleScan(HUDManager __instance)
    {
        if (_toggleCoroutine == null) return;
        __instance.StopCoroutine(_toggleCoroutine);
        _toggleCoroutine = null;
        _isScanToggled = false;
    }

    private static IEnumerator ToggleScanRoutine(HUDManager __instance)
    {
        while (true)
        {
            TryPerformScan(__instance);

            yield return null;
        }
    }
    private static void TryPerformScan(HUDManager __instance)
    {
        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (player == null) return;

        float cooldown = __instance.playerPingingScan;
        if (cooldown <= -1f && __instance.CanPlayerScan())
        {
            __instance.playerPingingScan = 0.3f;
            __instance.scanEffectAnimator.transform.position = player.gameplayCamera.transform.position;
            __instance.scanEffectAnimator.SetTrigger("scan");
            __instance.PingHUDElement(__instance.Compass, 1f, 0.8f, 0.12f);
            __instance.UIAudio.PlayOneShot(__instance.scanSFX);
        }
    }
}