using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Logger = BepInEx.Logging.Logger;

namespace ChallengerPEAK;


[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class ChallengerPeakPlugin : BaseUnityPlugin, IOnEventCallback
{
    public const byte SyncChallengesEventCode = 125;
    

    internal new static ManualLogSource Logger;
    
    internal static readonly List<Challenge> Challenges = new List<Challenge>();
    internal static readonly List<Challenge> LoadedChallenges = new List<Challenge>();

    internal static bool HasReceivedChallenges = false;
    internal static bool CanInitializeChallenges = false;
    
    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

        Harmony.CreateAndPatchAll(typeof(Patches));
        
        RegisterChallenge(new Challenges.NoTrace() );
        RegisterChallenge(new Challenges.FatalDamage() );
        RegisterChallenge(new Challenges.NoCooking() );
        RegisterChallenge(new Challenges.OneFlare() );

        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    static public void RegisterChallenge(Challenge challenge)
    {
        Challenges.Add(challenge);
    }
    
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        Logger.LogDebug($"Received event code: {eventCode}");

        if (eventCode == SyncChallengesEventCode && photonEvent.CustomData != null && !HasReceivedChallenges)
        {

            var challenges = (string[])photonEvent.CustomData;
            Logger.LogInfo($"Receiving challenges over RPC: {string.Join(", ", challenges)}");

            LoadChallenges(challenges);
            HasReceivedChallenges = true;


            if (CanInitializeChallenges)
            {
                InitializeChallenges();
            }
            
        }
    }

    private static void LoadChallenges(string[] challengeIDs)
    {
        foreach (var challengeID in challengeIDs)
        {
            var challenge = Challenges.Find(challenge => challenge.ID == challengeID);
            LoadedChallenges.Add(challenge);
        }
    }

    internal static void InitializeChallenges()
    {
        foreach (var challenge in LoadedChallenges)
        {
            challenge.Initialize();
        }
    }

    internal static void CleanupChallenges()
    {
        foreach (var challenge in LoadedChallenges)
        {
            challenge.Cleanup();
        }
        LoadedChallenges.Clear();
    }
}

// Copied mostly from PEAK's ButtonHoverFeedback but with some changed settings
class AnimateButton : MonoBehaviour
{
    private float _scale = 1f;

    private float _vel;


    private void Start()
    {
        GetComponent<Button>()?.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        _vel += 4.5f;
    }

    private void OnEnable()
    {
        transform.localScale = Vector3.one;
        _scale = 1f;
        _vel = 0f;
    }
    private void Update()
    {
        _vel = FRILerp.Lerp(_vel, (1f - _scale) * 25f, 20f);
        _scale += _vel * Time.deltaTime;
        transform.localScale = Vector3.one * _scale;
    }
    
    
}

public abstract class Challenge
{
    public abstract string ID { get; }
    public abstract string Title { get; }
    public abstract string Description { get; }
    
    public abstract void Initialize();
    public abstract void Cleanup();
}

class ChallengePass: MonoBehaviour
{
    public GameObject switchButton;
    public TextMeshProUGUI switchButtonText;
    public GameObject enableButton; 
    public TextMeshProUGUI enableButtonText;
    
    static public HashSet<string> enabledChallenges = new HashSet<string>();
    public int selectedChallenge = 0;
    public BoardingPass boardingPass;

    internal bool ShowChallenges = false;
    
    /// <summary>
    ///  Calls update ascent on the boardingPass
    /// </summary>
    private void UpdateAscent()
    {
        typeof(BoardingPass).GetMethod("UpdateAscent", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(boardingPass, null);
    }

    public void UpdateChallenge()
    {
        boardingPass.incrementAscentButton.interactable = selectedChallenge < ChallengerPeakPlugin.Challenges.Count - 1;
        boardingPass.decrementAscentButton.interactable =  selectedChallenge > 0;

        boardingPass.ascentTitle.text = ChallengerPeakPlugin.Challenges[selectedChallenge].Title;
        boardingPass.ascentDesc.text = ChallengerPeakPlugin.Challenges[selectedChallenge].Description;

        if (enabledChallenges.Contains(ChallengerPeakPlugin.Challenges[selectedChallenge].ID))
        {
            enableButtonText.text = "DISABLE";
        }
        else
        {
            enableButtonText.text = "ENABLE";
        }
    }

    private void SwitchButtonClicked()
    {
        ShowChallenges = !ShowChallenges;

        if (ShowChallenges)
        {
            enableButton.SetActive(true);
            boardingPass.reward.SetActive(false);
            UpdateChallenge();
            switchButtonText.text = "SHOW ASCENTS";
        }
        else
        {
            enableButton.SetActive(false);
            UpdateAscent();
            switchButtonText.text = "SHOW CHALLENGES";
        }
    }

    private void EnableButtonClicked()
    {
        if (!enabledChallenges.Add(ChallengerPeakPlugin.Challenges[selectedChallenge].ID)) // Remove if already present
        {
            enabledChallenges.Remove(ChallengerPeakPlugin.Challenges[selectedChallenge].ID);
        }
        UpdateChallenge();
    }

    private void StartButtonClicked()

    {
        var result = PhotonNetwork.RaiseEvent(ChallengerPeakPlugin.SyncChallengesEventCode, enabledChallenges.ToArray(),
            new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCacheGlobal }, SendOptions.SendReliable);
        
        ChallengerPeakPlugin.Logger.LogInfo($"Sending challenges over RPC: {string.Join(", ", enabledChallenges)} | Succeeded = {result}");
    }
    
