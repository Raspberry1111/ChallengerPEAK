using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using UnityEngine;

namespace ChallengerPEAK.Challenges;

public class RuleZero : Challenge
{
    // ReSharper disable once InconsistentNaming
    private const string _id = Plugin.Id + ".RuleZero";
    private readonly Harmony _harmony = new(_id);

    public override string ID => _id;
    public override string Title => "Rule 0";
    public override string Description => "Never abandon a friend in need!\n<alpha=#CC><size=70%>Myers spawns when someone dies";

    internal static Vector3? PlayerJustDiedPosition;

    public override void Initialize()
    {
        _harmony.PatchAll(typeof(RuleZeroPatches));
    }

    public override void Cleanup()
    {
        PlayerJustDiedPosition = null;
        _harmony.UnpatchSelf();
    }

    internal static Character? ClosestAlivePlayerTo(Vector3 position)
    {
        Character? closest = null;
        
        foreach (var character in Character.AllCharacters)
        {
            if (character.isBot || character.data.dead || character.data.fullyPassedOut) continue;
            if (closest == null || character.transform.position.sqrMagnitude < closest.transform.position.sqrMagnitude) 
                closest = character;
        }

        return closest;
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal class RuleZeroPatches
{
    [HarmonyPatch(typeof(Character), "RPCA_Die")]
    [HarmonyPrefix]
    private static void GetPlayerDeathPosition(Character __instance)
    {
        RuleZero.PlayerJustDiedPosition = __instance.gameObject.transform.position;
    }

    [HarmonyPatch(typeof(Scoutmaster), "Start")]
    [HarmonyPostfix]
    private static void ChangeScoutmasterAggroRange(Scoutmaster __instance)
    {
        __instance.maxAggroHeight = 600F;
    }
    
    [HarmonyPatch(typeof(Scoutmaster), "LookForTarget")]
    [HarmonyPostfix]
    private static void AddClosestPlayerAsTarget(Scoutmaster __instance)
    {
        if (Character.AllCharacters.Count <= 1 || RuleZero.PlayerJustDiedPosition == null) return;
        
        var closest = RuleZero.ClosestAlivePlayerTo(RuleZero.PlayerJustDiedPosition.Value);
        if (!closest)
            return;
            
        __instance.SetCurrentTarget(closest, 60F);
        __instance.gameObject.transform.position = RuleZero.PlayerJustDiedPosition.Value;
        
        RuleZero.PlayerJustDiedPosition = null;
    }
}