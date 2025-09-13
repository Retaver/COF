// Assets/Project/Scripts/Data/PersonalityProfile.cs
// Personality additions ONLY. No duplicate core fields or inheritance here.

using System.Collections.Generic;
using UnityEngine;

namespace MyGameNamespace
{
    public enum PersonalityArchetype { Balanced, Scholar, Warrior, Trickster, Leader }

    [System.Serializable]
    public class PersonalityProfile
    {
        [Header("Core Sliders (0–100)")]
        [Range(0, 100)] public int kindness = 50;
        [Range(0, 100)] public int confidence = 50;
        [Range(0, 100)] public int curiosity = 50;
        [Range(0, 100)] public int optimism = 50;
        [Range(0, 100)] public int honesty = 50;
        [Range(0, 100)] public int assertiveness = 50;

        [Header("Derived")]
        public PersonalityArchetype primaryArchetype = PersonalityArchetype.Balanced;

        public void ClampAll()
        {
            kindness = Mathf.Clamp(kindness, 0, 100);
            confidence = Mathf.Clamp(confidence, 0, 100);
            curiosity = Mathf.Clamp(curiosity, 0, 100);
            optimism = Mathf.Clamp(optimism, 0, 100);
            honesty = Mathf.Clamp(honesty, 0, 100);
            assertiveness = Mathf.Clamp(assertiveness, 0, 100);
        }
    }

    [System.Serializable]
    public class PersonalityState
    {
        public PersonalityProfile profile = new PersonalityProfile();

        // Optional lightweight buckets (0–100)
        public Dictionary<string, int> scores = new Dictionary<string, int>();

        // Trait IDs the character has
        public HashSet<string> traits = new HashSet<string>();
    }

    /// <summary>
    /// Partial PlayerCharacter: personality-only data.
    /// Do NOT repeat core fields/properties/methods here.
    /// </summary>
    public partial class PlayerCharacter
    {
        [Header("Personality System")]
        public PersonalityState personality = new PersonalityState();
        // No methods here — PlayerCharacter.cs already contains
        // helper methods used by other systems.
    }
}
