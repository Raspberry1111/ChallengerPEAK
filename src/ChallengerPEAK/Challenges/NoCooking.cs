using HarmonyLib;

namespace ChallengerPEAK.Challenges;

public class NoCooking : Challenge
{
    public override string ID => Plugin.Id + ".NoCooking";
    public override string Title => "No Cooking";
    public override string Description => "Everything gets destroyed when cooked";
    
    private Harmony _harmony;

    public override void Initialize()
    {
        Plugin.Log.LogInfo("Initializing No Cooking");
        _harmony = new Harmony(ID);
        _harmony.PatchAll(typeof(NoCookingPatches));
    }

    public override void Cleanup()
    {
        Plugin.Log.LogInfo("Cleaning up No Cooking");
    }
}

internal class NoCookingPatches
{
    [HarmonyPatch(typeof(Item), "Start")]
    [HarmonyPostfix]
    private static void ItemStart(Item __instance)
    {
        __instance.cooking.wreckWhenCooked = true;
    }
}