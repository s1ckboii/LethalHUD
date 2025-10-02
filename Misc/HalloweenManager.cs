using LethalHUD.Configs;
using LethalHUD.HUD;
using System;
using static LethalHUD.Enums;

namespace LethalHUD.Misc;
public class HalloweenManager
{
    [Serializable]
    private class StoredValues
    {
        public string ScanColor;
        public string SlotColor;
        public string GradientColorA;
        public string GradientColorB;
        public string HandsFullColor;
        public SprintStyle SprintColoring;
        public string SprintMeterColor;
        public string WeightStarterColor;
        public string ClockNumberColor;
        public string ClockBoxColor;
        public string ClockIconColor;
        public string ClockShipLeaveColor;
        public string SignalTextColor;
        public string GradientNameColorA;
        public string GradientNameColorB;
        public string ChatInputText;
        public string ChatMessageColor;
        public string GradientMessageColorA;
        public string GradientMessageColorB;
        public string LoadingTextColor;
        public string PlanetHeaderColor;
        public string PlanetSummaryColor;
        public MTColorMode MTColorMode;
        public string MTColorGradientA;
        public string MTColorGradientB;
        public string HealthColor;
    }

    private StoredValues _originalValues;
    private bool _ignoreUnifyMostColors;

    public static HalloweenManager Instance { get; } = new HalloweenManager();
    public bool IgnoreUnifyMostColors => _ignoreUnifyMostColors;

    public void ApplyHalloweenMode()
    {
        EnsureOriginalValues();

        _ignoreUnifyMostColors = true;

        Plugins.ConfigEntries.ScanColor.Value = "#6611BB";
        Plugins.ConfigEntries.SlotColor.Value = "#FF8800";
        Plugins.ConfigEntries.GradientColorA.Value = "#CC4400";
        Plugins.ConfigEntries.GradientColorB.Value = "#AA55EE";
        Plugins.ConfigEntries.HandsFullColor.Value = "#FF0000";
        Plugins.ConfigEntries.SprintColoring.Value = SprintStyle.Gradient;
        Plugins.ConfigEntries.SprintMeterColor.Value = "#AA55EE";
        Plugins.ConfigEntries.WeightStarterColor.Value = "#FF8800";
        Plugins.ConfigEntries.ClockNumberColor.Value = "#AA55EE";
        Plugins.ConfigEntries.ClockBoxColor.Value = "#6611BB";
        Plugins.ConfigEntries.ClockIconColor.Value = "#FF8800";
        Plugins.ConfigEntries.ClockShipLeaveColor.Value = "#FF8800";
        Plugins.ConfigEntries.SignalTextColor.Value = "#AA55EE";
        Plugins.ConfigEntries.GradientNameColorA.Value = "#AA55EE";
        Plugins.ConfigEntries.GradientNameColorB.Value = "#FF8800";
        Plugins.ConfigEntries.ChatInputText.Value = "#6611BB";
        Plugins.ConfigEntries.ChatMessageColor.Value = "#AA55EE";
        Plugins.ConfigEntries.GradientMessageColorA.Value = "#FF8800";
        Plugins.ConfigEntries.GradientMessageColorB.Value = "#AA55EE";
        Plugins.ConfigEntries.LoadingTextColor.Value = "#FF8800";
        Plugins.ConfigEntries.PlanetHeaderColor.Value = "#AA55EE";
        Plugins.ConfigEntries.PlanetSummaryColor.Value = "#FF8800";
        Plugins.ConfigEntries.MTColorSelection.Value = MTColorMode.Gradient;
        Plugins.ConfigEntries.MTColorGradientA.Value = "#AA55EE";
        Plugins.ConfigEntries.MTColorGradientB.Value = "#FF8800";
        Plugins.ConfigEntries.HealthColor.Value = "#AA55EE";

        RefreshUI();
        _ignoreUnifyMostColors = false;
    }

    public void RestoreOriginalValues()
    {
        if (_originalValues == null) return;

        _ignoreUnifyMostColors = true;

        Plugins.ConfigEntries.ScanColor.Value = _originalValues.ScanColor;
        Plugins.ConfigEntries.SlotColor.Value = _originalValues.SlotColor;
        Plugins.ConfigEntries.GradientColorA.Value = _originalValues.GradientColorA;
        Plugins.ConfigEntries.GradientColorB.Value = _originalValues.GradientColorB;
        Plugins.ConfigEntries.HandsFullColor.Value = _originalValues.HandsFullColor;
        Plugins.ConfigEntries.SprintColoring.Value = _originalValues.SprintColoring;
        Plugins.ConfigEntries.SprintMeterColor.Value = _originalValues.SprintMeterColor;
        Plugins.ConfigEntries.WeightStarterColor.Value = _originalValues.WeightStarterColor;
        Plugins.ConfigEntries.ClockNumberColor.Value = _originalValues.ClockNumberColor;
        Plugins.ConfigEntries.ClockBoxColor.Value = _originalValues.ClockBoxColor;
        Plugins.ConfigEntries.ClockIconColor.Value = _originalValues.ClockIconColor;
        Plugins.ConfigEntries.ClockShipLeaveColor.Value = _originalValues.ClockShipLeaveColor;
        Plugins.ConfigEntries.SignalTextColor.Value = _originalValues.SignalTextColor;
        Plugins.ConfigEntries.GradientNameColorA.Value = _originalValues.GradientNameColorA;
        Plugins.ConfigEntries.GradientNameColorB.Value = _originalValues.GradientNameColorB;
        Plugins.ConfigEntries.ChatInputText.Value = _originalValues.ChatInputText;
        Plugins.ConfigEntries.ChatMessageColor.Value = _originalValues.ChatMessageColor;
        Plugins.ConfigEntries.GradientMessageColorA.Value = _originalValues.GradientMessageColorA;
        Plugins.ConfigEntries.GradientMessageColorB.Value = _originalValues.GradientMessageColorB;
        Plugins.ConfigEntries.LoadingTextColor.Value = _originalValues.LoadingTextColor;
        Plugins.ConfigEntries.PlanetHeaderColor.Value = _originalValues.PlanetHeaderColor;
        Plugins.ConfigEntries.PlanetSummaryColor.Value = _originalValues.PlanetSummaryColor;
        Plugins.ConfigEntries.MTColorSelection.Value = _originalValues.MTColorMode;
        Plugins.ConfigEntries.MTColorGradientA.Value = _originalValues.MTColorGradientA;
        Plugins.ConfigEntries.MTColorGradientB.Value = _originalValues.MTColorGradientB;
        Plugins.ConfigEntries.HealthColor.Value = _originalValues.HealthColor;

        RefreshUI();
        _ignoreUnifyMostColors = false;
    }

