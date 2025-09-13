// Assets/Project/Scripts/Data/LevelSystem.cs
using UnityEngine;

[System.Serializable]
public class LevelSystem
{
    [Header("Level Data")]
    public int level = 1;
    public int experience = 0;
    public int experienceToNext = 1000;

    [Header("Available Points")]
    public int statPoints = 0;
    public int perkPoints = 0;
    public int skillPoints = 0;

    [Header("Level Configuration")]
    public int maxLevel = 50;
    public int baseExperienceRequired = 1000;
    public float experienceGrowthRate = 1.2f;

    public LevelSystem()
    {
        CalculateExperienceToNext();
    }

    public LevelSystem(int startingLevel)
    {
        level = Mathf.Max(1, startingLevel);
        CalculateExperienceToNext();
    }

    public bool CanLevelUp()
    {
        return level < maxLevel && experience >= experienceToNext;
    }

    public void LevelUp()
    {
        if (!CanLevelUp()) return;

        int oldLevel = level;
        level++;
        experience -= experienceToNext;

        // Award points on level up
        statPoints += GetStatPointsPerLevel();
        perkPoints += GetPerkPointsPerLevel();
        skillPoints += GetSkillPointsPerLevel();

        CalculateExperienceToNext();

        Debug.Log($"Leveled up! {oldLevel} -> {level}. " +
                  $"Awarded: {GetStatPointsPerLevel()} stat, {GetPerkPointsPerLevel()} perk, {GetSkillPointsPerLevel()} skill points");
    }

    private void CalculateExperienceToNext()
    {
        experienceToNext = CalculateExperienceRequired(level);
    }

    private int CalculateExperienceRequired(int forLevel)
    {
        if (forLevel >= maxLevel) return int.MaxValue;
        return Mathf.RoundToInt(baseExperienceRequired * Mathf.Pow(experienceGrowthRate, forLevel - 1));
    }

    public int GetTotalExperienceForLevel(int targetLevel)
    {
        int total = 0;
        for (int i = 1; i < targetLevel; i++)
            total += CalculateExperienceRequired(i);
        return total;
    }

    public float GetExperienceProgress()
    {
        if (experienceToNext <= 0) return 1f;
        return (float)experience / experienceToNext;
    }

    public int GetExperienceNeeded()
    {
        return Mathf.Max(0, experienceToNext - experience);
    }

    // Configurable point awards per level
    private int GetStatPointsPerLevel()
    {
        if (level <= 5) return 3;
        if (level <= 10) return 2;
        return 1;
    }

    private int GetPerkPointsPerLevel() => 1;

    private int GetSkillPointsPerLevel()
    {
        if (level <= 10) return 2;
        if (level <= 20) return 3;
        return 4;
    }

    // ----- Updated: auto-level here -----
    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        experience += amount;
        while (CanLevelUp()) LevelUp();
        Debug.Log($"Gained {amount} XP. Total: {experience}/{experienceToNext}");
    }

    public void SetLevel(int newLevel)
    {
        level = Mathf.Clamp(newLevel, 1, maxLevel);
        experience = 0;
        CalculateExperienceToNext();
        Debug.Log($"Level set to {level}");
    }

    public void ResetToLevel1()
    {
        level = 1;
        experience = 0;
        statPoints = 0;
        perkPoints = 0;
        skillPoints = 0;
        CalculateExperienceToNext();
        Debug.Log("Reset to level 1");
    }

    public string GetProgressString()
    {
        return $"Level {level} - {experience}/{experienceToNext} XP ({GetExperienceProgress():P0})";
    }

    public string GetPointsSummary()
    {
        return $"Available Points - Stat: {statPoints}, Perk: {perkPoints}, Skill: {skillPoints}";
    }

    // ----- New: compatibility shim some code calls -----
    public void RecomputeExperienceToNext() => CalculateExperienceToNext();

    public LevelSystem Clone()
    {
        return new LevelSystem
        {
            level = this.level,
            experience = this.experience,
            experienceToNext = this.experienceToNext,
            statPoints = this.statPoints,
            perkPoints = this.perkPoints,
            skillPoints = this.skillPoints,
            maxLevel = this.maxLevel,
            baseExperienceRequired = this.baseExperienceRequired,
            experienceGrowthRate = this.experienceGrowthRate
        };
    }
}
