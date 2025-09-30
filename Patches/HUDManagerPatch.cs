using LethalHUD.Compats;
using LethalHUD.HUD;
using LethalHUD.Misc;
using LethalHUD.Scan;
using HarmonyLib;
using System.Collections;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace LethalHUD.Patches;
[HarmonyPatch(typeof(HUDManager))]
internal static class HUDManagerPatch
{
    private static CallbackContext pingScan;
    private static int lastSlotCount = 0;

    [HarmonyPrefix]
    [HarmonyPatch("PingScan_performed")]
    private static void OnScanTriggered(ref CallbackContext context)
    {
        pingScan = context;
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
        ChatController.ColorChatInputField(HUDManager.Instance.chatTextField, Time.time * 0.25f);
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HUDManager), "DisplaySignalTranslatorMessage")]
    private static bool DisplaySignalTranslatorMessage_Prefix(string signalMessage, int seed, SignalTranslator signalTranslator, ref IEnumerator __result)
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
        if (Plugins.ConfigEntries.HoldScan.Value && IngamePlayerSettings.Instance.playerInput.actions.FindAction("PingScan").IsPressed())
            __instance.PingScan_performed(pingScan);
        if (Plugins.ConfigEntries.WeightCounterBoolean.Value)
            WeightController.UpdateWeightDisplay();
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
    private static void OnHUDManagerAddChatMessage(HUDManager __instance, string chatMessage, string nameOfUserWhoTyped)
    {
        if (string.IsNullOrEmpty(chatMessage))
            return;

        string last;

        if (!string.IsNullOrEmpty(nameOfUserWhoTyped))
        {
            //string coloredName = ChatController.GetColoredPlayerName(nameOfUserWhoTyped, playerWhoSent);
            string coloredName = ChatController.GetColoredPlayerName(nameOfUserWhoTyped);
            string coloredMessage = ChatController.GetColoredChatMessage(chatMessage);
            last = $"{coloredName}: {coloredMessage}";
        }
        else
        {
            last = ChatController.GetColoredChatMessage(chatMessage);
        }

        if (__instance.ChatMessageHistory.Count > 0)
        {
            __instance.ChatMessageHistory[^1] = last;
            __instance.chatText.text = string.Join("\n", __instance.ChatMessageHistory);
        }
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
}