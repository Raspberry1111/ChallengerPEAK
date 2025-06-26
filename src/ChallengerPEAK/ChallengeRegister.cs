using System.Collections.Generic;

namespace ChallengerPEAK;

public static class ChallengeRegister
{
    // ReSharper disable once InconsistentNaming
    private static readonly SortedDictionary<string, Challenge> _registeredChallenges = [];

    public static IReadOnlyDictionary<string, Challenge> RegisteredChallenges =>
        _registeredChallenges;

    /// <summary>
    ///     Registers the challenge
    /// </summary>
    /// <param name="challenge">The challenge to register</param>
    /// <returns>
    ///     <see langword="false" /> if a challenge with the specified ID already is registered, otherwise
    ///     <see langword="true" />
    /// </returns>
    public static bool RegisterChallenge(Challenge challenge)
    {
        return _registeredChallenges.TryAdd(challenge.ID, challenge);
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