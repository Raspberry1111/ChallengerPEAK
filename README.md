# Challenger PEAK
Adds optional challenges that can be enabled to increase the difficulty of your runs. These are additive (meaning you can enable as many as you choose) and exist alongside the vanilla ascent system.

![Image of boarding pass showing challenge](https://i.imgur.com/CA9qj8r.png)
(Yes i know the UI isn't the greatest at the moment; we'll just call it "functional")


This mod <i>should</i> mostly work in multiplayer, but please do report any issues on the GitHub if possible. People joining halfway through a run will correctly receive the list of challenges to their game client

This mod <b>requires both</b> the client and server to have it. Technically, a server-side only challenge could be added, however, all the built-in challenges do require both sides to have it installed.

## Existing Challenges
Fatal Damage - All injury damage (from the Scout Master or Falling) is permanent

No Cooking - Everything gets destroyed when cooked

No Trace - Anything that can be placed will spawn incinerated

One Flare - The only flare spawns in the plane. Good luck on Ascent 4+

## Installation
### Automated
It is recommended to use [Gale Mod Manager](https://github.com/Kesomannen/gale) as it fully supports auto-downloading and running PEAK mods

Simply search for `ChallengerPEAK` and hit download
### Manual
- [Install BepInEx](https://thunderstore.io/package/bbepis/BepInExPack/#README:~:text=the%20BepInEx%20framework.-,To%20install%2C,-extract%20contents%20of)
- Unzip the mod into the `BepInEx/plugins` folder

## Developers
These challenges can (in theory, it hasn't been fully tested yet) by other mods

The `ChallengerPeakPlugin` class exposes a method called `registerChallenge` that accepts an instance of a type derived from the `Challenge` abstract class.

The only requirements to derive `Challenge` are
- An ID
- A name
- A description
- `Initialize` method - Called once the `MapHandler` is started (once the run loads into the island)
- `Cleanup` method - Called once the `MapHandler` is destroyed (the map is unloaded)

Do NOT register any `Harmony` matches before the `Initialize` method is called (that are specific to your challenge), and make sure to unpatch them in the `Cleanup` method

You can also look at the existing challenges in this mod's [github repo](https://github.com/Raspberry1111/ChallengerPEAK/tree/main/Challenges)

In the future, challenges might also be able to add badges that would appear at the end game infocard (These would not appear on the player's sash)