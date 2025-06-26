using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChallengerPEAK;

internal class ChallengePass: MonoBehaviour
{
    public GameObject switchButton;
    public TextMeshProUGUI switchButtonText;
    public GameObject enableButton; 
    public TextMeshProUGUI enableButtonText;
    
    public BoardingPass boardingPass;

    public HashSet<string> enabledChallenges = [];
    public int selectedChallenge = 0;

    internal bool ShowChallenges = false;

    public Challenge SelectedChallenge => Plugin._registeredChallenges.Values.ElementAt(selectedChallenge);
    
    
    private void UpdateAscent()
    {
        Patches.BoardingPass_UpdateAscent(boardingPass);
    }
    
    internal void UpdateChallenge()
    {
        boardingPass.incrementAscentButton.interactable = selectedChallenge < Plugin._registeredChallenges.Count - 1;
        boardingPass.decrementAscentButton.interactable =  selectedChallenge > 0;

        boardingPass.ascentTitle.text = SelectedChallenge.Title;
        boardingPass.ascentDesc.text = SelectedChallenge.Description;

        if (enabledChallenges.Contains(SelectedChallenge.ID))
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
        if (!enabledChallenges.Add(SelectedChallenge.ID)) // Remove if already present
        {
            enabledChallenges.Remove(SelectedChallenge.ID);
        }
        UpdateChallenge();
    }

    private void StartButtonClicked()

    {
        // var result = PhotonNetwork.RaiseEvent(Plugin.SyncChallengesEventCode, enabledChallenges.ToArray(),
        //     new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCacheGlobal }, SendOptions.SendReliable);

        ChallengeManager.Instance.SendChallenges(enabledChallenges.ToArray());
    }
    
    private void Start()
    {
        var panel = gameObject.transform.Find("BoardingPass").Find("Panel");

        panel.Find("StartGameButton").GetComponent<Button>().onClick.AddListener(StartButtonClicked);
        
        switchButton = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
        switchButtonText = switchButton.GetComponentInChildren<TextMeshProUGUI>();
        
        switchButton.transform.SetParent(panel.Find("Ascent"), worldPositionStays: false);
        switchButton.name = "SwitchButton";
        
        switchButtonText.text = "SHOW CHALLENGES";
        switchButton.AddComponent<AnimateButton>();
        
        switchButton.GetComponent<Button>().onClick.AddListener(SwitchButtonClicked);
        
        var switchButtonRectTransform = switchButton.GetComponent<RectTransform>();
        switchButtonRectTransform.anchorMin = new Vector2(0, 0);
        switchButtonRectTransform.anchorMax = new Vector2(0, 0);
        switchButtonRectTransform.anchoredPosition = new Vector2(25 + switchButtonRectTransform.sizeDelta.x / 2, 25 + switchButtonRectTransform.sizeDelta.y / 2);

        
        enableButton = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
        enableButton.SetActive(false);
        enableButtonText = enableButton.GetComponentInChildren<TextMeshProUGUI>();
        
        enableButton.transform.SetParent(panel.Find("Ascent"), worldPositionStays: false);
        enableButton.name = "EnableChallengeButton";
        enableButtonText.text = "ENABLE";
        
        enableButton.AddComponent<AnimateButton>();
        enableButton.GetComponent<Button>().onClick.AddListener(EnableButtonClicked);
        
        var enableButtonRectTransform = enableButton.GetComponent<RectTransform>();
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

public abstract class Challenge
{
    public abstract string ID { get; }
    public abstract string Title { get; }
    public abstract string Description { get; }
    
    public abstract void Initialize();
    public abstract void Cleanup();
}


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
