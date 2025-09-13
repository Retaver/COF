using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AttackDatabase", menuName = "Combat/Attack Database")]
public class AttackDatabase : ScriptableObject
{
    [System.Serializable]
    public class Attack
    {
        [Header("Basic Info")]
        public string id;
        public string displayName;
        public string description;

        [Header("Damage & Costs")]
        public int baseDamage;
        public int energyCost;
        public int magicCost;

        [Header("Combat Properties")]
        [Range(0f, 1f)]
        public float accuracy = 0.95f;
        [Range(0f, 1f)]
        public float critChance = 0.05f;
        public float critMultiplier = 1.5f;

        [Header("Restrictions")]
        public RaceType[] allowedRaces;
        public AttackType attackType;

        [Header("Status Effects")]
        public StatusEffectChance[] statusEffects;
    }

    [System.Serializable]
    public class StatusEffectChance
    {
        public StatusEffectType effectType;
        [Range(0f, 1f)]
        public float chance;
        public int duration;
        public int value; // damage per turn for poison, accuracy reduction for blind, etc.
    }

    public enum AttackType
    {
        Physical,
        Magical,
        Special
    }

    public enum StatusEffectType
    {
        Blind,
        Poison,
        Burn,
        Stun,
        Weakness,
        Shield,
        Regeneration
    }

    [Header("Attack Database")]
    public Attack[] attacks;

    // Quick lookup dictionary
    private Dictionary<string, Attack> attackLookup;

    void OnEnable()
    {
        BuildLookupDictionary();
    }

    void BuildLookupDictionary()
    {
        attackLookup = new Dictionary<string, Attack>();
        foreach (var attack in attacks)
        {
            if (!string.IsNullOrEmpty(attack.id))
            {
                attackLookup[attack.id] = attack;
            }
        }
    }

    public Attack GetAttack(string attackId)
    {
        if (attackLookup == null)
            BuildLookupDictionary();

        attackLookup.TryGetValue(attackId, out Attack attack);
        return attack;
    }

    public Attack[] GetAttacksForRace(RaceType race)
    {
        List<Attack> raceAttacks = new List<Attack>();

        foreach (var attack in attacks)
        {
            if (attack.allowedRaces == null || attack.allowedRaces.Length == 0)
            {
                raceAttacks.Add(attack); // Available to all races
            }
            else
            {
                foreach (var allowedRace in attack.allowedRaces)
                {
                    if (allowedRace == race)
                    {
                        raceAttacks.Add(attack);
                        break;
                    }
                }
            }
        }

        return raceAttacks.ToArray();
    }
}