using HarmonyLib;

namespace ChallengerPEAK.Challenges;

public class NoCooking : Challenge
{
    public override string ID => MyPluginInfo.PLUGIN_GUID + ".NoCooking";
    public override string Title => "No Cooking";
    public override string Description => "Everything gets destroyed when cooked";
    
    private Harmony _harmony;

    public override void Initialize()
    {
        ChallengerPeakPlugin.Logger.LogInfo("Initializing No Cooking");
        _harmony = new Harmony(ID);
        _harmony.PatchAll(typeof(NoCookingPatches));
    }

    public override void Cleanup()
    {
        ChallengerPeakPlugin.Logger.LogInfo("Cleaning up No Cooking");
    }
}

class NoCookingPatches
{
    [HarmonyPatch(typeof(Item), "Start")]
    [HarmonyPostfix]
    static void ItemStart(Item __instance)
    {
        __instance.cooking.wreckWhenCooked = true;
    }
}