using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ChallengerPEAK;

internal class ChallengePass : MonoBehaviour
{
    public GameObject? switchButton;
    public TextMeshProUGUI? switchButtonText;
    public GameObject? enableButton;
    public TextMeshProUGUI? enableButtonText;

    public BoardingPass? boardingPass;

    private readonly HashSet<string> _enabledChallenges = [];
    internal int SelectedChallengeIdx;

    internal bool ShowChallenges;

    private Challenge SelectedChallenge =>
        ChallengeRegister.RegisteredChallenges.Values.ElementAt(SelectedChallengeIdx);

    private void Start()
    {
        var panel = gameObject.transform.Find("BoardingPass").Find("Panel");

        panel.Find("StartGameButton").GetComponent<Button>().onClick.AddListener(StartButtonClicked);

        switchButton = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
        switchButtonText = switchButton.GetComponentInChildren<TextMeshProUGUI>();

        switchButton.transform.SetParent(panel.Find("Ascent"), false);
        switchButton.name = "SwitchButton";

        switchButtonText.text = "SHOW CHALLENGES";
        switchButton.AddComponent<AnimateButton>();

        switchButton.GetComponent<Button>().onClick.AddListener(SwitchButtonClicked);

        var switchButtonRectTransform = switchButton.GetComponent<RectTransform>();
        switchButtonRectTransform.anchorMin = new Vector2(0, 0);
        switchButtonRectTransform.anchorMax = new Vector2(0, 0);
        switchButtonRectTransform.anchoredPosition = new Vector2(25 + switchButtonRectTransform.sizeDelta.x / 2,
            25 + switchButtonRectTransform.sizeDelta.y / 2);


        enableButton = TMP_DefaultControls.CreateButton(new TMP_DefaultControls.Resources());
        enableButton.SetActive(false);
        enableButtonText = enableButton.GetComponentInChildren<TextMeshProUGUI>();

        enableButton.transform.SetParent(panel.Find("Ascent"), false);
        enableButton.name = "EnableChallengeButton";
        enableButtonText.text = "ENABLE";

        enableButton.AddComponent<AnimateButton>();
        enableButton.GetComponent<Button>().onClick.AddListener(EnableButtonClicked);

        var enableButtonRectTransform = enableButton.GetComponent<RectTransform>();
        enableButtonRectTransform.anchorMin = new Vector2(1, 0);
        enableButtonRectTransform.anchorMax = new Vector2(1, 0);
        enableButtonRectTransform.anchoredPosition = new Vector2(-25 - enableButtonRectTransform.sizeDelta.x / 2,
            25 + enableButtonRectTransform.sizeDelta.y / 2);
    }

    private void OnEnable()
    {
        if (ShowChallenges) UpdateChallenge();
    }


    private void UpdateAscent()
    {
        ReversePatches.BoardingPass_UpdateAscent(boardingPass!);
    }

    internal void UpdateChallenge()
    {
        boardingPass!.incrementAscentButton.interactable =
            SelectedChallengeIdx < ChallengeRegister.RegisteredChallenges.Count - 1;
        boardingPass.decrementAscentButton.interactable = SelectedChallengeIdx > 0;

        boardingPass.ascentTitle.text = SelectedChallenge.Title;
        boardingPass.ascentDesc.text = SelectedChallenge.Description;

        enableButtonText!.text = _enabledChallenges.Contains(SelectedChallenge.ID) ? "DISABLE" : "ENABLE";
    }

    private void SwitchButtonClicked()
    {
        ShowChallenges = !ShowChallenges;

        if (ShowChallenges)
        {
            enableButton!.SetActive(true);
            boardingPass!.reward.SetActive(false);
            UpdateChallenge();
            switchButtonText!.text = "SHOW ASCENTS";
        }
        else
        {
            enableButton!.SetActive(false);
            UpdateAscent();
            switchButtonText!.text = "SHOW CHALLENGES";
        }
    }

    private void EnableButtonClicked()
    {
        if (!_enabledChallenges.Add(SelectedChallenge.ID)) // Remove if already present
            _enabledChallenges.Remove(SelectedChallenge.ID);
        UpdateChallenge();
    }

    private void StartButtonClicked()

    {
        // var result = PhotonNetwork.RaiseEvent(Plugin.SyncChallengesEventCode, _enabledChallenges.ToArray(),
        //     new RaiseEventOptions { Receivers = ReceiverGroup.All, CachingOption = EventCaching.AddToRoomCacheGlobal }, SendOptions.SendReliable);

        ChallengeManager.Instance!.SendChallenges(_enabledChallenges.ToArray());
    }
}

internal class AnimateButton : MonoBehaviour
{
    private float _scale = 1f;

    private float _vel;

    private void Start()
    {
        GetComponent<Button>()?.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        _vel = FRILerp.Lerp(_vel, (1f - _scale) * 25f, 20f);
        _scale += _vel * Time.deltaTime;
        transform.localScale = Vector3.one * _scale;
    }

    private void OnEnable()
    {
        transform.localScale = Vector3.one;
        _scale = 1f;
        _vel = 0f;
    }

    private void OnClick()
    {
        _vel += 4.5f;
    }
}

internal class ReversePatches
{
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(BoardingPass), "UpdateAscent")]
    internal static void BoardingPass_UpdateAscent(object instance)
    {
        // Stub
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal partial class Patches
{
    [HarmonyPatch(typeof(BoardingPass), "Initialize")]
    [HarmonyPrefix]
    private static void AddChallengePassToBoardingPass(BoardingPass __instance)
    {
        var challengePass = __instance.gameObject.AddComponent<ChallengePass>();
        challengePass.boardingPass = __instance;

        Plugin.Log.LogDebug("ChallengePass initialized!");
    }

    [HarmonyPatch(typeof(BoardingPass), "IncrementAscent")]
    [HarmonyPrefix]
    private static bool InterceptIncrementAscentButton(BoardingPass __instance)
    {
        var challengePass = __instance.gameObject.GetComponent<ChallengePass>();

        if (!challengePass.ShowChallenges) return true;

        challengePass.SelectedChallengeIdx++;
        challengePass.UpdateChallenge();

        return false;
    }

    [HarmonyPatch(typeof(BoardingPass), "DecrementAscent")]
    [HarmonyPrefix]
    private static bool InterceptDecrementAscentButton(BoardingPass __instance)
    {
        var challengePass = __instance.gameObject.GetComponent<ChallengePass>();

        if (!challengePass.ShowChallenges) return true;

        challengePass.SelectedChallengeIdx--;
        challengePass.UpdateChallenge();

        return false;
    }
}