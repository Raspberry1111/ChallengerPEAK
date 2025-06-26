using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;

namespace ChallengerPEAK.Challenges;

public class OneFlare : Challenge
{
    // ReSharper disable once InconsistentNaming
    private const string _id = Plugin.Id + ".OneFlare";
    private readonly Harmony _harmony = new(_id);

    public override string ID => _id;
    public override string Title => "One Flare";

    public override string Description =>
        "No flares spawn in luggage\n<alpha=#CC><size=70%>Good luck on Ascent 4+";

    public override void Initialize()
    {
        _harmony.PatchAll(typeof(OneFlarePatches));
    }

    public override void Cleanup()
    {
        _harmony.UnpatchSelf();
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal class OneFlarePatches
{
    [HarmonyPatch(typeof(Spawner), "GetObjectsToSpawn")]
    [HarmonyPostfix]
    private static void ChangeSpawnedObjects(Spawner __instance, ref List<GameObject> __result)
    {
        if (__instance.spawnMode == Spawner.SpawnMode.SingleItem) return;

        __result.RemoveAll(gameObject => gameObject.name.ToLower().Contains("flare"));
    }
}