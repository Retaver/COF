// CharacterStats.cs - Complete character stats system
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum CharacterStat
{
    Health,
    Energy,
    Magic,
    Strength,
    Dexterity,
    Constitution,
    Intelligence,
    Wisdom,
    Charisma
}

[System.Serializable]
public class CharacterStats
{
    [Header("Core Values")]
    public int level = 1;
    public int experience = 0;

    [Header("Health / Vitals")]
    public int maxHealth = 100;
    public int health = 100;

    [Header("Base Stats")]
    public Dictionary<StatType, int> baseStats = new Dictionary<StatType, int>();

    [Header("Bonus Stats")]
    public Dictionary<StatType, int> bonusStats = new Dictionary<StatType, int>();

    [Header("Available Points")]
    public int availableStatPoints = 0;

    // Default constructor
    public CharacterStats()
    {
        Initialize();
    }

    // Constructor with maxHealth parameter (for compatibility)
    public CharacterStats(int maxHealth)
    {
        Initialize();
        this.maxHealth = Mathf.Max(1, maxHealth);
        this.health = this.maxHealth;
    }

    private void Initialize()
    {
        // Initialize base stats with default values
        foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
        {
            baseStats[stat] = 10; // Default value
            bonusStats[stat] = 0; // No bonus initially
        }

        availableStatPoints = 0;

        // Ensure health fields are valid
        if (maxHealth <= 0) maxHealth = 100;
        if (health <= 0) health = maxHealth;
    }

    public int GetTotalStat(StatType statType)
    {
        int baseValue = baseStats.GetValueOrDefault(statType, 0);
        int bonusValue = bonusStats.GetValueOrDefault(statType, 0);
        return baseValue + bonusValue;
    }

    public int GetBaseStat(StatType statType)
    {
        return baseStats.GetValueOrDefault(statType, 0);
    }

    public int GetBonusStat(StatType statType)
    {
        return bonusStats.GetValueOrDefault(statType, 0);
    }

    // New method for compatibility
    public int GetStat(CharacterStat stat)
    {
        return stat switch
        {
            CharacterStat.Health => health,
            CharacterStat.Energy => GetTotalStat(StatType.Constitution),
            CharacterStat.Magic => GetTotalStat(StatType.Intelligence),
            CharacterStat.Strength => GetTotalStat(StatType.Strength),
            CharacterStat.Dexterity => GetTotalStat(StatType.Dexterity),
            CharacterStat.Constitution => GetTotalStat(StatType.Constitution),
            CharacterStat.Intelligence => GetTotalStat(StatType.Intelligence),
            CharacterStat.Wisdom => GetTotalStat(StatType.Wisdom),
            CharacterStat.Charisma => GetTotalStat(StatType.Charisma),
            _ => 0
        };
    }

    public void SetBaseStat(StatType statType, int value)
    {
        baseStats[statType] = Mathf.Max(0, value);
    }

    public void AddBonusStat(StatType statType, int bonus)
    {
        if (!bonusStats.ContainsKey(statType)) bonusStats[statType] = 0;
        bonusStats[statType] += bonus;
    }

    public void SetBonusStat(StatType statType, int bonus)
    {
        bonusStats[statType] = bonus;
    }

    // Method for displaying stats in debug or UI
    public string GetStatsDisplay()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== CHARACTER STATS ===");

        sb.AppendLine($"Level: {level} | XP: {experience}");
        sb.AppendLine($"HP: {health}/{maxHealth}");

        foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
        {
            int total = GetTotalStat(stat);
            int baseVal = GetBaseStat(stat);
            int bonus = GetBonusStat(stat);

            sb.AppendLine($"{stat}: {total} (Base: {baseVal}, Bonus: {bonus})");
        }

        if (availableStatPoints > 0)
        {
            sb.AppendLine($"Available Points: {availableStatPoints}");
        }

        return sb.ToString();
    }

    // Calculate modifier for dice rolls (D&D style)
    public int GetStatModifier(StatType statType)
    {
        int statValue = GetTotalStat(statType);
        return (statValue - 10) / 2;
    }

    // Reset all stats to defaults
    public void ResetStats()
    {
        foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
        {
            baseStats[stat] = 10;
            bonusStats[stat] = 0;
        }
        availableStatPoints = 0;

        level = 1;
        experience = 0;
        maxHealth = 100;
        health = maxHealth;
    }

    // Combat helpers expected by other systems

    // Apply damage to this character's health, returns actual damage applied
    public int TakeDamage(int damage)
    {
        int applied = Mathf.Max(0, damage);
        int before = health;
        health = Mathf.Max(0, health - applied);
        return before - health;
    }

    // Heal the character, returns actual healed amount
    public int Heal(int amount)
    {
        int healed = Mathf.Max(0, amount);
        int before = health;
        health = Mathf.Min(maxHealth, health + healed);
        return health - before;
    }

    // Clone stats for save/load or copying
    public CharacterStats Clone()
    {
        var clone = new CharacterStats();

        clone.level = this.level;
        clone.experience = this.experience;

        clone.maxHealth = this.maxHealth;
        clone.health = this.health;

        foreach (var kvp in baseStats)
        {
            clone.baseStats[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in bonusStats)
        {
            clone.bonusStats[kvp.Key] = kvp.Value;
        }

        clone.availableStatPoints = this.availableStatPoints;

        return clone;
    }
}