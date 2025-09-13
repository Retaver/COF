// PersonalityTrait.cs - Complete personality trait system (fixed to avoid duplicate enums)
using System.Collections.Generic;
using UnityEngine;
using MyGameNamespace;

[System.Serializable]
public class PersonalityTrait
{
    [Header("Basic Info")]
    public string id;
    public string name;
    public string description;

    [Header("Core Personality Data")]
    public PersonalityCategory category; // Uses enum from MissingEnums.cs
    public int intensity;

    [Header("Dialogue Options")]
    public List<string> unlockedDialogueOptions = new List<string>();
    public List<string> blockedDialogueOptions = new List<string>();

    [Header("Requirements")]
    public Dictionary<string, int> personalityRequirements = new Dictionary<string, int>();
    public List<string> prerequisiteTraits = new List<string>();
    public List<RaceType> allowedRaces = new List<RaceType>();
    public List<RaceType> forbiddenRaces = new List<RaceType>();

    [Header("Effects")]
    public Dictionary<StatType, int> statModifiers = new Dictionary<StatType, int>();
    public List<string> storyFlags = new List<string>();
    public List<string> dialogueOptions = new List<string>();
    public List<string> specialAbilities = new List<string>();

    [Header("Social Modifiers")]
    public float charismaModifier = 0f;
    public float intimidationBonus = 0f;
    public float persuasionBonus = 0f;
    public float deceptionBonus = 0f;

    [Header("Combat Modifiers")]
    public float damageModifier = 0f;
    public float defenseModifier = 0f;
    public float accuracyModifier = 0f;

    [Header("Misc Modifiers")]
    public int healthBonus = 0;
    public int energyBonus = 0;
    public int magicBonus = 0;
    public float experienceModifier = 0f;

    public PersonalityTrait()
    {
        personalityRequirements = new Dictionary<string, int>();
        prerequisiteTraits = new List<string>();
        allowedRaces = new List<RaceType>();
        forbiddenRaces = new List<RaceType>();
        statModifiers = new Dictionary<StatType, int>();
        storyFlags = new List<string>();
        dialogueOptions = new List<string>();
        specialAbilities = new List<string>();
        unlockedDialogueOptions = new List<string>();
        blockedDialogueOptions = new List<string>();
    }

    public PersonalityTrait(string traitId, string traitName, string traitDescription)
    {
        id = traitId;
        name = traitName;
        description = traitDescription;

        personalityRequirements = new Dictionary<string, int>();
        prerequisiteTraits = new List<string>();
        allowedRaces = new List<RaceType>();
        forbiddenRaces = new List<RaceType>();
        statModifiers = new Dictionary<StatType, int>();
        storyFlags = new List<string>();
        dialogueOptions = new List<string>();
        specialAbilities = new List<string>();
        unlockedDialogueOptions = new List<string>();
        blockedDialogueOptions = new List<string>();
    }

    public bool CanUnlock(PlayerCharacter character)
    {
        if (character == null) return false;

        if (forbiddenRaces.Contains(character.race)) return false;
        if (allowedRaces.Count > 0 && !allowedRaces.Contains(character.race)) return false;

        foreach (var req in personalityRequirements)
        {
            int score = character.GetPersonalityScore(req.Key);
            if (score < req.Value) return false;
        }

        foreach (var prereq in prerequisiteTraits)
        {
            if (!character.HasPersonalityTrait(prereq)) return false;
        }

        return true;
    }

    public void AddPersonalityRequirement(string aspect, int minValue) => personalityRequirements[aspect] = minValue;
    public void AddStatModifier(StatType stat, int modifier) => statModifiers[stat] = modifier;
    public void AddStoryFlag(string flag) { if (!storyFlags.Contains(flag)) storyFlags.Add(flag); }
    public void AddDialogueOption(string option) { if (!dialogueOptions.Contains(option)) dialogueOptions.Add(option); }
    public void AddUnlockedDialogueOption(string option) { if (!unlockedDialogueOptions.Contains(option)) unlockedDialogueOptions.Add(option); }
    public void AddBlockedDialogueOption(string option) { if (!blockedDialogueOptions.Contains(option)) blockedDialogueOptions.Add(option); }
    public void AddSpecialAbility(string ability) { if (!specialAbilities.Contains(ability)) specialAbilities.Add(ability); }

