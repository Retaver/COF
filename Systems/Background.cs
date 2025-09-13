using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Background
{
    [Header("Basic Info")]
    public string id;
    public string name;
    public string description;
    public string flavorText;

    [Header("Starting Bonuses")]
    public Dictionary<StatType, int> statBonuses = new Dictionary<StatType, int>();
    public int startingBits = 0;
    public List<string> startingItems = new List<string>();
    public List<PerkType> startingPerks = new List<PerkType>();

    [Header("Personality Influences")]
    public Dictionary<string, int> personalityModifiers = new Dictionary<string, int>();
    public List<PersonalityTrait> suggestedTraits = new List<PersonalityTrait>();

    [Header("Social Standing")]
    public int startingReputation = 0;
    public List<string> socialConnections = new List<string>();
    public List<string> enemyFactions = new List<string>();

    [Header("Story Elements")]
    public List<string> storyFlags = new List<string>();
    public List<string> availableDialogue = new List<string>();
    public List<string> unlockedDialogueCategories = new List<string>();
    public List<string> backgroundFlags = new List<string>();
    public string startingLocation = "";

    [Header("Restrictions")]
    public List<RaceType> allowedRaces = new List<RaceType>();
    public List<RaceType> forbiddenRaces = new List<RaceType>();

    public Background()
    {
        InitializeLists();
    }

    public Background(string backgroundId, string backgroundName, string backgroundDescription)
    {
        id = backgroundId;
        name = backgroundName;
        description = backgroundDescription;
        InitializeLists();
    }

    private void InitializeLists()
    {
        if (statBonuses == null) statBonuses = new Dictionary<StatType, int>();
        if (startingItems == null) startingItems = new List<string>();
        if (startingPerks == null) startingPerks = new List<PerkType>();
        if (personalityModifiers == null) personalityModifiers = new Dictionary<string, int>();
        if (suggestedTraits == null) suggestedTraits = new List<PersonalityTrait>();
        if (socialConnections == null) socialConnections = new List<string>();
        if (enemyFactions == null) enemyFactions = new List<string>();
        if (storyFlags == null) storyFlags = new List<string>();
        if (availableDialogue == null) availableDialogue = new List<string>();
        if (unlockedDialogueCategories == null) unlockedDialogueCategories = new List<string>();
        if (backgroundFlags == null) backgroundFlags = new List<string>();
        if (allowedRaces == null) allowedRaces = new List<RaceType>();
        if (forbiddenRaces == null) forbiddenRaces = new List<RaceType>();
    }

    public bool IsAvailableForRace(RaceType race)
    {
        if (forbiddenRaces.Contains(race)) return false;
        if (allowedRaces.Count > 0 && !allowedRaces.Contains(race)) return false;
        return true;
    }

    public void AddStatBonus(StatType stat, int bonus)
    {
        if (statBonuses.ContainsKey(stat))
            statBonuses[stat] += bonus;
        else
            statBonuses[stat] = bonus;
    }

    public void AddStartingItem(string itemId)
    {
        if (!startingItems.Contains(itemId))
            startingItems.Add(itemId);
    }

    public void AddStartingPerk(PerkType perk)
    {
        if (!startingPerks.Contains(perk))
            startingPerks.Add(perk);
    }

    public void AddPersonalityModifier(string aspect, int modifier)
    {
        personalityModifiers[aspect] = modifier;
    }

    public void AddSocialConnection(string connection)
    {
        if (!socialConnections.Contains(connection))
            socialConnections.Add(connection);
    }

    public void AddEnemyFaction(string faction)
    {
        if (!enemyFactions.Contains(faction))
            enemyFactions.Add(faction);
    }

    public void AddStoryFlag(string flag)
    {
        if (!storyFlags.Contains(flag))
            storyFlags.Add(flag);
    }

    public void AddDialogueOption(string dialogue)
    {
        if (!availableDialogue.Contains(dialogue))
            availableDialogue.Add(dialogue);
    }

    public string GetRaceRestrictionText()
    {
        if (forbiddenRaces.Count > 0)
        {
            return $"Forbidden for: {string.Join(", ", forbiddenRaces)}";
        }

        if (allowedRaces.Count > 0)
        {
            return $"Available for: {string.Join(", ", allowedRaces)}";
        }

        return "Available for all races";
    }

    public string GetBonusesText()
    {
        var bonuses = new List<string>();

        foreach (var statBonus in statBonuses)
        {
            string sign = statBonus.Value > 0 ? "+" : "";
            bonuses.Add($"{sign}{statBonus.Value} {statBonus.Key}");
        }

        if (startingBits > 0)
            bonuses.Add($"+{startingBits} starting bits");

        if (startingPerks.Count > 0)
            bonuses.Add($"Starting perks: {string.Join(", ", startingPerks)}");

        if (startingItems.Count > 0)
            bonuses.Add($"Starting items: {string.Join(", ", startingItems)}");

        if (personalityModifiers.Count > 0)
        {
            var personalityText = new List<string>();
            foreach (var mod in personalityModifiers)
            {
                string sign = mod.Value > 0 ? "+" : "";
                personalityText.Add($"{sign}{mod.Value} {mod.Key}");
            }
            bonuses.Add($"Personality: {string.Join(", ", personalityText)}");
        }

        return bonuses.Count > 0 ? string.Join(", ", bonuses) : "No mechanical bonuses";
    }

    public Background Clone()
    {
        var clone = new Background
        {
            id = this.id,
            name = this.name,
            description = this.description,
            flavorText = this.flavorText,
            startingBits = this.startingBits,
            startingReputation = this.startingReputation,
            startingLocation = this.startingLocation
        };

        clone.statBonuses = new Dictionary<StatType, int>(this.statBonuses);
        clone.startingItems = new List<string>(this.startingItems);
        clone.startingPerks = new List<PerkType>(this.startingPerks);
        clone.personalityModifiers = new Dictionary<string, int>(this.personalityModifiers);
        clone.suggestedTraits = new List<PersonalityTrait>(this.suggestedTraits);
        clone.socialConnections = new List<string>(this.socialConnections);
        clone.enemyFactions = new List<string>(this.enemyFactions);
        clone.storyFlags = new List<string>(this.storyFlags);
        clone.availableDialogue = new List<string>(this.availableDialogue);
        clone.unlockedDialogueCategories = new List<string>(this.unlockedDialogueCategories);
        clone.backgroundFlags = new List<string>(this.backgroundFlags);
        clone.allowedRaces = new List<RaceType>(this.allowedRaces);
        clone.forbiddenRaces = new List<RaceType>(this.forbiddenRaces);

        return clone;
    }
}