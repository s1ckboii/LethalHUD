using GameNetcodeStuff;
using HarmonyLib;
using LethalHUD.Compats;
using LethalHUD.CustomHUD;
using LethalHUD.HUD;
using LethalHUD.Misc;
using LethalHUD.Scan;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using static LethalHUD.Enums;
using static UnityEngine.InputSystem.InputAction;

namespace LethalHUD.Patches;
[HarmonyPatch(typeof(HUDManager))]
internal static class HUDManagerPatch
{
    private static InputAction _pingScanAction;
    private static CallbackContext _pingScan;
    private static bool _isScanToggled = false;
    private static Coroutine _toggleCoroutine;

    private static int lastSlotCount = 0;

    #region Prefixes

    [HarmonyPrefix]
    [HarmonyPatch("PingScan_performed")]
    private static void OnScanTriggered_Prefix(ref CallbackContext context)
    {
        _pingScan = context;

        if (ModCompats.IsGoodItemScanPresent)
            ScanNodeController.ResetGoodItemScanNodes();
       LootInfoManager.LootScan();
    }

    [HarmonyPrefix]
    [HarmonyPatch("UseSignalTranslatorClientRpc")]
    private static bool OnHUDManagerUseSignalTranslatorClientRpc_Prefix(HUDManager __instance, string signalMessage, int timesSendingMessage)
    {
        SignalTranslator signalTranslator = Object.FindObjectOfType<SignalTranslator>();
        if (string.IsNullOrEmpty(signalMessage) || signalTranslator == null) return true;

        signalTranslator.timeLastUsingSignalTranslator = Time.realtimeSinceStartup;

        if (signalTranslator.signalTranslatorCoroutine != null)
        {
            __instance.StopCoroutine(signalTranslator.signalTranslatorCoroutine);
        }

        signalTranslator.timesSendingMessage = timesSendingMessage;

        signalTranslator.signalTranslatorCoroutine = __instance.StartCoroutine(
            SignalTranslatorController.DisplaySignalTranslatorMessage(signalMessage, timesSendingMessage, signalTranslator)
        );

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("DisableAllScanElements")]
    private static void OnHUDManagerDisabledAllScanElements_Prefix()
    {
        if (ModCompats.IsGoodItemScanPresent)
            ScanNodeController.ResetGoodItemScanNodes();
    }

    [HarmonyPrefix]
    [HarmonyPatch("SetClockVisible")]
    private static void OnHUDManagerSetClockVisible_Prefix(ref bool visible)
    {
        ClockController.UpdateClockVisibility(ref visible);
        ClockController.ApplyClockAlpha();
    }

