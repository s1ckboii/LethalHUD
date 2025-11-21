using Newtonsoft.Json;

namespace LethalHUD.Configs;
public class ConfigJsonProperties
{
    [JsonProperty("Main Color")]
    public string UnifyMostColors { get; set; }

    [JsonProperty("Scan Color")]
    public string ScanColor { get; set; }

    [JsonProperty("Inventory Color")]
    public string SlotColor { get; set; }

    [JsonProperty("Inventory Gradient Color A")]
    public string GradientColorA { get; set; }

    [JsonProperty("Inventory Gradient Color B")]
    public string GradientColorB { get; set; }

    [JsonProperty("Hands Full Color")]
    public string HandsFullColor { get; set; }

    [JsonProperty("Health Color")]
    public string HealthColor { get; set; }

    [JsonProperty("Sprint Meter Color")]
    public string SprintMeterColor { get; set; }

    [JsonProperty("Weight Starter Color")]
    public string WeightStarterColor { get; set; }

    [JsonProperty("Gradient Name Color A")]
    public string GradientNameColorA { get; set; }

    [JsonProperty("Gradient Name Color B")]
    public string GradientNameColorB { get; set; }

    [JsonProperty("Chat Message Color")]
    public string ChatMessageColor { get; set; }

    [JsonProperty("Gradient Message Color A")]
    public string GradientMessageColorA { get; set; }

    [JsonProperty("Gradient Message Color B")]
    public string GradientMessageColorB { get; set; }

    [JsonProperty("Clock Number Color")]
    public string ClockNumberColor { get; set; }

    [JsonProperty("Clock Box Color")]
    public string ClockBoxColor { get; set; }

    [JsonProperty("Clock Icon Color")]
    public string ClockIconColor { get; set; }

    [JsonProperty("Clock Ship Leave Color")]
    public string ClockShipLeaveColor { get; set; }

    [JsonProperty("Signal Text Color")]
    public string SignalTextColor { get; set; }

    [JsonProperty("Signal Text2 Color")]
    public string SignalText2Color { get; set; }

    [JsonProperty("Signal Background Color")]
    public string SignalBGColor { get; set; }

    [JsonProperty("Signal Message Color")]
    public string SignalMessageColor { get; set; }

    [JsonProperty("Loading Text Color")]
    public string LoadingTextColor { get; set; }

    [JsonProperty("Planet Summary Color")]
    public string PlanetSummaryColor { get; set; }

    [JsonProperty("Planet Header Color")]
    public string PlanetHeaderColor { get; set; }

    [JsonProperty("Spectator Tip Color")]
    public string SpectatorTipColor { get; set; }

    [JsonProperty("Spectating Player Color")]
    public string SpectatingPlayerColor { get; set; }

    [JsonProperty("Hold End Game Color")]
    public string HoldEndGameColor { get; set; }

    [JsonProperty("Hold End Game Votes Color")]
    public string HoldEndGameVotesColor { get; set; }

    [JsonProperty("Seperate Additional Misc Tools Colors")]
    public string SeperateAdditionalMiscToolsColors { get; set; }

    [JsonProperty("Misc Tools Color Gradient A")]
    public string MTColorGradientA { get; set; }

    [JsonProperty("Misc Tools Color Gradient B")]
    public string MTColorGradientB { get; set; }
}
