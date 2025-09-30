using LethalHUD.HUD;
using HarmonyLib;

namespace LethalHUD.Patches;

[HarmonyPatch(typeof(TimeOfDay))]
internal static class TimeOfDayPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("MoveTimeOfDay")]
    private static void OnTimeOfDayMoveTimeOfDay()
    {
        ClockController.ApplyRealtimeClock();
    }
}
