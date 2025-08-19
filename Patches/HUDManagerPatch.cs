using GameNetcodeStuff;
using LethalHUD.Compats;
using LethalHUD.HUD;
using LethalHUD.Misc;
using LethalHUD.Scan;
using MonoDetour;
using MonoDetour.Cil;
using MonoDetour.HookGen;
using MonoMod.Cil;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

namespace LethalHUD.Patches;
[MonoDetourTargets(typeof(HUDManager), Members = ["PingScan_performed", "Start", "OnEnable", "DisableAllScanElements", "DisplayNewScrapFound", "Update", "UpdateScanNodes", "AddChatMessage"])]
internal static class HUDManagerPatch
{
    private static CallbackContext pingScan;

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

        // Transpiler
        On.HUDManager.AddChatMessage.ILHook(ILHook_AddChatMessage);
    }

    private static void OnScanTriggered(HUDManager self,ref CallbackContext context)
    {
        pingScan = context;
        if (ModCompats.IsGoodItemScanPresent)
            ScanNodeController.ResetGoodItemScanNodes();
    }

    private static void OnHUDManagerStart(HUDManager self)
    {
        ScanController.SetScanColor();
        ScanController.UpdateScanTexture();
        PlayerHPDisplay.Init();
        if (ModCompats.IsBetterScanVisionPresent)
            BetterScanVisionProxy.OverrideNightVisionColor();
    }

    private static void OnHUDManagerEnable(HUDManager self)
    {
        if (self.gameObject.GetComponent<LethalHUDMono>() == null)
        {
            self.gameObject.AddComponent<LethalHUDMono>();
        }
        if (self.gameObject.GetComponent<FPSAndPingCounter>() == null)
        {
            self.gameObject.AddComponent<FPSAndPingCounter>();
        }
        ChatController.ColorChatInputField(self.chatTextField,Time.time * 0.25f);
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


    }
    private static void OnHUDManagerUpdateScanNodes(HUDManager self, ref PlayerControllerB playerScript)
    {
        if (Plugins.ConfigEntries.ScanNodeFade.Value)
            ScanNodeController.UpdateTimers(self.scanElements, self.scanNodes);
    }


    private static void ILHook_AddChatMessage(ILManipulationInfo info)
    {
        ILWeaver w = new(info);

        // Patch player name coloring
        w.MatchMultipleStrict(
            matchWeaver => { matchWeaver.InsertAfterCurrent(matchWeaver.CreateCall(ChatController.GetColoredPlayerName)); },
            x => x.MatchLdarg(2) && w.SetCurrentTo(x)
        );

        // Patch hardcoded blue chat color <color=#7069ff>
        w.MatchMultipleStrict(
            matchWeaver =>
            {
                matchWeaver.Remove(matchWeaver.Current, out _);
                matchWeaver.InsertAfterCurrent(matchWeaver.CreateCall(ChatController.GetDefaultChatColorTag));
            },
            x => x.MatchLdstr("<color=#7069ff>") && w.SetCurrentTo(x)
        );
    }
}