    public string GetRequirementsText()
    {
        var requirements = new List<string>();

        foreach (var req in personalityRequirements)
            requirements.Add($"{req.Key} {req.Value}+");

        if (prerequisiteTraits.Count > 0)
            requirements.Add($"Requires: {string.Join(", ", prerequisiteTraits)}");

        if (allowedRaces.Count > 0)
            requirements.Add($"Races: {string.Join(", ", allowedRaces)}");

        if (forbiddenRaces.Count > 0)
            requirements.Add($"Forbidden: {string.Join(", ", forbiddenRaces)}");

        return requirements.Count > 0 ? string.Join(", ", requirements) : "No requirements";
    }

    public string GetEffectsText()
    {
        var effects = new List<string>();

        foreach (var modifier in statModifiers)
        {
            string sign = modifier.Value > 0 ? "+" : "";
            effects.Add($"{sign}{modifier.Value} {modifier.Key}");
        }

        if (charismaModifier != 0f) effects.Add($"{(charismaModifier > 0 ? "+" : "")}{charismaModifier:P0} Charisma");
        if (damageModifier != 0f) effects.Add($"{(damageModifier > 0 ? "+" : "")}{damageModifier:P0} Damage");
        if (defenseModifier != 0f) effects.Add($"{(defenseModifier > 0 ? "+" : "")}{defenseModifier:P0} Defense");
        if (healthBonus != 0) effects.Add($"{(healthBonus > 0 ? "+" : "")}{healthBonus} Health");
        if (energyBonus != 0) effects.Add($"{(energyBonus > 0 ? "+" : "")}{energyBonus} Energy");
        if (magicBonus != 0) effects.Add($"{(magicBonus > 0 ? "+" : "")}{magicBonus} Magic");
        if (experienceModifier != 0f) effects.Add($"{(experienceModifier > 0 ? "+" : "")}{experienceModifier:P0} Experience");

        if (specialAbilities.Count > 0)
            effects.AddRange(specialAbilities);

        return effects.Count > 0 ? string.Join(", ", effects) : "No direct effects";
    }

    public PersonalityTrait Clone()
    {
        var clone = new PersonalityTrait
        {
            id = this.id,
            name = this.name,
            description = this.description,
            category = this.category,
            intensity = this.intensity,
            charismaModifier = this.charismaModifier,
            intimidationBonus = this.intimidationBonus,
            persuasionBonus = this.persuasionBonus,
            deceptionBonus = this.deceptionBonus,
            damageModifier = this.damageModifier,
            defenseModifier = this.defenseModifier,
            accuracyModifier = this.accuracyModifier,
            healthBonus = this.healthBonus,
            energyBonus = this.energyBonus,
            magicBonus = this.magicBonus,
            experienceModifier = this.experienceModifier
        };

        clone.personalityRequirements = new Dictionary<string, int>(this.personalityRequirements);
        clone.prerequisiteTraits = new List<string>(this.prerequisiteTraits);
        clone.allowedRaces = new List<RaceType>(this.allowedRaces);
        clone.forbiddenRaces = new List<RaceType>(this.forbiddenRaces);
        clone.statModifiers = new Dictionary<StatType, int>(this.statModifiers);
        clone.storyFlags = new List<string>(this.storyFlags);
        clone.dialogueOptions = new List<string>(this.dialogueOptions);
        clone.specialAbilities = new List<string>(this.specialAbilities);
        clone.unlockedDialogueOptions = new List<string>(this.unlockedDialogueOptions);
        clone.blockedDialogueOptions = new List<string>(this.blockedDialogueOptions);

        return clone;
    }
}

[System.Serializable]
public class PersonalityQuirk
{
    public string id;
    public string name;
    public string description;
    public bool isPositive = true;
    public float frequency = 0.1f;

    public PersonalityQuirk() { }

    public PersonalityQuirk(string quirkId, string quirkName, string quirkDescription, bool positive = true)
    {
        id = quirkId;
        name = quirkName;
        description = quirkDescription;
        isPositive = positive;
    }
}
