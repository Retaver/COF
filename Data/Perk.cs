using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Perk
{
    [Header("Basic Info")]
    public PerkType perkType;
    public string name;
    [TextArea(2, 4)]
    public string description;
    public string category;

    [Header("Requirements")]
    public int levelRequirement = 1;
    public Dictionary<StatType, int> statRequirements = new();
    public List<PerkType> prerequisites = new();

    [Header("Race Restrictions")]
    public List<RaceType> allowedRaces = new(); // Empty = all races allowed
    public List<RaceType> forbiddenRaces = new(); // Explicitly forbidden races

    [Header("Effects")]
    public Dictionary<StatType, int> statBonuses = new();
    public List<string> specialEffects = new();

    // Constructor
    public Perk()
    {
        statRequirements = new Dictionary<StatType, int>();
        prerequisites = new List<PerkType>();
        allowedRaces = new List<RaceType>();
        forbiddenRaces = new List<RaceType>();
        statBonuses = new Dictionary<StatType, int>();
        specialEffects = new List<string>();
    }

    // Constructor with parameters
    public Perk(PerkType type, string perkName, string desc, string cat)
    {
        perkType = type;
        name = perkName;
        description = desc;
        category = cat;

        statRequirements = new Dictionary<StatType, int>();
        prerequisites = new List<PerkType>();
        allowedRaces = new List<RaceType>();
        forbiddenRaces = new List<RaceType>();
        statBonuses = new Dictionary<StatType, int>();
        specialEffects = new List<string>();
    }

    // Check if this perk is available for a specific race
    public bool IsAvailableForRace(RaceType playerRace)
    {
        // If explicitly forbidden for this race
        if (forbiddenRaces.Contains(playerRace))
            return false;

        // If no allowed races specified, available for all (except forbidden)
        if (allowedRaces.Count == 0)
            return true;

        // Must be in allowed races list
        return allowedRaces.Contains(playerRace);
    }

    // Get race restriction display text
    public string GetRaceRestrictionText()
    {
        if (forbiddenRaces.Count > 0)
        {
            return $"Not available for: {string.Join(", ", forbiddenRaces)}";
        }

        if (allowedRaces.Count > 0)
        {
            return $"Only for: {string.Join(", ", allowedRaces)}";
        }

        return "Available for all races";
    }

    // Get formatted requirements text
    public string GetRequirementsText()
    {
        var requirements = new List<string>();

        if (levelRequirement > 1)
            requirements.Add($"Level {levelRequirement}");

        foreach (var stat in statRequirements)
            requirements.Add($"{stat.Key} {stat.Value}");

        if (prerequisites.Count > 0)
            requirements.Add($"Requires: {string.Join(", ", prerequisites)}");

        return requirements.Count > 0 ? string.Join(", ", requirements) : "No requirements";
    }

    // Get stat bonus text
    public string GetStatBonusText()
    {
        if (statBonuses.Count == 0) return "No stat bonuses";

        var bonuses = new List<string>();
        foreach (var bonus in statBonuses)
        {
            bonuses.Add($"+{bonus.Value} {bonus.Key}");
        }

        return string.Join(", ", bonuses);
    }
}