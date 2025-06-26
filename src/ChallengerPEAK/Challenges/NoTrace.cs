using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace ChallengerPEAK.Challenges;

public class NoTrace : Challenge
{
    // ReSharper disable once InconsistentNaming
    private const string _id = Plugin.Id + ".NoTrace";
    private readonly Harmony _harmony = new(_id);

    public override string ID => _id;
    public override string Title => "Leave No Trace";
    public override string Description => "Anything that can be placed will spawn incinerated";

    public override void Initialize()
    {
        _harmony.PatchAll(typeof(NoTracePatches));
    }

    public override void Cleanup()
    {
        _harmony.UnpatchSelf();
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal class NoTracePatches
{
    [HarmonyPatch(typeof(Item), "Start")]
    [HarmonyPostfix]
    private static void ItemStart(Item __instance)
    {
        var name = __instance.gameObject.name.ToLower();

        if (!name.Contains("rope") && !name.Contains("climbingspike") && !name.Contains("chain") &&
            !name.Contains("stove"))
            return;

        __instance.cooking.wreckWhenCooked = true;

        // A cook level of `5` is what the game uses so its what we'll use
        __instance.SetCookedAmountRPC(5);
        __instance.cooking.UpdateCookedBehavior();
    }
}