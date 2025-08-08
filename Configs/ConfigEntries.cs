using BepInEx.Configuration;
using LethalHUD.HUD;
using LethalHUD.Scan;
using System;
using Unity.Netcode;
using static LethalHUD.HUD.InventoryGradientEnums;
using static LethalHUD.Scan.ScanlinesEnums;

namespace LethalHUD.Configs;
internal class ConfigEntries
{
    public static ConfigFile ConfigFile { get; private set; }
    internal DateTime _lastScanColorChange = DateTime.MinValue;
    internal DateTime _lastMainColorChange = DateTime.MinValue;
    internal DateTime _lastSlotColorChange = DateTime.MinValue;

    private const string DefaultMainColor = "#000CFF";
    private const string DefaultSlotColor = "#3226B4";

    public ConfigEntries()
    {
        Setup();
        ConfigHelper.ClearUnusedEntries();
    }

    #region Main
    public ConfigEntry<string> MainColor { get; private set; }
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
    #endregion

    #region Chat ConfigEntries
    public ConfigEntry<bool> NameColors { get; private set; }
    public ConfigEntry<string> LocalNameColor { get; private set; }
    #endregion
    public void Setup()
    {
        ConfigHelper.SkipAutoGen();

        #region Main Binds
        MainColor = ConfigHelper.Bind(true, "Main", "MainColor", "#000CFF", "Allows you to change the scan and inventory frames colors in HEX format in a unified way. (Default value: #000CFF)");
        #endregion


        #region Scan Binds
        HoldScan = ConfigHelper.Bind("Scan", "Hold Scan Button", false, "Allows you to keep holding scan button.");
        FadeOut = ConfigHelper.Bind("Scan", "FadeOut", false, "Fade out effect for scan color.");
        RecolorScanLines = ConfigHelper.Bind("Scan", "RecolorScanLines", true, "Recolor the blue horizontal scan lines texture aswell.");
        SelectedScanlineMode = ConfigHelper.Bind("Scan", "Scanline", ScanLines.Default, "Select the scanline style.", false);
        DirtIntensity = ConfigHelper.Bind("Scan", "Scanline Intensity", 0f, "Set the scanline's intensity yourself. (Default value for vanilla: 352.08, others are: 100", false, new AcceptableValueRange<float>(-500f, 500f));
        ScanColor = ConfigHelper.Bind(true, "Scan", "ScanColor", "#000CFF", "Allows you to change the scan's color in HEX format. (Default value: #000CFF)");
        Alpha = ConfigHelper.Bind("Scan", "Alpha", 0.26f, "Alpha / opacity.", false, new AcceptableValueRange<float>(0f, 1f));
        VignetteIntensity = ConfigHelper.Bind("Scan", "VignetteIntensity", 0.46f, "Intensity of the vignette / borders effect during scan.", false,new AcceptableValueRange<float>(0f, 1f));
        #endregion

        #region InventorySlot Binds
        SlotColor = ConfigHelper.Bind(true, "Inventory", "FrameColor", "#3226B4", "Allows you to change the inventoryslot colors  (Default value: #3226B4)");
        SlotRainbowColor = ConfigHelper.Bind("Inventory", "RainbowFrames", SlotEnums.None, "If true, inventory slot frames are colored with a rainbow gradient.");
        GradientColorA = ConfigHelper.Bind(true, "Inventory", "GradientColorA", "#3226B4", "Start color for custom wavy gradient.");
        GradientColorB = ConfigHelper.Bind(true, "Inventory", "GradientColorB", "#3226B4", "End color for custom wavy gradient.");
        #endregion

        #region Chat Binds
        NameColors = ConfigHelper.Bind("Chat", "ColoredNames", false, "Enable colored player names in chat (only visible to others who also have this enabled.");
        LocalNameColor = ConfigHelper.Bind(true, "Chat", "NameColor", "#FF0000", "Change your name's color in chat in HEX format");
        #endregion

        #region Main Changes
        MainColor.SettingChanged += (obj, args) =>
        {
            _lastMainColorChange = DateTime.Now;
            if (MainColor.Value.Equals(DefaultMainColor, StringComparison.OrdinalIgnoreCase))
            {
                if (!ScanColor.Value.Equals(DefaultMainColor, StringComparison.OrdinalIgnoreCase))
                    ScanColor.Value = DefaultMainColor;

                if (!SlotColor.Value.Equals(DefaultSlotColor, StringComparison.OrdinalIgnoreCase))
                    SlotColor.Value = DefaultSlotColor;

                return;
            }


            if (_lastMainColorChange > _lastScanColorChange &&
                !MainColor.Value.Equals(ScanColor.Value, StringComparison.OrdinalIgnoreCase))
            {
                ScanColor.Value = MainColor.Value;
            }

            if (_lastMainColorChange > _lastSlotColorChange &&
                !MainColor.Value.Equals(SlotColor.Value, StringComparison.OrdinalIgnoreCase))
            {
                SlotColor.Value = MainColor.Value;
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
        NameColors.SettingChanged += (obj, args) =>
        {
            if (NameColors.Value)
            {
                ChatController.Instance.UpdateLocalColor();
            }
        };
        LocalNameColor.SettingChanged += (obj, args) =>
        {
            if (NameColors.Value)
            {
                ChatController.Instance.UpdateLocalColor();
            }
        };
        #endregion
    }
}