    [HarmonyPrefix]
    [HarmonyPatch("UpdateHealthUI")]
    private static bool OnHUDManagerUpdateHealthUI_Prefix()
    {
        return !CustomHealthBar.UsingCustom;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HUDManager), "DisplaySignalTranslatorMessage")]
    private static bool OnHUDManagerDisplaySignalTranslatorMessage_Prefix(string signalMessage, int seed, SignalTranslator signalTranslator, ref IEnumerator __result)
    {
        if (signalTranslator != null && !string.IsNullOrEmpty(signalMessage))
        {
            __result = SignalTranslatorController.DisplaySignalTranslatorMessage(signalMessage, seed, signalTranslator);
            return false;
        }

        return true;
    }
    #endregion

    #region Postfixes

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    private static void OnHUDManagerStart_Postfix(HUDManager __instance)
    {
        lastSlotCount = 0;
        ScrapValueDisplay.ResetForNewHUD();

        _pingScanAction = IngamePlayerSettings.Instance.playerInput.actions.FindAction("PingScan");

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
    private static void OnHUDManagerEnable_Postfix(HUDManager __instance)
    {
        if (__instance.gameObject.GetComponent<LethalHUDMono>() == null)
        {
            __instance.gameObject.AddComponent<LethalHUDMono>();
        }
        if (__instance.gameObject.GetComponent<StatsDisplay>() == null)
        {
            __instance.gameObject.AddComponent<StatsDisplay>();
        }

        CanvasGroup selfRed = __instance.selfRedCanvasGroup;
        if (selfRed == null)
            return;

        PlayerRedCanvasController.Bind(selfRed);

        CustomHealthBar.OnHUDEnable(__instance);
        CustomFrames.OnHUDEnable(__instance);

        ChatController.ColorChatInputField(HUDManager.Instance.chatTextField, Time.time * 0.25f);

        __instance.StartCoroutine(ApplySelfRedAfterTick(__instance));
    }

    [HarmonyPostfix]
    [HarmonyPatch("DisplayNewScrapFound")]
    private static void OnHUDManagerDisplayNewScrapFound_Postfix()
    {
        InventoryFrames.SetSlotColors();
    }

    [HarmonyPostfix]
    [HarmonyPatch("SetClock")]
    private static void OnHUDManagerSetClock_Postfix(HUDManager __instance, float timeNormalized, float numberOfHours)
    {
        ClockController.TryOverrideClock(__instance, timeNormalized, numberOfHours);
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    private static void OnHUDManagerUpdate_Postfix(HUDManager __instance)
    {
        ScanController.UpdateScanAlpha();
        PlayerHPDisplay.UpdateNumber();
        WeightController.UpdateWeightDisplay();

        ScanMode scanMode = Plugins.ConfigEntries.ScanModeType.Value;

        switch (scanMode)
        {
            case ScanMode.Default:
                StopToggleScan(__instance);
                break;

            case ScanMode.Hold:
                StopToggleScan(__instance);
                if (_pingScanAction.IsPressed())
                    __instance.PingScan_performed(_pingScan);
                break;

            case ScanMode.Toggle:
                if (_pingScanAction.WasPressedThisFrame())
                {
                    if (_isScanToggled)
                        StopToggleScan(__instance);
                    else
                        StartToggleScan(__instance);
                }
                break;
        }

        if (CustomHealthBar.UsingCustom && __instance.localPlayer != null)
        {
            CustomHealthBar.UpdateFromPlayer(__instance.localPlayer);
        }

        if (__instance.itemSlotIconFrames != null)
        {
            int currentCount = __instance.itemSlotIconFrames.Length;
            if (currentCount != lastSlotCount && lastSlotCount != 0)
            {
                lastSlotCount = currentCount;
                CustomFrames.Apply(Plugins.ConfigEntries.CustomInventoryFrames.Value);
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("UpdateHealthUI")]
    private static void OnHUDManagerUpdateHealthUI_Postfix(int health)
    {
        if (CustomHealthBar.UsingCustom) return;

        switch (Plugins.ConfigEntries.SelfRedCanvasMode.Value)
        {
            case SelfRedMode.Vanilla:
                return;

            case SelfRedMode.ColoredFilled:
                PlayerRedCanvasController.ApplyFillAndColor(health);
                break;

            case SelfRedMode.RedFillUp:
                PlayerRedCanvasController.ApplyFillWithRedFade(health);
                break;

        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("UpdateHealthUI")]
    private static void OnHUDManagerUpdateHealthUI(int health)
    {
        switch (Plugins.ConfigEntries.SelfRedCanvasMode.Value)
        {
            case SelfRedMode.Vanilla:
                return;

            case SelfRedMode.ColoredFilled:
                PlayerRedCanvasController.ApplyFillAndColor(health);
                break;

            case SelfRedMode.RedFillUp:
                PlayerRedCanvasController.ApplyFillWithRedFade(health);
                break;

        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("UpdateScanNodes")]
    private static void OnHUDManagerUpdateScanNodes_Postfix(HUDManager __instance)
    {
        if (Plugins.ConfigEntries.ScanNodeFade.Value)
            ScanNodeController.UpdateTimers(__instance.scanElements, __instance.scanNodes);
    }

    [HarmonyPostfix]
    [HarmonyPatch("AddChatMessage")]
    private static void OnHUDManagerAddChatMessage_Postfix(HUDManager __instance, string chatMessage, string nameOfUserWhoTyped, int playerWhoSent)
    {
        if (string.IsNullOrEmpty(chatMessage))
            return;

        int index = __instance.ChatMessageHistory.Count - 1;
        if (index < 0) return;

        string original = __instance.ChatMessageHistory[index];

        Match tagMatch = Regex.Match(chatMessage, @"^(<[^>]+>)+");
        string preTags = "";
        string innerText = chatMessage;

        if (tagMatch.Success)
        {
            preTags = tagMatch.Value;
            innerText = chatMessage[tagMatch.Length..];
        }

        bool alreadyColored = chatMessage.Contains("<color=") || chatMessage.Contains("<gradient=");

        string coloredMessage = alreadyColored
            ? chatMessage
            : preTags + ChatController.GetColoredChatMessage(innerText);

        string final;
        if (!string.IsNullOrEmpty(nameOfUserWhoTyped))
        {
            string coloredName = ChatController.GetColoredPlayerName(nameOfUserWhoTyped, playerWhoSent);
            final = $"{coloredName}: {coloredMessage}";
        }
        else
        {
            final = coloredMessage;
        }

        __instance.ChatMessageHistory[index] = final;

        StringBuilder sb = new();
        for (int i = 0; i < __instance.ChatMessageHistory.Count; i++)
        {
            sb.Append('\n');
            sb.Append(__instance.ChatMessageHistory[i]);
        }

        __instance.chatText.text = sb.ToString();

        __instance.PingHUDElement(__instance.Chat, Plugins.ConfigEntries.ChatFadeDelayTime.Value, 1f, 0f);
    }

    [HarmonyPostfix]
    [HarmonyPatch("OpenMenu_performed")]
    private static void OnHUDManagerOpenMenu_performed_Postfix(HUDManager __instance)
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
    private static void AfterChangeControlTipMultiple_Postfix()
    {
        ControlTipController.ApplyColor();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ClearControlTips")]
    private static void AfterClearControlTips_Postfix()
    {
        ControlTipController.ApplyColor();
    }
    #endregion

    #region IEnumerators, Utils

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

            LootInfoManager.LootScan();
        }
    }
    private static IEnumerator ApplySelfRedAfterTick(HUDManager hud)
    {
        yield return null;

        if (hud == null || hud.selfRedCanvasGroup == null)
            yield break;

        PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
        if (player == null)
            yield break;

        int health = player.health;

        switch (Plugins.ConfigEntries.SelfRedCanvasMode.Value)
        {
            case SelfRedMode.Vanilla:
                yield break;

            case SelfRedMode.ColoredFilled:
                PlayerRedCanvasController.ApplyFillAndColor(health);
                break;

            case SelfRedMode.RedFillUp:
                PlayerRedCanvasController.ApplyFillWithRedFade(health);
                break;
        }
    }
    #endregion
}