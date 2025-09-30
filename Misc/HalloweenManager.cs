using LethalHUD.Configs;
using LethalHUD.HUD;
using System;

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
        public string SprintMeterColor;
        public string WeightStarterColor;
        public string ClockNumberColor;
        public string ClockBoxColor;
        public string ClockIconColor;
        public string ClockShipLeaveColor;
        public string SignalTextColor;
        public string MiscToolsColor;
        public string GradientNameColorA;
        public string GradientNameColorB;
        public string ChatInputText;
        public string ChatMessageColor;
        public string GradientMessageColorA;
        public string GradientMessageColorB;
    }

    private StoredValues _originalValues;
    private bool _ignoreUnifyMostColors;

    public static HalloweenManager Instance { get; } = new HalloweenManager();
    public bool IgnoreUnifyMostColors => _ignoreUnifyMostColors;

    public void ApplyHalloweenMode()
    {
        if (_originalValues == null)
        {
            _originalValues = ConfigUtils.LoadStoredValues<StoredValues>("Halloween")
                              ?? new StoredValues
                              {
                                  ScanColor = Plugins.ConfigEntries.ScanColor.Value,
                                  SlotColor = Plugins.ConfigEntries.SlotColor.Value,
                                  GradientColorA = Plugins.ConfigEntries.GradientColorA.Value,
                                  GradientColorB = Plugins.ConfigEntries.GradientColorB.Value,
                                  HandsFullColor = Plugins.ConfigEntries.HandsFullColor.Value,
                                  SprintMeterColor = Plugins.ConfigEntries.SprintMeterColor.Value,
                                  WeightStarterColor = Plugins.ConfigEntries.WeightStarterColor.Value,
                                  ClockNumberColor = Plugins.ConfigEntries.ClockNumberColor.Value,
                                  ClockBoxColor = Plugins.ConfigEntries.ClockBoxColor.Value,
                                  ClockIconColor = Plugins.ConfigEntries.ClockIconColor.Value,
                                  ClockShipLeaveColor = Plugins.ConfigEntries.ClockShipLeaveColor.Value,
                                  SignalTextColor = Plugins.ConfigEntries.SignalTextColor.Value,
                                  MiscToolsColor = Plugins.ConfigEntries.MiscToolsColor.Value,
                                  GradientNameColorA = Plugins.ConfigEntries.GradientNameColorA.Value,
                                  GradientNameColorB = Plugins.ConfigEntries.GradientNameColorB.Value,
                                  ChatInputText = Plugins.ConfigEntries.ChatInputText.Value,
                                  ChatMessageColor = Plugins.ConfigEntries.ChatMessageColor.Value,
                                  GradientMessageColorA = Plugins.ConfigEntries.GradientMessageColorA.Value,
                                  GradientMessageColorB = Plugins.ConfigEntries.GradientMessageColorB.Value
                              };

            ConfigUtils.SaveStoredValues(_originalValues, "Halloween");
        }

        _ignoreUnifyMostColors = true;

        Plugins.ConfigEntries.ScanColor.Value = "#6611BB";
        Plugins.ConfigEntries.SlotColor.Value = "#FF8800";
        Plugins.ConfigEntries.GradientColorA.Value = "#CC4400";
        Plugins.ConfigEntries.GradientColorB.Value = "#AA55EE";
        Plugins.ConfigEntries.HandsFullColor.Value = "#FF0000";
        Plugins.ConfigEntries.SprintMeterColor.Value = "#AA55EE";
        Plugins.ConfigEntries.WeightStarterColor.Value = "#FF8800";
        Plugins.ConfigEntries.ClockNumberColor.Value = "#AA55EE";
        Plugins.ConfigEntries.ClockBoxColor.Value = "#6611BB";
        Plugins.ConfigEntries.ClockIconColor.Value = "#FF8800";
        Plugins.ConfigEntries.ClockShipLeaveColor.Value = "#FF8800";
        Plugins.ConfigEntries.SignalTextColor.Value = "#AA55EE";
        Plugins.ConfigEntries.MiscToolsColor.Value = "#FFFFFF";
        Plugins.ConfigEntries.GradientNameColorA.Value = "#55D4EE";
        Plugins.ConfigEntries.GradientNameColorB.Value = "#00FF86";
        Plugins.ConfigEntries.ChatInputText.Value = "#6611BB";
        Plugins.ConfigEntries.ChatMessageColor.Value = "#AA55EE";
        Plugins.ConfigEntries.GradientMessageColorA.Value = "#FF8800";
        Plugins.ConfigEntries.GradientMessageColorB.Value = "#AA55EE";

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
        Plugins.ConfigEntries.SprintMeterColor.Value = _originalValues.SprintMeterColor;
        Plugins.ConfigEntries.WeightStarterColor.Value = _originalValues.WeightStarterColor;
        Plugins.ConfigEntries.ClockNumberColor.Value = _originalValues.ClockNumberColor;
        Plugins.ConfigEntries.ClockBoxColor.Value = _originalValues.ClockBoxColor;
        Plugins.ConfigEntries.ClockIconColor.Value = _originalValues.ClockIconColor;
        Plugins.ConfigEntries.ClockShipLeaveColor.Value = _originalValues.ClockShipLeaveColor;
        Plugins.ConfigEntries.SignalTextColor.Value = _originalValues.SignalTextColor;
        Plugins.ConfigEntries.MiscToolsColor.Value = _originalValues.MiscToolsColor;
        Plugins.ConfigEntries.GradientNameColorA.Value = _originalValues.GradientNameColorA;
        Plugins.ConfigEntries.GradientNameColorB.Value = _originalValues.GradientNameColorB;
        Plugins.ConfigEntries.ChatInputText.Value = _originalValues.ChatInputText;
        Plugins.ConfigEntries.ChatMessageColor.Value = _originalValues.ChatMessageColor;
        Plugins.ConfigEntries.GradientMessageColorA.Value = _originalValues.GradientMessageColorA;
        Plugins.ConfigEntries.GradientMessageColorB.Value = _originalValues.GradientMessageColorB;

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
        ChatController.ApplyLocalPlayerColor(
            Plugins.ConfigEntries.GradientNameColorA.Value,
            Plugins.ConfigEntries.GradientNameColorB.Value
        );
        ClockController.ApplyClockAppearance();

        if (Plugins.ConfigEntries.SprintMeterBoolean.Value)
            SprintMeterController.UpdateSprintMeterColor();
        if (Plugins.ConfigEntries.WeightCounterBoolean.Value)
            WeightController.UpdateWeightDisplay();
    }
}
