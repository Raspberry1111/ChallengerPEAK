using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ChallengerPEAK.Challenges;

public class OneFlare : Challenge
{
    public override string ID => MyPluginInfo.PLUGIN_GUID + ".OneFlare";
    public override string Title => "One Flare";

    public override string Description =>
        "The only flare spawns in the plane, the rest got eaten by Bing-Bong\n<alpha=#CC><size=70%>Good luck on Ascent 4+";
    
    private Harmony _harmony;

    public override void Initialize()
    {
        ChallengerPeakPlugin.Logger.LogInfo("Initializing One Flare");
        _harmony = new Harmony(ID);
        _harmony.PatchAll(typeof(OneFlarePatches));    }

    public override void Cleanup()
    {
        ChallengerPeakPlugin.Logger.LogInfo("Cleaning up One Flare");
        _harmony.UnpatchSelf();
    }
}

class OneFlarePatches
{
    [HarmonyPatch(typeof(Spawner), "GetObjectsToSpawn")]
    [HarmonyPostfix]
    public static void ChangeSpawnedObjects(ref List<GameObject> __result)
    {
        var newList = new List<GameObject>();

        foreach (var gameObject in __result)
        {
            var name = gameObject.name.ToLower();

            if (!name.Contains("flare"))
                 newList.Add(gameObject);                           
        }
        
        __result = newList;
    }
}