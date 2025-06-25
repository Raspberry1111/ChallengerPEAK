using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ChallengerPEAK.Challenges;

public class NoTrace : Challenge
{
    public override string ID => MyPluginInfo.PLUGIN_GUID + ".NoTrace";
    public override string Title => "Leave No Trace";
    public override string Description => "Anything that can be placed will spawn incinerated";
    
    private Harmony _harmony;

    public override void Initialize()
    {
        ChallengerPeakPlugin.Logger.LogInfo("Initializing No Trace");
        _harmony = new Harmony(ID);
        _harmony.PatchAll(typeof(NoTracePatches));
    }

    public override void Cleanup()
    {
        ChallengerPeakPlugin.Logger.LogInfo("Cleaning up No Trace");
        _harmony.UnpatchSelf();
    }
}

class NoTracePatches
{
    [HarmonyPatch(typeof(Item), "Start")]
    [HarmonyPostfix]
    static void ItemStart(Item __instance)
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