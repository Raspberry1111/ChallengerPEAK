using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace ChallengerPEAK.Challenges;

public class OneFlare : Challenge
{
    public override string ID => Plugin.Id + ".OneFlare";
    public override string Title => "One Flare";

    public override string Description =>
        "The only flare spawns in the plane, the rest got eaten by Bing-Bong\n<alpha=#CC><size=70%>Good luck on Ascent 4+";
    
    private Harmony _harmony;

    public override void Initialize()
    {
        Plugin.Log.LogInfo("Initializing One Flare");
        _harmony = new Harmony(ID);
        _harmony.PatchAll(typeof(OneFlarePatches));    }

    public override void Cleanup()
    {
        Plugin.Log.LogInfo("Cleaning up One Flare");
        _harmony.UnpatchSelf();
    }
}

class OneFlarePatches
{
    [HarmonyPatch(typeof(Spawner), "GetObjectsToSpawn")]
    [HarmonyPostfix]
    public static void ChangeSpawnedObjects(Spawner __instance, ref List<GameObject> __result)
    {
        if (__instance.spawnMode == Spawner.SpawnMode.SingleItem)
        {
            return;
        }
        
        __result.RemoveAll(gameObject => gameObject.name.ToLower().Contains("flare"));
    }
}