using System.Diagnostics.CodeAnalysis;
using BepInEx;
using BepInEx.Logging;
using ChallengerPEAK.Challenges;
using HarmonyLib;
using UnityEngine;

namespace ChallengerPEAK;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new(Id);
    internal static ManualLogSource Log { get; private set; } = null!;

    private void Awake()
    {
        Log = Logger;

        _harmony.PatchAll(typeof(Patches));
        _harmony.PatchAll(typeof(ReversePatches));

        ChallengeRegister.RegisterChallenge(new FatalDamage());
        ChallengeRegister.RegisterChallenge(new NoCooking());
        ChallengeRegister.RegisterChallenge(new NoTrace());
        ChallengeRegister.RegisterChallenge(new OneFlare());
        ChallengeRegister.RegisterChallenge(new NoBackpacks());

        Log.LogInfo($"Plugin {Name} is loaded!");
    }

    private void OnDestroy()
    {
        ChallengeManager.CleanupChallenges();
        _harmony.UnpatchSelf();
    }
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal partial class Patches
{
    [HarmonyPatch(typeof(AscentUI), "Update")]
    [HarmonyPostfix]
    private static void UpdateAscentUI(AscentUI __instance)
    {
        foreach (var challenge in ChallengeManager.LoadedChallenges) __instance.text.text += "\n" + challenge.Title;

        var rect = __instance.text.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(1, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.anchoredPosition = new Vector2(-2, 0);
        // rect.anchoredPosition = new Vector2(-2, -rect.sizeDelta.y / 2 - 3);
    }
}