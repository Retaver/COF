using System.Collections.Generic;
using UnityEngine;
using MyGameNamespace;

// Part of the RPG skill system. SkillCategory is defined in Enums.cs.

[System.Serializable]
public class SkillNode
{
    [Header("Basic Info")]
    public string id;
    public string name;
    public string description;
    public SkillCategory category; // Reference to the enum in Enums.cs

    [Header("Progression")]
    public int maxRank = 1;
    public int currentRank = 0;
    public int skillPointCost = 1;
    public int levelRequirement = 1;

    [Header("Prerequisites")]
    public List<string> prerequisiteSkills = new();
    public Dictionary<StatType, int> statRequirements = new();
    public List<RaceType> allowedRaces = new(); // Empty = all races

    [Header("Effects")]
    public Dictionary<StatType, int> statBonusPerRank = new();
    public int healthBonusPerRank = 0;
    public int energyBonusPerRank = 0;
    public int magicBonusPerRank = 0;
    public List<string> specialEffects = new();

    public bool IsMaxRank => currentRank >= maxRank;
    public bool IsUnlocked => currentRank > 0;
    public int NextRankCost => skillPointCost * (currentRank + 1);
    public int TotalInvested => skillPointCost * currentRank;

    public SkillNode()
    {
        statRequirements = new Dictionary<StatType, int>();
        statBonusPerRank = new Dictionary<StatType, int>();
        allowedRaces = new List<RaceType>();
        prerequisiteSkills = new List<string>();
        specialEffects = new List<string>();
    }

    public bool CanUpgrade(PlayerCharacter player, SkillTree skillTree)
    {
        if (IsMaxRank) return false;
        if (player.level < levelRequirement) return false;
        if (player.levelSystem.skillPoints < NextRankCost) return false;
        if (allowedRaces.Count > 0 && !allowedRaces.Contains(player.race)) return false;

        foreach (var statReq in statRequirements)
        {
            if (player.stats.GetTotalStat(statReq.Key) < statReq.Value)
                return false;
        }
        foreach (var prereqId in prerequisiteSkills)
        {
            var prereq = skillTree?.GetSkill(prereqId);
            if (prereq == null || !prereq.IsUnlocked)
                return false;
        }
        return true;
    }

    public void UpgradeSkill(PlayerCharacter player, SkillTree skillTree)
    {
        if (!CanUpgrade(player, skillTree)) return;

        player.levelSystem.skillPoints -= NextRankCost;
        currentRank++;

        foreach (var bonus in statBonusPerRank)
        {
            player.stats.bonusStats[bonus.Key] += bonus.Value;
        }
        player.gameStats.maxHealth += healthBonusPerRank;
        player.gameStats.maxEnergy += energyBonusPerRank;
        player.gameStats.maxMagic += magicBonusPerRank;
        player.CalculateAllStats();

        Debug.Log($"Upgraded {name} to rank {currentRank}");
    }
}