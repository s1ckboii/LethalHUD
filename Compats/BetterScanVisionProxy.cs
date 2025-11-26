using BetterScanVision;
using LethalHUD.Configs;
using System.Runtime.CompilerServices;

namespace LethalHUD.Compats;
internal class BetterScanVisionProxy
{
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void OverrideNightVisionColor()
    {
        if (!ModCompats.IsBetterScanVisionPresent) return;

        if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null)
            return;

        if (PlayerControllerBPatches.nightVision != null)
            PlayerControllerBPatches.nightVision.color = ConfigHelper.GetScanColor();
    }
}