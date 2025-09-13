// ReputationSystem.cs
using System.Collections.Generic;
using UnityEngine;

public class ReputationSystem : MonoBehaviour
{
    [System.Serializable]
    public class Reputation
    {
        public string factionId;
        public string factionName;
        public int reputation = 0; // -100 to +100
        public ReputationLevel level = ReputationLevel.Neutral;

        public void ModifyReputation(int change)
        {
            reputation = Mathf.Clamp(reputation + change, -100, 100);
            level = CalculateLevel();
        }

        private ReputationLevel CalculateLevel()
        {
            return reputation switch
            {
                >= 80 => ReputationLevel.Revered,
                >= 60 => ReputationLevel.Honored,
                >= 20 => ReputationLevel.Friendly,
                >= -20 => ReputationLevel.Neutral,
                >= -60 => ReputationLevel.Unfriendly,
                >= -80 => ReputationLevel.Hostile,
                _ => ReputationLevel.Hated
            };
        }
    }

    public enum ReputationLevel
    {
        Hated, Hostile, Unfriendly, Neutral, Friendly, Honored, Revered
    }

    private Dictionary<string, Reputation> factionReps = new();

    public void ModifyReputation(string factionId, int change)
    {
        if (!factionReps.ContainsKey(factionId))
        {
            factionReps[factionId] = new Reputation { factionId = factionId };
        }

        factionReps[factionId].ModifyReputation(change);
        // Raise a game flag for the new reputation level.  The GameEventSystem
        // requires a value string; pass "true" by default for simple flags.
        GameEventSystem.Instance?.RaiseGameFlagSet($"rep_{factionId}_{factionReps[factionId].level}", "true");
    }
}