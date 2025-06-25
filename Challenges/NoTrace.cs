using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ChallengerPEAK.Challenges;

public class NoTrace : Challenge
{
    public override string ID => MyPluginInfo.PLUGIN_GUID + ".NoTrace";
    public override string Title => "Leave No Trace";
    public override string Description => "Anything that can be placed on the mountain will no longer spawn\n<alpha=#CC><size=70%>And no, you won't get rerolls.";
    
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
    [HarmonyPatch(typeof(Spawner), "GetObjectsToSpawn")]
    [HarmonyPostfix]
    public static void ChangeSpawnedObjects(ref List<GameObject> __result)
    {
        var newList = new List<GameObject>();
        foreach (var gameObject in __result)
        {
            ChallengerPeakPlugin.Logger.LogInfo($"Intercepted item: {gameObject.name}");

            var name = gameObject.name.ToLower();
            if (!name.Contains("rope") && !name.Contains("climbingspike") && !name.Contains("chain") && !name.Contains("stove"))
                newList.Add(gameObject);
            else
                Object.Destroy(gameObject);
        }
        
        __result = newList;
    }
}