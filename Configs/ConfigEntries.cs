using BepInEx.Configuration;
using LethalHUD.Compats;
using LethalHUD.Events;
using LethalHUD.HUD;
using LethalHUD.Misc;
using LethalHUD.Scan;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using static LethalHUD.Enums;

namespace LethalHUD.Configs;
public class ConfigEntries
{
    public static ConfigFile ConfigFile { get; private set; }
    internal DateTime _lastScanColorChange = DateTime.MinValue;
    internal DateTime _lastUnifyMostColorsChange = DateTime.MinValue;
    internal DateTime _lastSlotColorChange = DateTime.MinValue;
    internal DateTime _lastLocalNameColorChange = DateTime.MinValue;

    internal const string DefaultMainColor = "#000CFF";
    private const string DefaultSlotColor = "#3226B4";
    private const string DefaultCompassColor = "#2C265B";

    public ConfigEntries()
    {
        ConfigFile configFile = Plugins.Config;

        // Disable saving config after a call to 'Bind()' is made.
        configFile.SaveOnConfigSet = false;

        // Bind config entries.
        Setup();

        // Remove old config settings.
        configFile.OrphanedEntries.Clear();

        // Re-enable saving and save config.
        configFile.SaveOnConfigSet = true;
        configFile.Save();
    }

    #region Main ConfigEntries
    public ConfigEntry<string> UnifyMostColors { get; private set; }
    public ConfigEntry<bool> EventSystemEnabled { get; private set; }
    public ConfigEntry<string> ForcedEvent { get; private set; }
    #endregion

    #region Scan ConfigEntries
    public ConfigEntry<ScanMode> ScanModeType { get; private set; }
    public ConfigEntry<bool> FadeOut { get; private set; }
    public ConfigEntry<bool> RecolorScanLines { get; private set; }
    public ConfigEntry<float> Alpha { get; private set; }
    public ConfigEntry<float> DirtIntensity { get; private set; }
    public ConfigEntry<string> ScanColor { get; private set; }
    public ConfigEntry<float> VignetteIntensity { get; private set; }
    public ConfigEntry<ScanLines> SelectedScanlineMode { get; private set; }
    #endregion
    #region InventorySlot ConfigEntries
    public ConfigEntry<float> SlotFade { get; private set; }
    public ConfigEntry<float> SlotFadeDelayTime { get; private set; }
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
    public ConfigEntry<string> HealthColor { get; private set; }
    public ConfigEntry<float> HPIndicatorX { get; private set; }
    public ConfigEntry<float> HPIndicatorY { get; private set; }
    public ConfigEntry<bool> SprintBool { get; private set; }
    public ConfigEntry<SprintStyle> SprintColoring { get; private set; }
    public ConfigEntry<string> SprintMeterColor { get; private set; }
    public ConfigEntry<WeightUnit> WeightUnitConfig { get; private set; }
    public ConfigEntry<WeightDecimalFormat> WeightDecimalFormatConfig { get; private set; }
    public ConfigEntry<WeightUnitDisplay> WeightUnitDisplayConfig { get; private set; }
    public ConfigEntry<string> WeightStarterColor { get; private set; }
    #endregion
    #region Compass ConfigEntries
    public ConfigEntry<bool> CompassInvertMask { get; private set; }
    public ConfigEntry<bool> CompassInvertOutsides { get; private set; }
    public ConfigEntry<float> CompassAlpha { get; private set; }
    #endregion
    #region Chat ConfigEntries
    public ConfigEntry<float> ChatFadeDelayTime { get; private set; }
    public ConfigEntry<bool> ColoredNames { get; private set; }
    public ConfigEntry<string> GradientNameColorA { get; private set; }
    public ConfigEntry<string> GradientNameColorB { get; private set; }
    public ConfigEntry<string> ChatInputText { get; private set; }
    public ConfigEntry<string> ChatMessageColor { get; private set; }
    public ConfigEntry<string> GradientMessageColorA { get; private set; }
    public ConfigEntry<string> GradientMessageColorB { get; private set; }
    #endregion
    #region Clock ConfigEntries
    public ConfigEntry<bool> NormalHumanBeingClock { get; private set; }
    //public ConfigEntry<float> SpectatorClockVisibility { get; private set; }
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
    #region Signal ConfigEntries
    public ConfigEntry<bool> CenterSTText { get; private set; }
    public ConfigEntry<string> SignalTextColor { get; private set; }
    public ConfigEntry<string> SignalText2Color { get; private set; }
    public ConfigEntry<string> SignalBGColor { get; private set; }
    public ConfigEntry<string> SignalMessageColor { get; private set; }
    public ConfigEntry<float> SignalLetterDisplay { get; private set; }

