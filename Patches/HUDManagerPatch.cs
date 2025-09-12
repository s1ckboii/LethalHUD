using GameNetcodeStuff;
using LethalHUD.Compats;
using LethalHUD.HUD;
using LethalHUD.Misc;
using LethalHUD.Scan;
using MonoDetour;
using MonoDetour.HookGen;
using System.Collections;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace LethalHUD.Patches;
[MonoDetourTargets(typeof(HUDManager), Members = ["PingScan_performed", "Start", "OnEnable", "DisableAllScanElements", "DisplayNewScrapFound", "Update", "UpdateScanNodes", "AddChatMessage"])]
internal static class HUDManagerPatch
{
    private static CallbackContext pingScan;
    private static int lastSlotCount = 0;

[MonoDetourHookInitialize]
    public static void Init()
    {
        // Prefix
        On.HUDManager.PingScan_performed.Prefix(OnScanTriggered);
        On.HUDManager.DisableAllScanElements.Prefix(OnHUDManagerDisabledAllScanElements);

        // Postfix
        On.HUDManager.Start.Postfix(OnHUDManagerStart);
        On.HUDManager.OnEnable.Postfix(OnHUDManagerEnable);
        On.HUDManager.DisplayNewScrapFound.Postfix(OnHUDManagerDisplayNewScrapFound);
        On.HUDManager.Update.Postfix(OnHUDManagerUpdate);
        On.HUDManager.UpdateScanNodes.Postfix(OnHUDManagerUpdateScanNodes);
        On.HUDManager.AddChatMessage.Postfix(OnHUDManagerAddChatMessage);
    }

    private static void OnScanTriggered(HUDManager self,ref CallbackContext context)
    {
        pingScan = context;
        if (ModCompats.IsGoodItemScanPresent)
            ScanNodeController.ResetGoodItemScanNodes();
        //LootInfoManager.LootScan();
    }

    private static void OnHUDManagerStart(HUDManager self)
    {
        ScanController.SetScanColor();
        ScanController.UpdateScanTexture();
        PlayerHPDisplay.Init();
        ScrapValueDisplay.Init();
        if (ModCompats.IsBetterScanVisionPresent)
            BetterScanVisionProxy.OverrideNightVisionColor();

        self.StartCoroutine(ScanTextureRoutine());
    }

    private static void OnHUDManagerEnable(HUDManager self)
    {
        if (self.gameObject.GetComponent<LethalHUDMono>() == null)
        {
            self.gameObject.AddComponent<LethalHUDMono>();
        }
        if (self.gameObject.GetComponent<StatsDisplay>() == null)
        {
            self.gameObject.AddComponent<StatsDisplay>();
        }
        /*
        if (self.gameObject.GetComponent<ChatNetworkManager>() == null)
        {
            self.gameObject.AddComponent<ChatNetworkManager>();

            ChatController.ApplyLocalPlayerColor(Plugins.ConfigEntries.LocalNameColor.Value, Plugins.ConfigEntries.GradientNameColorB.Value);
        }
        */
        ChatController.ColorChatInputField(HUDManager.Instance.chatTextField, Time.time * 0.25f);
    }

    private static void OnHUDManagerDisplayNewScrapFound(HUDManager self)
    {
        InventoryFrames.SetSlotColors();
    }

    private static void OnHUDManagerDisabledAllScanElements(HUDManager self)
    {
        if (ModCompats.IsGoodItemScanPresent)
            ScanNodeController.ResetGoodItemScanNodes();
    }

    private static void OnHUDManagerUpdate(HUDManager self)
    {
        ScanController.UpdateScanAlpha();
        PlayerHPDisplay.UpdateNumber();
        if (Plugins.ConfigEntries.HoldScan.Value && IngamePlayerSettings.Instance.playerInput.actions.FindAction("PingScan").IsPressed())
            self.PingScan_performed(pingScan);
        if (Plugins.ConfigEntries.WeightCounterBoolean.Value)
            WeightController.UpdateWeightDisplay();
        int currentCount = self.itemSlotIconFrames.Length;
        if (currentCount != lastSlotCount)
        {
            ScrapValueDisplay.RefreshSlots();
            lastSlotCount = currentCount;
        }
    }

    private static void OnHUDManagerUpdateScanNodes(HUDManager self, ref PlayerControllerB playerScript)
    {
        if (Plugins.ConfigEntries.ScanNodeFade.Value)
            ScanNodeController.UpdateTimers(self.scanElements, self.scanNodes);
    }

    private static void OnHUDManagerAddChatMessage(HUDManager self, ref string chatMessage, ref string nameOfUserWhoTyped, ref int playerWhoSent, ref bool dontRepeat)
    {
        if (self.ChatMessageHistory.Count == 0)
            return;

        string last;

        if (!string.IsNullOrEmpty(nameOfUserWhoTyped))
        {
            string coloredName = ChatController.GetColoredPlayerName(nameOfUserWhoTyped, playerWhoSent);
            string coloredMessage = ChatController.GetColoredChatMessage(chatMessage);
            last = $"{coloredName}: {coloredMessage}";
        }
        else
        {
            last = ChatController.GetColoredChatMessage(chatMessage);
        }

        self.ChatMessageHistory[^1] = last;
        self.chatText.text = string.Join("\n", self.ChatMessageHistory);
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