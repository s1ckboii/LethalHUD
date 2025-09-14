using BepInEx.Configuration;
using LethalHUD.HUD;
using LethalHUD.Scan;
using System;
using UnityEngine;
using static LethalHUD.Enums;

namespace LethalHUD.Configs;
public class ConfigEntries
{
    public static ConfigFile ConfigFile { get; private set; }
    internal DateTime _lastScanColorChange = DateTime.MinValue;
    internal DateTime _lastUnifyMostColorsChange = DateTime.MinValue;
    internal DateTime _lastSlotColorChange = DateTime.MinValue;
    internal DateTime _lastLocalNameColorChange = DateTime.MinValue;

    private const string DefaultMainColor = "#000CFF";
    private const string DefaultSlotColor = "#3226B4";
    private const string DefaultCompassColor = "#2C265B";

    public ConfigEntries()
    {
        Setup();
        ConfigHelper.ClearUnusedEntries();
    }

    #region Main ConfigEntries
    public ConfigEntry<string> UnifyMostColors { get; private set; }
    #endregion

    #region Scan ConfigEntries
    public ConfigEntry<bool> HoldScan { get; private set; }
    public ConfigEntry<bool> FadeOut { get; private set; }
    public ConfigEntry<bool> RecolorScanLines { get; private set; }
    public ConfigEntry<float> Alpha { get; private set; }
    public ConfigEntry<float> DirtIntensity { get; private set; }
    public ConfigEntry<string> ScanColor { get; private set; }
    public ConfigEntry<float> VignetteIntensity { get; private set; }
    public ConfigEntry<ScanLines> SelectedScanlineMode { get; private set; }
    #endregion
    #region InventorySlot ConfigEntries
    public ConfigEntry<string> SlotColor { get; private set; }
    public ConfigEntry<SlotEnums> SlotRainbowColor { get; private set; }
    public ConfigEntry<string> GradientColorA { get; private set; }
    public ConfigEntry<string> GradientColorB { get; private set; }
    public ConfigEntry<string> HandsFullColor { get; private set; }
    public ConfigEntry<bool> ShowItemValue { get; private set; }
    public ConfigEntry<bool> ShowTotalInventoryValue { get; private set; }
    public ConfigEntry<bool> ShowTotalDelta { get; private set; }
    public ConfigEntry<TotalValuePrefix> TotalPrefix { get; private set; }
    public ConfigEntry<ItemValue> SetDollar { get; private set; }
    public ConfigEntry<float> TotalValueOffsetX { get; private set; }
    public ConfigEntry<float> TotalValueOffsetY { get; private set; }
    #endregion
    #region ScanNode ConfigEntries
    public ConfigEntry<bool> ScanNodeFade { get; private set; }
    public ConfigEntry<float> ScanNodeLifetime { get; private set; }
    public ConfigEntry<float> ScanNodeFadeDuration { get; private set; }
    public ConfigEntry<ScanNodeShape> ScanNodeShapeChoice { get; private set; }
    #endregion
    #region HSW ConfigEntries
    public ConfigEntry<bool> HealthIndicator { get; private set; }
    public ConfigEntry<HPDisplayMode> HealthFormat { get; private set; }
    public ConfigEntry<int> HealthSize { get; private set; }
    public ConfigEntry<int> HealthRotation { get; private set; }
    public ConfigEntry<bool> HealthStarterColor { get; private set; }
    public ConfigEntry<float> HPIndicatorX { get; private set; }
    public ConfigEntry<float> HPIndicatorY { get; private set; }
    public ConfigEntry<bool> SprintMeterBoolean { get; private set; }
    public ConfigEntry<string> SprintMeterColorGradient { get; private set; }
    public ConfigEntry<string> SprintMeterColorSolid { get; private set; }
    public ConfigEntry<string> SprintMeterColorShades { get; private set; }
    public ConfigEntry<bool> WeightCounterBoolean { get; private set; }
    public ConfigEntry<WeightUnit> WeightUnitConfig { get; private set; }
    public ConfigEntry<string> WeightStarterColor { get; private set; }
    #endregion
    #region Compass ConfigEntries
    public ConfigEntry<bool> CompassInvertMask { get; private set; }
    public ConfigEntry<bool> CompassInvertOutsides { get; private set; }
    public ConfigEntry<float> CompassAlpha { get; private set; }
    #endregion
    #region Chat ConfigEntries
    public ConfigEntry<bool> ColoredNames { get; private set; }
    public ConfigEntry<string> LocalNameColor { get; private set; }
    public ConfigEntry<string> GradientNameColorA { get; private set; }
    public ConfigEntry<string> GradientNameColorB { get; private set; }
    public ConfigEntry<string> ChatInputText { get; private set; }
    public ConfigEntry<string> ChatMessageColor { get; private set; }
    public ConfigEntry<string> GradientMessageColorA { get; private set; }
    public ConfigEntry<string> GradientMessageColorB { get; private set; }
    #endregion
    #region Clock ConfigEntries
    public ConfigEntry<bool> NormalHumanBeingClock { get; private set; }
    public ConfigEntry<ClockStyle> ClockFormat { get; private set; }
    public ConfigEntry<bool> RealtimeClock { get; private set; }
    public ConfigEntry<float> ClockSizeMultiplier { get; private set; }
    public ConfigEntry<string> ClockNumberColor { get; private set; }
    public ConfigEntry<string> ClockBoxColor { get; private set; }
    public ConfigEntry<string> ClockIconColor { get; private set; }
    public ConfigEntry<string> ClockShipLeaveColor { get; private set; }
    public ConfigEntry<bool> ShowClockInShip { get; private set; }
    public ConfigEntry<bool> ShowClockInFacility { get; private set; }
    public ConfigEntry<float> ClockVisibilityInShip { get; private set; }
    public ConfigEntry<float> ClockVisibilityInFacility { get; private set; }
    #endregion
    #region Misc ConfigEntries
    public ConfigEntry<bool> ShowFPSDisplay { get; private set; }
    public ConfigEntry<bool> ShowPingDisplay { get; private set; }
    public ConfigEntry<bool> ShowSeedDisplay { get; private set; }
    //public ConfigEntry<bool> ReplaceScrapCounterVisual { get; private set; }
    //public ConfigEntry<bool> ShowShipLoot { get; private set; }
    //public ConfigEntry<float> DisplayTime { get; private set; }
    public ConfigEntry<FPSPingLayout> MiscLayoutEnum { get; private set; }
    public ConfigEntry<string> MiscToolsColor { get; private set; }
    public ConfigEntry<float> FPSCounterX { get; private set; }
    public ConfigEntry<float> FPSCounterY { get; private set; }
    #endregion
    public void Setup()
    {
        ConfigHelper.SkipAutoGen();

        #region Main Binds
        UnifyMostColors = ConfigHelper.Bind(true, "Main", "Main Color", "#000CFF", "Allows you to change the scan and inventory frames colors in HEX format in a unified way, on reset they go back to default.");
        #endregion

        #region Scan Binds
        HoldScan = ConfigHelper.Bind("Scan", "Hold Scan Button", false, "Allows you to keep holding scan button.");
        FadeOut = ConfigHelper.Bind("Scan", "Fade Out", true, "Fade out effect for scan color.");
        RecolorScanLines = ConfigHelper.Bind("Scan", "Recolor Scan Lines", true, "Recolor the blue horizontal scan lines texture aswell.");
        SelectedScanlineMode = ConfigHelper.Bind("Scan", "Scanline", ScanLines.Default, "Select the scanline style.", false);
        DirtIntensity = ConfigHelper.Bind("Scan", "Scanline Intensity", 0f, "Set the scanline's intensity yourself. (Default value for vanilla: 352.08, others are: 100", false, new AcceptableValueRange<float>(-500f, 500f));
        ScanColor = ConfigHelper.Bind(true, "Scan", "Scan Color", "#000CFF", "Allows you to change the scan's color in HEX format.");
        Alpha = ConfigHelper.Bind("Scan", "Alpha", 0.26f, "Alpha / opacity.", false, new AcceptableValueRange<float>(0f, 1f));
        VignetteIntensity = ConfigHelper.Bind("Scan", "Vignette Intensity", 0.46f, "Intensity of the vignette / borders effect during scan.", false, new AcceptableValueRange<float>(0f, 1f));
        #endregion
        #region ScanNode Binds
        ScanNodeFade = ConfigHelper.Bind("ScanNodes", "Fade Away", true, "Allows you to apply fadeaway for scannodes.");
        ScanNodeLifetime = ConfigHelper.Bind("ScanNodes", "Lifetime", 3f, "Change how long it is visible before fading away.", false, new AcceptableValueRange<float>(0f, 10f));
        ScanNodeFadeDuration = ConfigHelper.Bind("ScanNodes", "Fade Duration", 1f, "Change how long it takes to fade out.", false, new AcceptableValueRange<float>(0f, 5f));
        ScanNodeShapeChoice = ConfigHelper.Bind("ScanNodes", "Shape", ScanNodeShape.Default, "Choose the shape for scan nodes.");
        #endregion
        #region InventorySlot Binds
        SlotColor = ConfigHelper.Bind(true, "Inventory", "Frame Color", "#3226B4", "Allows you to change the inventoryslot colors.");
        SlotRainbowColor = ConfigHelper.Bind("Inventory", "Rainbow Frames", SlotEnums.None, "If true, inventory slot frames are colored with a rainbow gradient.");
        GradientColorA = ConfigHelper.Bind(true, "Inventory", "Gradient Color A", "#3226B4", "Start color for custom wavy gradient.");
        GradientColorB = ConfigHelper.Bind(true, "Inventory", "Gradient Color B", "#3226B4", "End color for custom wavy gradient.");
        HandsFullColor = ConfigHelper.Bind(true, "Inventory", "Hands Full Color", "#3A00FF", "Change the color of the Hands Full text when holding a two handed item.");
        ShowItemValue = ConfigHelper.Bind("Inventory", "Show Item Value", false, "Enable quality of life visual helper, you can see the value of the items in your inventory");
        ShowTotalInventoryValue = ConfigHelper.Bind("Inventory", "Show Total Inventory Value", false, "Enable quality of life visual helper, you can see the total value of the items in your inventory");
        ShowTotalDelta = ConfigHelper.Bind("Inventory", "Show Total Delta", true, "This shows the + and - numbers next to the inventory total value.");
        TotalPrefix = ConfigHelper.Bind("Inventory", "Total Prefix", TotalValuePrefix.Full, "Change inventory total value text.");
        SetDollar = ConfigHelper.Bind("Inventory", "Change Currency", ItemValue.Default, "Let's you change from blocky credit to dollar sign (no Wesley, I'm not doing conversions to world currencies).");
        TotalValueOffsetX = ConfigHelper.Bind("Inventory", "Total Value Offset X", -200f ,"X position of the inventory total value text.", false, new AcceptableValueRange<float>(-360f, 520f));
        TotalValueOffsetY = ConfigHelper.Bind("Inventory", "Total Value Offset Y", -55f, "Y position of the inventory total value text.", false, new AcceptableValueRange<float>(-250f, 250f));
        #endregion
        #region Chat Binds
        ColoredNames = ConfigHelper.Bind("Chat", "Colored Names", false, "Enable colored player names in chat (In the future, currently its only client-sided -> only visible to others who also have this enabled).");
        LocalNameColor = ConfigHelper.Bind(true, "Chat", "Local Name Color", "#FF0000", "Change your name's (currently everyones) color in chat in HEX format.");
        GradientNameColorA = ConfigHelper.Bind(true, "Chat", "Gradient Name Color A", "#FF0000", "Starting color for a gradient, if both left untouched LocalNameColor takes priority.");
        GradientNameColorB = ConfigHelper.Bind(true, "Chat", "Gradient Name Color B", "#FF0000", "Ending color for a gradient, if both left untouched LocalNameColor takes priority.");
        ChatInputText = ConfigHelper.Bind(true, "Chat", "Chat Input Text", "#FFFF00", "Change input text's color.");
        ChatMessageColor = ConfigHelper.Bind(true, "Chat", "Chat Message Color", "#FFFF00", "Change the chat message's color.");
        GradientMessageColorA = ConfigHelper.Bind(true, "Chat", "Gradient Message Color A", "#FFFF00", "Starting color for a gradient, if both left untouched ChatMessageColor takes priority.");
        GradientMessageColorB = ConfigHelper.Bind(true, "Chat", "Gradient Message Color B", "#FFFF00", "Ending color for a gradient, if both left untouched ChatMessageColor takes priority.");
        #endregion
        #region HSW Binds
        HealthIndicator = ConfigHelper.Bind("Health/Stamina/Weight", "Health Indicator", true, "Enable health points indicator.");
        HealthFormat = ConfigHelper.Bind("Health/Stamina/Weight", "Health Format", HPDisplayMode.Plain, "Change the display mode of the HP Indicator.");
        HealthSize = ConfigHelper.Bind("Health/Stamina/Weight", "Health Size", 24, "Change the fontsize of the HP Indicator.", false, new AcceptableValueRange<int>(1, 50));
        HealthRotation = ConfigHelper.Bind("Health/Stamina/Weight", "Health Rotation", 356, "Change the rotation of the HP Indicator.", false, new AcceptableValueRange<int>(0, 359));
        HealthStarterColor = ConfigHelper.Bind("Health/Stamina/Weight", "Health Starter Color", false, "Takes the color of the inventory slots as starter color.");
        HPIndicatorX = ConfigHelper.Bind("Health/Stamina/Weight", "HP Indicator X", -300f, "X position of the HP Indicator counter on screen.", false, new AcceptableValueRange<float>(-360f, 520));
        HPIndicatorY = ConfigHelper.Bind("Health/Stamina/Weight", "HP Indicator Y", 110f, "Y position of the HP Indicator counter on screen.", false, new AcceptableValueRange<float>(-250f, 250f));
        SprintMeterBoolean = ConfigHelper.Bind("Health/Stamina/Weight", "Sprint Meter Configuration", false, "Enable color configs for sprintmeter.");
        SprintMeterColorSolid = ConfigHelper.Bind(true, "Health/Stamina/Weight", "Sprint Meter Color Solid", "#FF7600", "Fixed solid color for [SOLID] sprint meter mode.");
        SprintMeterColorGradient = ConfigHelper.Bind(true, "Health/Stamina/Weight", "Sprint Meter Color Gradient", "#FF7600", "Base color for [GRADIENT] sprint meter mode.");
        SprintMeterColorShades = ConfigHelper.Bind(true, "Health/Stamina/Weight", "Sprint Meter Color Shades", "#FF7600", "Base color for [SHADES] sprint meter mode.");
        WeightCounterBoolean = ConfigHelper.Bind("Health/Stamina/Weight", "Weight Counter Configuration", false, "Enable configs for weightcounter.");
        WeightUnitConfig = ConfigHelper.Bind("Health/Stamina/Weight", "Weight Unit", WeightUnit.Pounds, "Select the weight unit.");
        WeightStarterColor = ConfigHelper.Bind(true, "Health/Stamina/Weight", "WeightColor", "#E55901", "The starting base color for weight display in hex format.");
        #endregion
        #region Compass Binds
        CompassInvertMask = ConfigHelper.Bind("Compass", "Compass Invert Mask", false, "Lets you invert the mask in the inside.");
        CompassInvertOutsides = ConfigHelper.Bind("Compass", "Compass Invert Outsides", false, "Lets you invert the mask on the outside.");
        CompassAlpha = ConfigHelper.Bind("Compass", "Compass Alpha", 1f, "Lets you change the alpha value of Compass.", false, new AcceptableValueRange<float>(0f, 1f));
        #endregion
        #region Clock Binds
        NormalHumanBeingClock = ConfigHelper.Bind("Clock", "24 Hour Clock", false, "Toggle between 12-hour (AM/PM) and 24-hour clock formats.");
        ClockFormat = ConfigHelper.Bind("Clock", "Clock Format", ClockStyle.Regular, "Choose a clock format.");
        RealtimeClock = ConfigHelper.Bind("Clock", "Realtime Clock", false, "Toggle fixed clock numbers, no more jumpy numbers.");
        ClockSizeMultiplier = ConfigHelper.Bind("Clock", "Clock Size Multiplier", 1f, "Change the size of the clock.", false, new AcceptableValueRange<float>(0.69f, 3f));
        ClockNumberColor = ConfigHelper.Bind(true, "Clock", "Clock Number Color", "#FF4C00", "Color of the clock’s numbers.");
        ClockBoxColor = ConfigHelper.Bind(true, "Clock", "Clock Box Color", "#FF4C00", "Color of the box around the clock.");
        ClockIconColor = ConfigHelper.Bind(true, "Clock", "Clock Icon Color", "#FF4C00", "Color of the icon.");
        ClockShipLeaveColor = ConfigHelper.Bind(true, "Clock", "Clock Ship Leave Color", "#FF4C00", "Color of the ship leave icon.");
        ShowClockInShip = ConfigHelper.Bind("Clock", "Show Clock In Ship", false, "Toggle whether the clock is visible while you’re inside the ship.");
        ShowClockInFacility = ConfigHelper.Bind("Clock", "Show Clock Inside", false, "Toggle whether the clock is visible inside facilities.");
        ClockVisibilityInShip = ConfigHelper.Bind("Clock", "Clock Visibility In Ship", 1f, "Adjust transparency of the clock while in the ship.", false, new AcceptableValueRange<float>(0.01f, 1f));
        ClockVisibilityInFacility = ConfigHelper.Bind("Clock", "Clock Visibility Inside", 1f, "Adjust transparency of the clock while inside facilities.", false, new AcceptableValueRange<float>(0.01f, 1f));
        #endregion
        #region Misc Binds
        ShowFPSDisplay = ConfigHelper.Bind("Misc", "FPS Counter", false, "Enables an FPS counter.");
        ShowPingDisplay = ConfigHelper.Bind("Misc", "Ping Counter", false, "Display the current network ping (ms) on the HUD.");
        ShowSeedDisplay = ConfigHelper.Bind("Misc", "Seed Display", false, "Display current seed.");
        //ReplaceScrapCounterVisual = ConfigHelper.Bind("Misc", "Scrap Counter Visual", false, "Replace total value scanner with shiploot visual.");
        //ShowShipLoot = ConfigHelper.Bind("Misc", "Ship Loot", false, "Enable ship loot info");
        //DisplayTime = ConfigHelper.Bind("Misc", "Ship Loot Display Time", 5f, "Change how long ship loot should be displayed.", false, new AcceptableValueRange<float>(0f, 20f));
        MiscLayoutEnum = ConfigHelper.Bind("Misc", "Layout options", FPSPingLayout.Vertical, "Layout of FPS and Ping counters");
        FPSCounterX = ConfigHelper.Bind("Misc", "Layout position X", 10f, "X position of the FPS counter on screen.", false, new AcceptableValueRange<float>(0f, 840f));
        FPSCounterY = ConfigHelper.Bind("Misc", "Layout position Y", 10f, "Y position of the FPS counter on screen.", false, new AcceptableValueRange<float>(0f, 480f));
        MiscToolsColor = ConfigHelper.Bind(true, "Misc", "Misc Tools Color", "#FFFFFF", "Color misc tools (fps and ping counters)");
        #endregion

        #region Main Changes
        UnifyMostColors.SettingChanged += (obj, args) =>
        {
            _lastUnifyMostColorsChange = DateTime.Now;
            if (UnifyMostColors.Value.Equals(DefaultMainColor, StringComparison.OrdinalIgnoreCase))
            {
                if (!ScanColor.Value.Equals(DefaultMainColor, StringComparison.OrdinalIgnoreCase))
                    ScanColor.Value = DefaultMainColor;

                if (!SlotColor.Value.Equals(DefaultSlotColor, StringComparison.OrdinalIgnoreCase))
                    SlotColor.Value = DefaultSlotColor;
                CompassController.SetCompassColor(HUDUtils.ParseHexColor(DefaultCompassColor));

                return;
            }


            if (_lastUnifyMostColorsChange > _lastScanColorChange &&
                !UnifyMostColors.Value.Equals(ScanColor.Value, StringComparison.OrdinalIgnoreCase))
            {
                ScanColor.Value = UnifyMostColors.Value;
            }

            if (_lastUnifyMostColorsChange > _lastSlotColorChange &&
                !UnifyMostColors.Value.Equals(SlotColor.Value, StringComparison.OrdinalIgnoreCase))
            {
                SlotColor.Value = UnifyMostColors.Value;
            }
        };
        #endregion

        #region Scan Changes
        SelectedScanlineMode.SettingChanged += (obj, args) => ScanController.UpdateScanTexture();
        ScanColor.SettingChanged += (obj, args) =>
        {
            _lastScanColorChange = DateTime.Now;
            ScanController.SetScanColor();
        };
        Alpha.SettingChanged += (obj, args) => { ScanController.UpdateScanAlpha(); };
        DirtIntensity.SettingChanged += (obj, args) => { ScanController.SetScanColor(); };
        FadeOut.SettingChanged += (obj, args) => { ScanController.UpdateScanAlpha(); };
        VignetteIntensity.SettingChanged += (obj, args) => { ScanController.UpdateVignetteIntensity(); };
        RecolorScanLines.SettingChanged += (obj, args) => { ScanController.UpdateScanTexture(); };
        #endregion
        #region ScanNode Changes
        ScanNodeLifetime.SettingChanged += (obj, args) =>
        {
            if (Plugins.ConfigEntries.ScanNodeFade.Value)
                ScanNodeController.lifetime = ScanNodeLifetime.Value;
        };
        ScanNodeFadeDuration.SettingChanged += (obj, args) =>
        {
            if (Plugins.ConfigEntries.ScanNodeFade.Value)
                ScanNodeController.fadeDuration = ScanNodeFadeDuration.Value;
        };
        ScanNodeShapeChoice.SettingChanged += (obj, args) => { ScanNodeTextureManager.ForceRefresh(); };
        #endregion
        #region Inventory Changes
        SlotColor.SettingChanged += (obj, args) =>
        {
            _lastSlotColorChange = DateTime.Now;
            InventoryFrames.SetSlotColors();
        };
        SlotRainbowColor.SettingChanged += (obj, args) => { InventoryFrames.SetSlotColors(); };
        GradientColorA.SettingChanged += (obj, args) => { InventoryFrames.SetSlotColors(); };
        GradientColorB.SettingChanged += (obj, args) => { InventoryFrames.SetSlotColors(); };
        #endregion
        /*
        #region Chat Changes
        LocalNameColor.SettingChanged += (obj, args) =>
        {
            ChatController.ApplyLocalPlayerColor(Plugins.ConfigEntries.LocalNameColor.Value);
        };
        GradientNameColorA.SettingChanged += (obj, args) =>
        {
            ChatController.ApplyLocalPlayerColor(
                Plugins.ConfigEntries.GradientNameColorA.Value,
                Plugins.ConfigEntries.GradientNameColorB.Value
            );
        };
        GradientNameColorB.SettingChanged += (obj, args) =>
        {
            ChatController.ApplyLocalPlayerColor(
                Plugins.ConfigEntries.GradientNameColorA.Value,
                Plugins.ConfigEntries.GradientNameColorB.Value
            );
        };
        #endregion
        */
        #region Clock Changes
        ClockFormat.SettingChanged += (obj, args) => { ClockController.ApplyClockAppearance(); };
        ClockSizeMultiplier.SettingChanged += (obj, args) => { ClockController.ApplyClockAppearance(); };
        ClockNumberColor.SettingChanged += (obj, args) => { ClockController.ApplyClockAppearance(); };
        ClockBoxColor.SettingChanged += (obj, args) => { ClockController.ApplyClockAppearance(); };
        ClockIconColor.SettingChanged += (obj, args) => { ClockController.ApplyClockAppearance(); };
        ClockShipLeaveColor.SettingChanged += (obj, args) => { ClockController.ApplyClockAppearance(); };
        #endregion
        #region HSW Changes
        SprintMeterColorSolid.SettingChanged += (obj, args) =>
        {
            if (Plugins.ConfigEntries.SprintMeterBoolean.Value)
            {
                PlayerPrefs.SetString(SprintMeter.PlayerPrefsKey, "Solid");
                PlayerPrefs.Save();
                SprintMeter.UpdateSprintMeterColor();
            }

        };
        SprintMeterColorGradient.SettingChanged += (obj, args) =>
        {
            if (Plugins.ConfigEntries.SprintMeterBoolean.Value)
            {
                PlayerPrefs.SetString(SprintMeter.PlayerPrefsKey, "Gradient");
                PlayerPrefs.Save();
                SprintMeter.UpdateSprintMeterColor();
            }
        };
        SprintMeterColorShades.SettingChanged += (obj, args) =>
        {
            if (Plugins.ConfigEntries.SprintMeterBoolean.Value)
            {
                PlayerPrefs.SetString(SprintMeter.PlayerPrefsKey, "Shades");
                PlayerPrefs.Save();
                SprintMeter.UpdateSprintMeterColor();
            }
        };
        WeightUnitConfig.SettingChanged += (obj, args) =>
        {
            if (Plugins.ConfigEntries.WeightCounterBoolean.Value)
                WeightController.UpdateWeightDisplay();
        };
        ShowTotalDelta.SettingChanged += (obj, args) => { ScrapValueDisplay.UpdateTotalTextPosition(); };
        TotalPrefix.SettingChanged += (obj, args) => { ScrapValueDisplay.UpdateTotalTextPosition(); };
        TotalValueOffsetX.SettingChanged += (obj, args) => { ScrapValueDisplay.UpdateTotalTextPosition(); };
        TotalValueOffsetY.SettingChanged += (obj, args) => { ScrapValueDisplay.UpdateTotalTextPosition(); };
        #endregion
    }
}