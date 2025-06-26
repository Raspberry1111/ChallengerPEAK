using System.Collections.Generic;
using Photon.Pun;

namespace ChallengerPEAK;

public class ChallengeManager : MonoBehaviourPunCallbacks
{
    internal static ChallengeManager Instance { get; private set; }
    
    
    private static string[] EnabledChallenges = [];
    
    internal static readonly List<Challenge> LoadedChallenges = [];
    internal static bool HasReceivedChallenges = false;
    internal static bool CanInitializeChallenges = false;

    private void Awake()
    {
        Instance = this;
    }
    
    public void SendChallenges(string[] challenges)
    {
        Plugin.Log.LogInfo($"Sending challenges: {string.Join(", ", challenges)}" );
        photonView.RPC("ReceiveChallengesRPC", RpcTarget.All, (object)challenges);
    }
    
    public void SendChallenges(string[] challenges, Photon.Realtime.Player player)
    {
        Plugin.Log.LogInfo($"Sending challenges to {player.NickName}: {string.Join(", ", challenges)}" );
        photonView.RPC("ReceiveChallengesRPC", player, (object)challenges);
    }


    [PunRPC]
    public void ReceiveChallengesRPC(string[] challenges)
    {
        if (HasReceivedChallenges)
        {
            Plugin.Log.LogInfo("Received challenges after first receive. Ignoring...");
            return;
        }
    
        Plugin.Log.LogInfo($"Received challenges: {string.Join(", ", challenges)}" );
        
        EnabledChallenges = challenges;
        HasReceivedChallenges = true;

        if (CanInitializeChallenges)
        {
            InitializeChallenges();
        }
    }

    internal static void InitializeChallenges()
    {
        Plugin.Log.LogDebug("Initializing challenges");
        foreach (var challengeID in EnabledChallenges)
        {
            Plugin.Log.LogDebug($"Initializing challenge {challengeID}");
            var challenge = Plugin.RegisteredChallenges[challengeID];
            
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
        EnabledChallenges = [];

        HasReceivedChallenges = false;
        CanInitializeChallenges = false;
        
        Plugin.Log.LogDebug("Cleaned up challenges");
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        if (!PhotonNetwork.IsMasterClient)
            return;
        
        SendChallenges(EnabledChallenges, newPlayer);
    }
}