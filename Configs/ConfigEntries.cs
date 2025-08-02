using BepInEx.Configuration;
using LethalHUD.HUD;
using LethalHUD.Scan;
using System;
using static LethalHUD.Scan.ScanlinesEnums;

namespace LethalHUD.Configs;
public sealed class ConfigEntries
{
    internal DateTime _lastScanColorChange = DateTime.MinValue;
    internal DateTime _lastMainColorChange = DateTime.MinValue;
    internal DateTime _lastSlotColorChange = DateTime.MinValue;


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
    public ConfigEntry<bool> RandomColor { get; private set; }
    public ConfigEntry<ScanLines> SelectedScanlineMode { get; private set; }
    #endregion

    #region InventorySlot ConfigEntries
    public ConfigEntry<string> SlotColor { get; private set; }
    #endregion

    private static ConfigEntries instance = null;
    public static ConfigEntries Instance
    {
        get
        {
            instance ??= new ConfigEntries();
            return instance;
        }
    }

    public void Setup()
    {
        #region Main Binds
        MainColor = Plugins.BepInExConfig().Bind("Main", "MainColor", "#000CFF", "Allows you to change the scan and inventory frames colors in HEX format in a unified way. (Default value: #000CFF)");
        #endregion


        #region Scan Binds
        HoldScan = Plugins.BepInExConfig().Bind("Scan", "Hold Scan Button", false, "Allows you to keep holding scan button.");
        FadeOut = Plugins.BepInExConfig().Bind("Scan", "FadeOut", false, new ConfigDescription("Fade out effect for scan color."));
        RecolorScanLines = Plugins.BepInExConfig().Bind("Scan", "RecolorScanLines", true, new ConfigDescription("Recolor the blue horizontal scan lines texture aswell."));
        SelectedScanlineMode = Plugins.BepInExConfig().Bind("Scan", "Scanline", ScanLines.Default, "Select the scanline style.");
        DirtIntensity = Plugins.BepInExConfig().Bind("Scan", "Scanline Intensity", 0f, new ConfigDescription("Set the scanline's intensity yourself. (Default value for vanilla: 352.08, others are: 42", new AcceptableValueRange<float>(-500f, 500f)));
        ScanColor = Plugins.BepInExConfig().Bind("Scan", "ScanColor", "#000CFF", "Allows you to change the scan's color in HEX format. (Default value: #000CFF)");
        Alpha = Plugins.BepInExConfig().Bind("Scan", "Alpha", 0.26f, new ConfigDescription("Alpha / opacity.", new AcceptableValueRange<float>(0f, 1f)));
        VignetteIntensity = Plugins.BepInExConfig().Bind("Scan", "VignetteIntensity", 0.46f, new ConfigDescription("Intensity of the vignette / borders effect during scan.", new AcceptableValueRange<float>(0f, 1f)));
        RandomColor = Plugins.BepInExConfig().Bind("Scan", "RandomColor", false, "Random color per button press ~ish kinda");
        #endregion

        #region InventorySlot Binds
        SlotColor = Plugins.BepInExConfig().Bind("Inventory", "FrameColor", "#000CFF", "Allows you to change the inventoryslot colors  (Default value: #000CFF)");
        #endregion

        #region Main Changes
        MainColor.SettingChanged += (obj, args) =>
        {
            _lastMainColorChange = DateTime.Now;
            if (_lastMainColorChange > _lastScanColorChange)
                ScanController.SetScanColor();
            if (_lastMainColorChange > _lastSlotColorChange)
                InventoryFrames.ApplySlotColors();
        };
        #endregion


        #region Scan Changes
        SelectedScanlineMode.SettingChanged += (obj, args) => ScanController.UpdateScanTexture();
        ScanColor.SettingChanged += (obj, args) =>
        {
            _lastScanColorChange = DateTime.Now;
            ScanController.SetScanColor();
        };
        Alpha.SettingChanged += (obj, args) => { ScanController.SetScanColor(); };
        DirtIntensity.SettingChanged += (obj, args) => { ScanController.SetScanColor(); };
        FadeOut.SettingChanged += (obj, args) => { ScanController.SetScanColor(); };
        RandomColor.SettingChanged += (obj, args) => { if (!RandomColor.Value) ScanController.SetScanColor(); };
        VignetteIntensity.SettingChanged += (obj, args) => { ScanController.UpdateVignetteIntensity(); };
        RecolorScanLines.SettingChanged += (obj, args) => { ScanController.UpdateScanTexture(); };
        #endregion

        #region Inventory Changes
        SlotColor.SettingChanged += (obj, args) =>
        {
            _lastSlotColorChange = DateTime.Now;
            InventoryFrames.ApplySlotColors();
        };
        #endregion
    }
}