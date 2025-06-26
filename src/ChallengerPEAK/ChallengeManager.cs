using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Photon.Pun;

namespace ChallengerPEAK;

internal class ChallengeManager : MonoBehaviourPunCallbacks
{
    // These are all static because when we load into the game this will get destroyed and recreated, therefore clearing
    // all the instance fields
    private static string[] _enabledChallenges = [];

    internal static readonly List<Challenge> LoadedChallenges = [];
    internal static bool HasReceivedChallenges;
    internal static bool CanInitializeChallenges;
    internal static ChallengeManager? Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void SendChallenges(string[] challenges)
    {
        Plugin.Log.LogInfo($"Sending challenges: {string.Join(", ", challenges)}");
        photonView.RPC("SyncChallengesRPC", RpcTarget.All, (object)challenges);
    }

    public void SendChallenges(string[] challenges, Photon.Realtime.Player player)
    {
        Plugin.Log.LogInfo($"Sending challenges to {player.NickName}: {string.Join(", ", challenges)}");
        photonView.RPC("SyncChallengesRPC", player, (object)challenges);
    }


    [PunRPC]
    public void SyncChallengesRPC(string[] challenges)
    {
        if (challenges.Length == 0) return;
        if (HasReceivedChallenges)
        {
            Plugin.Log.LogInfo("Received challenges after first receive. Ignoring...");
            return;
        }

        Plugin.Log.LogInfo($"Received challenges: {string.Join(", ", challenges)}");

        _enabledChallenges = challenges;
        HasReceivedChallenges = true;

        if (CanInitializeChallenges) InitializeChallenges();
    }

    internal static void InitializeChallenges()
    {
        Plugin.Log.LogDebug("Initializing challenges");
        foreach (var challengeID in _enabledChallenges)
        {
            Plugin.Log.LogDebug($"Initializing challenge {challengeID}");
            var challenge = ChallengeRegister.RegisteredChallenges[challengeID];

            challenge.Initialize();
            LoadedChallenges.Add(challenge);
        }

        Plugin.Log.LogDebug("Initialized challenges");
    }

    internal static void CleanupChallenges()
    {
        Plugin.Log.LogDebug("Cleaning up challenges");
        foreach (var challenge in LoadedChallenges)
        {
            Plugin.Log.LogDebug($"Cleaning up challenge {challenge.ID}");
            challenge.Cleanup();
        }

        LoadedChallenges.Clear();
        _enabledChallenges = [];

        HasReceivedChallenges = false;
        CanInitializeChallenges = false;

        Plugin.Log.LogDebug("Cleaned up challenges");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        if (!PhotonNetwork.IsMasterClient)
            return;

        SendChallenges(_enabledChallenges, newPlayer);
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal partial class Patches
{
    [HarmonyPatch(typeof(GameUtils), "Awake")]
    [HarmonyPostfix]
    private static void AddChallengeManager(GameUtils __instance)
    {
        __instance.gameObject.AddComponent<ChallengeManager>();
    }

    [HarmonyPatch(typeof(MapHandler), "Start")]
    [HarmonyPostfix]
    private static void InitializeChallengesOnMapStart()
    {
        ChallengeManager.CanInitializeChallenges = true;
        Plugin.Log.LogDebug(
            $"MapHandler_StartPatch: CanInitializeChallenges={ChallengeManager.CanInitializeChallenges} | HasReceivedChallenges={ChallengeManager.HasReceivedChallenges}");

        if (ChallengeManager.HasReceivedChallenges) ChallengeManager.InitializeChallenges();
    }

    [HarmonyPatch(typeof(MapHandler), "OnDestroy")]
    [HarmonyPostfix]
    private static void CleanupChallengesOnMapDestroy()
    {
        ChallengeManager.CleanupChallenges();
    }
}