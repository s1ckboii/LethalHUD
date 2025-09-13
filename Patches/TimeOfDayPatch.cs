using LethalHUD.HUD;
using MonoDetour;
using MonoDetour.HookGen;

namespace LethalHUD.Patches;

[MonoDetourTargets(typeof(TimeOfDay), Members = ["MoveTimeOfDay"])]
internal static class TimeOfDayPatch
{
    [MonoDetourHookInitialize]
    public static void Init()
    {
        On.TimeOfDay.MoveTimeOfDay.Prefix(OnTimeOfDayMoveTimeOfDay);
    }

    private static void OnTimeOfDayMoveTimeOfDay(TimeOfDay self)
    {
        ClockController.ApplyRealtimeClock();
    }
}
