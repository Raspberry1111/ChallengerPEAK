using HarmonyLib;

namespace ChallengerPEAK.Challenges;

public class FatalDamage : Challenge
{
    // ReSharper disable once InconsistentNaming
    private const string _id = Plugin.Id + ".FatalDamage";
    private readonly Harmony _harmony = new(_id);

    public override string ID => _id;
    public override string Title => "Fatal Damage";
    public override string Description => "All injury damage (from the Scout Master or Falling) is permanent";

    public override void Initialize()
    {
        _harmony.PatchAll(typeof(FatalDamagePatches));
    }

    public override void Cleanup()
    {
        _harmony.UnpatchSelf();
    }
}

internal class FatalDamagePatches
{
    [HarmonyPatch(typeof(CharacterAfflictions), "AddStatus")]
    [HarmonyPrefix]
    private static void ChangeAfflictionTypes(ref CharacterAfflictions.STATUSTYPE statusType)
    {
        if (statusType == CharacterAfflictions.STATUSTYPE.Injury) statusType = CharacterAfflictions.STATUSTYPE.Curse;
    }
}