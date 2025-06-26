using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace ChallengerPEAK.Challenges;

public class NoBackpacks : Challenge
{
    // ReSharper disable once InconsistentNaming
    private const string _id = Plugin.Id + ".NoBackpacks";
    private readonly Harmony _harmony = new(_id);

    public override string ID => _id;
    public override string Title => "No Backpacks";
    public override string Description => "No backpacks will spawn";

    public override void Initialize()
    {
        _harmony.PatchAll(typeof(NoBackpacksPatches));
    }

    public override void Cleanup()
    {
        _harmony.UnpatchSelf();
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal class NoBackpacksPatches
{
    [HarmonyPatch(typeof(Backpack), "Update")]
    [HarmonyPrefix]
    private static bool BackpackUpdatePatch(Backpack __instance)
    {
        Object.Destroy(__instance.gameObject);
        return true;
    }
}