    #endregion
    #region MoreDisplay ConfigEntries
    public ConfigEntry<string> LoadingTextColor { get; private set; }
    public ConfigEntry<string> PlanetSummaryColor { get; private set; }
    public ConfigEntry<string> PlanetHeaderColor { get; private set; }
    public ConfigEntry<bool> PlanetRisk { get; private set; }
    #endregion

    #region Spectator HUD ConfigEntries
    public ConfigEntry<string> SpectatorTipColor { get; private set; }
    public ConfigEntry<string> SpectatingPlayerColor { get; private set; }
    public ConfigEntry<string> HoldEndGameColor { get; private set; }
    public ConfigEntry<string> HoldEndGameVotesColor { get; private set; }
    #endregion

    #region Misc ConfigEntries
    public ConfigEntry<float> TerminalFadeDelaysTime { get; private set; }
    public ConfigEntry<bool> ShowFPSDisplay { get; private set; }
    public ConfigEntry<bool> ShowPingDisplay { get; private set; }
    public ConfigEntry<bool> ShowSeedDisplay { get; private set; }
    //public ConfigEntry<bool> ReplaceScrapCounterVisual { get; private set; }
    //public ConfigEntry<bool> ShowShipLoot { get; private set; }
    //public ConfigEntry<float> DisplayTime { get; private set; }
    public ConfigEntry<FPSPingLayout> MiscLayoutEnum { get; private set; }
    public ConfigEntry<MTColorMode> MTColorSelection { get; private set; }
    public ConfigEntry<bool> SplitAdditionalMTFromToolTips { get; private set; }
    public ConfigEntry<string> SeperateAdditionalMiscToolsColors { get; private set; }
    public ConfigEntry<string> MTColorGradientA { get; private set; }
    public ConfigEntry<string> MTColorGradientB { get; private set; }
    public ConfigEntry<float> FPSCounterX { get; private set; }
    public ConfigEntry<float> FPSCounterY { get; private set; }
    #endregion
    #region KeyBind ConfigEntries
    public ConfigEntry<Key> HideHUDButton { get; private set; }
    #endregion
    public void Setup()
    {
        ConfigHelper.SkipAutoGen();

#if DEBUG
        LethalConfigProxy.AddButton("Dev", "Reset Configs", "Resets all configs to default for testing.", "Reset", () => ConfigHelper.ResetMyConfigs());
#endif

        // Init with None
        List<string> eventNames = ["None"];

        if (!EventColorManager.HasLoaded)
            EventColorManager.Load();

        EventColorConfig eventConfig = EventColorManager.Config;
        if (eventConfig?.Events != null)
        {
            foreach (var ev in eventConfig.Events)
                eventNames.Add(ev.Name);
        }

        #region Main Binds
        UnifyMostColors = ConfigHelper.Bind(true, "Main", "Main Color", "#000CFF", "Allows you to change the scan and inventory frames colors in HEX format in a unified way, on reset they go back to default.");
        EventSystemEnabled = ConfigHelper.Bind("Main", "Event System Enabled", true, "Enable event color overrides based on the JSON (e.g., Halloween, Winterfest).");
        ForcedEvent = ConfigHelper.Bind("Main", "Forced Event", "None", "Force a specific event color scheme. Set to 'None' to disable.", false, new AcceptableValueList<string>([.. eventNames]));
        #endregion

        #region Scan Binds
        ScanModeType = ConfigHelper.Bind("Scan", "Scan Mode", ScanMode.Default, "Choose the scan mode.");
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
        SlotFade = ConfigHelper.Bind("Inventory", "Inventory Fade", 0.13f, "Change the base fadeout for the inventory.", false, new AcceptableValueRange<float>(0f, 0.99f));
        SlotFadeDelayTime = ConfigHelper.Bind("Inventory", "Inventory Fade Delay", 1.5f, "Change the delay time for fading out.", false, new AcceptableValueRange<float>(0f, 10f));
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
        ChatFadeDelayTime = ConfigHelper.Bind("Chat", "Chat Fade Delay", 5f, "Change the delay time for fading out chat.", false, new AcceptableValueRange<float>(0f, 20f));
        ColoredNames = ConfigHelper.Bind("Chat", "Colored Names", false, "Enable colored player names in chat (In the future, currently its only client-sided -> only visible to others who also have this enabled).");
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
        HealthColor = ConfigHelper.Bind(true, "Health/Stamina/Weight", "Health Color", "#00CC00", "Base color for HP Indicator.");
        HPIndicatorX = ConfigHelper.Bind("Health/Stamina/Weight", "HP Indicator X", -300f, "X position of the HP Indicator counter on screen.", false, new AcceptableValueRange<float>(-360f, 520));
        HPIndicatorY = ConfigHelper.Bind("Health/Stamina/Weight", "HP Indicator Y", 110f, "Y position of the HP Indicator counter on screen.", false, new AcceptableValueRange<float>(-250f, 250f));
        SprintBool = ConfigHelper.Bind("Health/Stamina/Weight", "Sprint Meter", true, "Enable sprint meter coloring.");
        SprintColoring = ConfigHelper.Bind("Health/Stamina/Weight", "Sprint Meter Style", SprintStyle.Solid, "Choose a style for the sprint meter.");
        SprintMeterColor = ConfigHelper.Bind(true, "Health/Stamina/Weight", "Sprint Meter Color Solid", "#FF7600", "Base color for sprint meter");
        WeightUnitConfig = ConfigHelper.Bind("Health/Stamina/Weight", "Weight Unit", WeightUnit.Pounds, "Select the weight unit.");
        WeightDecimalFormatConfig = ConfigHelper.Bind("Health/Stamina/Weight", "Weight Decimal Format", WeightDecimalFormat.Rounded, "Choose how many decimals should be shown.");
        WeightUnitDisplayConfig = ConfigHelper.Bind("Health/Stamina/Weight", "Weight Unit Display", WeightUnitDisplay.OnlyOne, "Choose how the weight unit should be displayed.");
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
        //SpectatorClockVisibility = ConfigHelper.Bind("Clock", "Spectator Clock", 0f, "Show clock when spectating.", false, new AcceptableValueRange<float>(0f, 1f));
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
        #region Signal Binds
        CenterSTText = ConfigHelper.Bind("Signal Translator", "Center Transmitter Text", false, "Center the message.");
        SignalTextColor = ConfigHelper.Bind(true, "Signal Translator", "Signal Message Color", "#1BFF00", "Color of the signal message.");
        SignalText2Color = ConfigHelper.Bind(true, "Signal Translator", "Signal BackgroundMessage Color", "#FF00E7", "Color of the signal message.");
        SignalBGColor = ConfigHelper.Bind(true, "Signal Translator", "Signal Background Color", "#1BFF00", "Color of the signal message.");
        SignalMessageColor = ConfigHelper.Bind(true, "Signal Translator", "Signal Message Background Color", "#0AFF00", "Background color of the signal message.");
        SignalLetterDisplay = ConfigHelper.Bind("Signal Translator", "Signal Letter Display", 0.7f, "Change the speed of the message (the smaller the number, the faster it is.)", false, new AcceptableValueRange<float>(0.01f, 1f));
        #endregion
        #region MoreDisplay Binds
        LoadingTextColor = ConfigHelper.Bind(true, "More Display", "Loading Text Color", "#A5F4FF", "Color of the loading text.");
        PlanetSummaryColor = ConfigHelper.Bind(true, "More Display", "Planet Summary Color", "#86ECFF", "Color of the planet summary text.");
        PlanetHeaderColor = ConfigHelper.Bind(true, "More Display", "Planet Header Color", "#86ECFF", "Color of the planet header text.");
        PlanetRisk = ConfigHelper.Bind("More Display", "Planet Risk Color", true, "Custom coloring based on risk level.");
        #endregion
        #region Spectator HUD Binds
        SpectatorTipColor = ConfigHelper.Bind(true, "Spectator", "Tip Text Color", "#FF4834", "Color of the spectator tip text. (I have absolutely no idea which one is this, good luck)");
        SpectatingPlayerColor = ConfigHelper.Bind(true, "Spectator", "Spectating Player Text Color", "#D4D4D4", "Color of the 'Spectating Player' text.");
        HoldEndGameColor = ConfigHelper.Bind(true, "Spectator", "Hold To End Game Text Color", "#FFFFFF", "Color of the 'Hold to End Game Early' text.");
        HoldEndGameVotesColor = ConfigHelper.Bind(true, "Spectator", "Hold To End Game Votes Text Color", "#BCBCBC", "Color of the 'Votes' text.");
        #endregion
        #region Misc Binds
        TerminalFadeDelaysTime = ConfigHelper.Bind("Misc", "Terminal Fade Delay", 0.5f, "Change the delay time for fading out HUD stuff.", false, new AcceptableValueRange<float>(0f, 5f));
        ShowFPSDisplay = ConfigHelper.Bind("Misc", "FPS Counter", false, "Enables an FPS counter.");
        ShowPingDisplay = ConfigHelper.Bind("Misc", "Ping Counter", false, "Display the current network ping (ms) on the HUD.");
        ShowSeedDisplay = ConfigHelper.Bind("Misc", "Seed Display", false, "Display current seed.");
        //ReplaceScrapCounterVisual = ConfigHelper.Bind("Misc", "Scrap Counter Visual", false, "Replace total value scanner with shiploot visual.");
        //ShowShipLoot = ConfigHelper.Bind("Misc", "Ship Loot", false, "Enable ship loot info");
        //DisplayTime = ConfigHelper.Bind("Misc", "Ship Loot Display Time", 5f, "Change how long ship loot should be displayed.", false, new AcceptableValueRange<float>(0f, 20f));
        MiscLayoutEnum = ConfigHelper.Bind("Misc", "Layout options", FPSPingLayout.Vertical, "Layout of FPS and Ping counters");
        FPSCounterX = ConfigHelper.Bind("Misc", "Layout position X", 10f, "X position of the FPS counter on screen.", false, new AcceptableValueRange<float>(0f, 840f));
        FPSCounterY = ConfigHelper.Bind("Misc", "Layout position Y", 10f, "Y position of the FPS counter on screen.", false, new AcceptableValueRange<float>(0f, 480f));
        MTColorSelection = ConfigHelper.Bind("Misc", "Control Tip Color Mode", MTColorMode.Solid, "Change the color mode of control tips (On solid you only use 'Misc Tools Gradient Color A').");
        SplitAdditionalMTFromToolTips = ConfigHelper.Bind("Misc", "Separate Additional Misc Tool Colors", false, "Allows you to choose a different color for additional tooltips (FPS, Ping, Seed Counters).");
        SeperateAdditionalMiscToolsColors = ConfigHelper.Bind(true, "Misc", "Additional Misc Tools Color", "#FFFFFF", "Color for additional tooltips (FPS, Ping, Seed Counters).");
        MTColorGradientA = ConfigHelper.Bind(true, "Misc", "Misc Tools Gradient Color A", "#FFFFFF", "Starting color for misc and tooltips.");
        MTColorGradientB = ConfigHelper.Bind(true, "Misc", "Misc Tools Gradient Color B", "#FFFFFF", "Ending color for misc and tooltips.");
        #endregion
        #region Keybind Binds
        HideHUDButton = ConfigHelper.Bind("Keybinds", "Hide HUD Button", Key.Numpad5, "Keybind to hide the HUD.");
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

        #region Chat Changes
        GradientNameColorA.SettingChanged += (obj, args) => { ChatController.ApplyLocalPlayerColor(Plugins.ConfigEntries.GradientNameColorA.Value, Plugins.ConfigEntries.GradientNameColorB.Value); };
        GradientNameColorB.SettingChanged += (obj, args) => { ChatController.ApplyLocalPlayerColor(Plugins.ConfigEntries.GradientNameColorA.Value, Plugins.ConfigEntries.GradientNameColorB.Value); };
        #endregion

        #region Clock Changes
        ClockFormat.SettingChanged += (obj, args) => { ClockController.ApplyClockAppearance(); };
        ClockSizeMultiplier.SettingChanged += (obj, args) => { ClockController.ApplyClockAppearance(); };
        ClockNumberColor.SettingChanged += (obj, args) => { ClockController.ApplyClockAppearance(); };
        ClockBoxColor.SettingChanged += (obj, args) => { ClockController.ApplyClockAppearance(); };
        ClockIconColor.SettingChanged += (obj, args) => { ClockController.ApplyClockAppearance(); };
        ClockShipLeaveColor.SettingChanged += (obj, args) => { ClockController.ApplyClockAppearance(); };
        #endregion
        #region HSW Changes
        SprintColoring.SettingChanged += (obj, args) =>
        {
            SprintMeterController.UpdateSprintMeterColor();
        };
        SprintMeterColor.SettingChanged += (obj, args) =>
        {
            SprintMeterController.UpdateSprintMeterColor();
        };
        WeightUnitConfig.SettingChanged += (obj, args) =>
        {
            WeightController.UpdateWeightDisplay();
        };
        WeightUnitDisplayConfig.SettingChanged += (obj, args) =>
        {
            WeightController.UpdateWeightDisplay();
        };
        WeightDecimalFormatConfig.SettingChanged += (obj, args) =>
        {
            WeightController.UpdateWeightDisplay();
        };
        ShowTotalDelta.SettingChanged += (obj, args) => { ScrapValueDisplay.UpdateTotalTextPosition(); };
        TotalPrefix.SettingChanged += (obj, args) => { ScrapValueDisplay.UpdateTotalTextPosition(); };
        TotalValueOffsetX.SettingChanged += (obj, args) => { ScrapValueDisplay.UpdateTotalTextPosition(); };
        TotalValueOffsetY.SettingChanged += (obj, args) => { ScrapValueDisplay.UpdateTotalTextPosition(); };
        #endregion
        #region Signal Changes
        //CenterSTText.SettingChanged += (obj, args) => { SignalTranslatorController.CenterText(); };
        #endregion
        #region MoreDisplay Changes
        LoadingTextColor.SettingChanged += (obj, args) => { PlanetInfoDisplay.ApplyColors(); };
        PlanetSummaryColor.SettingChanged += (ob, args) => { PlanetInfoDisplay.ApplyColors(); };
        PlanetHeaderColor.SettingChanged += (obj, args) => { PlanetInfoDisplay.ApplyColors(); };
        PlanetRisk.SettingChanged += (obj, args) => { PlanetInfoDisplay.ApplyColors(); };
        #endregion
        #region Spectator HUD Changes
        SpectatorTipColor.SettingChanged += (obj, args) => { SpectatorHUDController.ApplyColors(); };
        SpectatingPlayerColor.SettingChanged += (obj, args) => { SpectatorHUDController.ApplyColors(); };
        HoldEndGameColor.SettingChanged += (obj, args) => { SpectatorHUDController.ApplyColors(); };
        HoldEndGameVotesColor.SettingChanged += (obj, args) => { SpectatorHUDController.ApplyColors(); };
        #endregion

        ForcedEvent.SettingChanged += (obj, args) => { EventColorManager.Update(); };
    }
}