    public void RestoreOnLoad()
    {
        var loadedValues = ConfigUtils.LoadStoredValues<StoredValues>("Halloween");
        if (loadedValues != null)
            _originalValues = loadedValues;

        if (Plugins.ConfigEntries.HalloweenMode.Value)
            ApplyHalloweenMode();
        else if (_originalValues != null)
            RestoreOriginalValues();
    }

    private void RefreshUI()
    {
        InventoryFrames.SetSlotColors();
        ChatController.ApplyLocalPlayerColor(Plugins.ConfigEntries.GradientNameColorA.Value, Plugins.ConfigEntries.GradientNameColorB.Value);
        ClockController.ApplyClockAppearance();

        SprintMeterController.UpdateSprintMeterColor();
        WeightController.UpdateWeightDisplay();
    }

    private void EnsureOriginalValues()
    {
        _originalValues ??= ConfigUtils.LoadStoredValues<StoredValues>("Halloween") ?? new StoredValues();

        bool updated = false;
        void UpdateField<T>(ref T field, T value)
        {
            if (!Equals(field, value))
            {
                field = value;
                updated = true;
            }
        }

        UpdateField(ref _originalValues.ScanColor, Plugins.ConfigEntries.ScanColor.Value);
        UpdateField(ref _originalValues.SlotColor, Plugins.ConfigEntries.SlotColor.Value);
        UpdateField(ref _originalValues.GradientColorA, Plugins.ConfigEntries.GradientColorA.Value);
        UpdateField(ref _originalValues.GradientColorB, Plugins.ConfigEntries.GradientColorB.Value);
        UpdateField(ref _originalValues.HandsFullColor, Plugins.ConfigEntries.HandsFullColor.Value);
        UpdateField(ref _originalValues.SprintColoring, Plugins.ConfigEntries.SprintColoring.Value);
        UpdateField(ref _originalValues.SprintMeterColor, Plugins.ConfigEntries.SprintMeterColor.Value);
        UpdateField(ref _originalValues.WeightStarterColor, Plugins.ConfigEntries.WeightStarterColor.Value);
        UpdateField(ref _originalValues.ClockNumberColor, Plugins.ConfigEntries.ClockNumberColor.Value);
        UpdateField(ref _originalValues.ClockBoxColor, Plugins.ConfigEntries.ClockBoxColor.Value);
        UpdateField(ref _originalValues.ClockIconColor, Plugins.ConfigEntries.ClockIconColor.Value);
        UpdateField(ref _originalValues.ClockShipLeaveColor, Plugins.ConfigEntries.ClockShipLeaveColor.Value);
        UpdateField(ref _originalValues.SignalTextColor, Plugins.ConfigEntries.SignalTextColor.Value);
        UpdateField(ref _originalValues.GradientNameColorA, Plugins.ConfigEntries.GradientNameColorA.Value);
        UpdateField(ref _originalValues.GradientNameColorB, Plugins.ConfigEntries.GradientNameColorB.Value);
        UpdateField(ref _originalValues.ChatInputText, Plugins.ConfigEntries.ChatInputText.Value);
        UpdateField(ref _originalValues.ChatMessageColor, Plugins.ConfigEntries.ChatMessageColor.Value);
        UpdateField(ref _originalValues.GradientMessageColorA, Plugins.ConfigEntries.GradientMessageColorA.Value);
        UpdateField(ref _originalValues.GradientMessageColorB, Plugins.ConfigEntries.GradientMessageColorB.Value);
        UpdateField(ref _originalValues.LoadingTextColor, Plugins.ConfigEntries.LoadingTextColor.Value);
        UpdateField(ref _originalValues.PlanetHeaderColor, Plugins.ConfigEntries.PlanetHeaderColor.Value);
        UpdateField(ref _originalValues.PlanetSummaryColor, Plugins.ConfigEntries.PlanetSummaryColor.Value);
        UpdateField(ref _originalValues.MTColorMode, Plugins.ConfigEntries.MTColorSelection.Value);
        UpdateField(ref _originalValues.MTColorGradientA, Plugins.ConfigEntries.MTColorGradientA.Value);
        UpdateField(ref _originalValues.MTColorGradientB, Plugins.ConfigEntries.MTColorGradientB.Value);
        UpdateField(ref _originalValues.HealthColor, Plugins.ConfigEntries.HealthColor.Value);

        if (updated)
            ConfigUtils.SaveStoredValues(_originalValues, "Halloween");
    }
}