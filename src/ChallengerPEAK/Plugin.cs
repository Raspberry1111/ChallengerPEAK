using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using ChallengerPEAK.Challenges;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace ChallengerPEAK;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    public const byte SyncChallengesEventCode = 125;
    
    internal static ManualLogSource Log { get; private set; } = null!;
    
    internal static SortedDictionary<string, Challenge> _registeredChallenges = [];

    public static IReadOnlyDictionary<string, Challenge> RegisteredChallenges =>
        _registeredChallenges;
    
    /// <summary>
    /// Registers the challenge
    /// </summary>
    /// <param name="challenge">The challenge to register</param>
    /// <returns><see langword="false"/> if a challenge with the specified ID already is registered, otherwise <see langword="true"/></returns>
    public static bool RegisterChallenge(Challenge challenge)
    {
        return _registeredChallenges.TryAdd(challenge.ID, challenge);
    }
    
    private void Awake()
    {
        Log = Logger;
        
        Harmony.CreateAndPatchAll(typeof(Patches));

        RegisterChallenge(new FatalDamage());
        RegisterChallenge(new NoCooking());
        RegisterChallenge(new NoTrace());
        RegisterChallenge(new OneFlare());
        
        Log.LogInfo($"Plugin {Name} is loaded!");
    }
}

class Patches
{
    [HarmonyPatch(typeof(GameUtils), "Awake")]
    [HarmonyPostfix]
    private static void GameUtils_Awake_Patch(GameUtils __instance)
    {
        __instance.gameObject.AddComponent<ChallengeManager>();
    }
    
    [HarmonyPatch(typeof(BoardingPass), "Initialize")]
    [HarmonyPrefix]
    private static void BoardingPassInitializePatch(BoardingPass __instance)
    {
        ChallengePass challengePass = __instance.gameObject.AddComponent<ChallengePass>();
        challengePass.boardingPass = __instance;
        
        __instance.gameObject.AddComponent<PhotonView>().ObservedComponents = new List<Component> { challengePass };
        
        Plugin.Log.LogDebug("ChallengePass initialized!");
    }

    [HarmonyPatch(typeof(BoardingPass), "IncrementAscent")]
    [HarmonyPrefix]
    private static bool BoardingPassIncrementAscentPatch(BoardingPass __instance)
    {
        var challengePass = __instance.gameObject.GetComponent<ChallengePass>();

        if (!challengePass.ShowChallenges)
        {
            return true;
        }
        
        challengePass.selectedChallenge++;
        challengePass.UpdateChallenge();

        return false;
    }

    [HarmonyPatch(typeof(BoardingPass), "DecrementAscent")]
    [HarmonyPrefix]
    private static bool DecrementAscent(BoardingPass __instance)
    {
        var challengePass = __instance.gameObject.GetComponent<ChallengePass>();

        if (!challengePass.ShowChallenges)
        {
            return true;
        }

        challengePass.selectedChallenge--;
        challengePass.UpdateChallenge();

        return false;
    }

    [HarmonyPatch(typeof(MapHandler), "Start")]
    [HarmonyPostfix]
    private static void MapHandler_StartPatch()
    {
        ChallengeManager.CanInitializeChallenges = true;
        Plugin.Log.LogDebug($"MapHandler_StartPatch: CanInitializeChallenges={ChallengeManager.CanInitializeChallenges} | HasReceivedChallenges={ChallengeManager.HasReceivedChallenges}");

        if (ChallengeManager.HasReceivedChallenges)
        {
            ChallengeManager.InitializeChallenges();
        }
    }
    
    [HarmonyPatch(typeof(MapHandler), "OnDestroy")]
    [HarmonyPostfix]
    private static void CleanupChallenges()
    {
        ChallengeManager.CleanupChallenges();

        // if (PhotonNetwork.IsMasterClient)
        // {
        //     var result = PhotonNetwork.RaiseEvent(ChallengerPeakPlugin.SyncChallengesEventCode, null,
        //         new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.RemoveFromRoomCache }, SendOptions.SendReliable); // Clear old data from cache
        //     ChallengerPeakPlugin.Logger.LogInfo($"Removing old challenges from Photon cache: Succeeded = {result}");
        //
        // }
    }


    [HarmonyPatch(typeof(AscentUI), "Update")]
    [HarmonyPostfix]
    private static void UpdateAscentUI(AscentUI __instance)
    {
        foreach (var challenge in ChallengeManager.LoadedChallenges)
        {
            __instance.text.text += "\n" + challenge.Title;
        }
        __instance.text.text = __instance.text.text.TrimStart();
        
        var rect = __instance.text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-2, -rect.sizeDelta.y - 3);
    }

    
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(BoardingPass), "UpdateAscent")]
    internal static void BoardingPass_UpdateAscent(object instance)
    {
        throw new System();
    }
}