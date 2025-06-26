using System.Diagnostics.CodeAnalysis;
using HarmonyLib;

namespace ChallengerPEAK.Challenges;

public class NoCooking : Challenge
{
    // ReSharper disable once InconsistentNaming
    private const string _id = Plugin.Id + ".NoCooking";
    private readonly Harmony _harmony = new(_id);

    public override string ID => _id;
    public override string Title => "No Cooking";
    public override string Description => "Everything gets destroyed when cooked";

    public override void Initialize()
    {
        _harmony.PatchAll(typeof(NoCookingPatches));
    }

    public override void Cleanup()
    {
        _harmony.UnpatchSelf();
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal class NoCookingPatches
{
    [HarmonyPatch(typeof(Item), "Start")]
    [HarmonyPostfix]
    private static void ItemStart(Item __instance)
    {
        __instance.cooking.wreckWhenCooked = true;
    }
}