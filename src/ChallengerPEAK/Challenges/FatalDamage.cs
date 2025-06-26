using HarmonyLib;

namespace ChallengerPEAK.Challenges;

public class FatalDamage : Challenge
{
    public override string ID => Plugin.Id + ".FatalDamage";
    public override string Title => "Fatal Damage";
    public override string Description => "All injury damage (from the Scout Master or Falling) is permanent";

    private Harmony _harmony;
 
    public override void Initialize()
    {
        Plugin.Log.LogInfo("Initializing Fatal Damage");
        _harmony = new Harmony(ID);
        _harmony.PatchAll(typeof(FatalDamagePatches));

    }

    public override void Cleanup()
    {
        Plugin.Log.LogInfo("Cleaning up Fatal Damage");
        _harmony.UnpatchSelf();
    }
}

class FatalDamagePatches
{
    [HarmonyPatch(typeof(CharacterAfflictions), "AddStatus")]
    [HarmonyPrefix]
    public static void ChangeAfflictionTypes(ref CharacterAfflictions.STATUSTYPE statusType)
    { 
        if (statusType == CharacterAfflictions.STATUSTYPE.Injury)
        {
            statusType = CharacterAfflictions.STATUSTYPE.Curse;
        }
    }
}