    private void Start()
    {
        Transform panel = gameObject.transform.Find("BoardingPass").Find("Panel");

        panel.Find("StartGameButton").GetComponent<Button>().onClick.AddListener(StartButtonClicked);
        
        switchButton = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
        switchButtonText = switchButton.GetComponentInChildren<TextMeshProUGUI>();
        
        switchButton.transform.SetParent(panel.Find("Ascent"), worldPositionStays: false);
        switchButton.SetName("SwitchButton");
        
        switchButtonText.text = "SHOW CHALLENGES";
        switchButton.AddComponent<AnimateButton>();
        
        switchButton.GetComponent<Button>().onClick.AddListener(SwitchButtonClicked);
        
        RectTransform switchButtonRectTransform = switchButton.GetComponent<RectTransform>();
        switchButtonRectTransform.anchorMin = new Vector2(0, 0);
        switchButtonRectTransform.anchorMax = new Vector2(0, 0);
        switchButtonRectTransform.anchoredPosition = new Vector2(25 + switchButtonRectTransform.sizeDelta.x / 2, 25 + switchButtonRectTransform.sizeDelta.y / 2);

        
        enableButton = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
        enableButton.SetActive(false);
        enableButtonText = enableButton.GetComponentInChildren<TextMeshProUGUI>();
        
        enableButton.transform.SetParent(panel.Find("Ascent"), worldPositionStays: false);
        enableButton.SetName("EnableChallengeButton");
        enableButtonText.text = "ENABLE";
        
        enableButton.AddComponent<AnimateButton>();
        enableButton.GetComponent<Button>().onClick.AddListener(EnableButtonClicked);
        
        RectTransform enableButtonRectTransform = enableButton.GetComponent<RectTransform>();
        enableButtonRectTransform.anchorMin = new Vector2(1, 0);
        enableButtonRectTransform.anchorMax = new Vector2(1, 0);
        enableButtonRectTransform.anchoredPosition = new Vector2(-25 - enableButtonRectTransform.sizeDelta.x / 2, 25 + enableButtonRectTransform.sizeDelta.y / 2);
    }

    private void OnEnable()
    {
        if (ShowChallenges)
        {
            UpdateChallenge();
        }
    }
}

class Patches
{
    [HarmonyPatch(typeof(BoardingPass), "Initialize")]
    [HarmonyPrefix]
    static void Initialize(BoardingPass __instance)
    {
        ChallengePass challengePass = __instance.gameObject.AddComponent<ChallengePass>();
        challengePass.boardingPass = __instance;
        
        ChallengerPeakPlugin.Logger.LogDebug("Challenge pass initialized!");
    }

    [HarmonyPatch(typeof(BoardingPass), "IncrementAscent")]
    [HarmonyPrefix]
    static bool IncrementAscent(BoardingPass __instance)
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
    static bool DecrementAscent(BoardingPass __instance)
    {
        var challengePass = __instance.gameObject.GetComponent<ChallengePass>();
        __instance.gameObject.AddComponent<PhotonView>().ObservedComponents = new List<Component> { challengePass };

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
    static void InitializeChallenges()
    {
        ChallengerPeakPlugin.CanInitializeChallenges = true;

        if (ChallengerPeakPlugin.HasReceivedChallenges)
        {
            ChallengerPeakPlugin.InitializeChallenges();
        }
    }
    
    [HarmonyPatch(typeof(MapHandler), "OnDestroy")]
    [HarmonyPostfix]
    static void CleanupChallenges()
    {
        ChallengerPeakPlugin.CleanupChallenges();
        ChallengerPeakPlugin.CanInitializeChallenges = false;
        ChallengerPeakPlugin.HasReceivedChallenges = false;

        if (PhotonNetwork.IsMasterClient)
        {
            var result = PhotonNetwork.RaiseEvent(ChallengerPeakPlugin.SyncChallengesEventCode, null,
                new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.RemoveFromRoomCache }, SendOptions.SendReliable); // Clear old data from cache
            ChallengerPeakPlugin.Logger.LogInfo($"Removing old challenges from Photon cache: Succeeded = {result}");

        }
    }


    [HarmonyPatch(typeof(AscentUI), "Update")]
    [HarmonyPostfix]
    static void UpdateAscentUI(AscentUI __instance)
    {
        foreach (var challenge in ChallengerPeakPlugin.LoadedChallenges)
        {
            __instance.text.text += "\n" + challenge.Title;
        }
        __instance.text.text = __instance.text.text.TrimStart();
        
        var rect = __instance.text.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-2, -rect.sizeDelta.y - 3);
    }
}