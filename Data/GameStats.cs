// GameStats.cs - Complete game stats system
using UnityEngine;

[System.Serializable]
public class GameStats
{
    [Header("Health")]
    public int health = 30;
    public int maxHealth = 30;

    [Header("Energy")]
    public int energy = 30;
    public int maxEnergy = 30;

    [Header("Magic")]
    public int magic = 30;
    public int maxMagic = 30;

    [Header("Social")]
    public int friendship = 30;
    public int maxFriendship = 30;

    [Header("Corruption")]
    public int corruption = 0;
    public int maxCorruption = 100;

    public GameStats()
    {
        // Default constructor with default values
    }

    public GameStats(int maxHp, int maxEn, int maxMp, int maxFr, int maxCr = 100)
    {
        maxHealth = maxHp;
        health = maxHp;
        maxEnergy = maxEn;
        energy = maxEn;
        maxMagic = maxMp;
        magic = maxMp;
        maxFriendship = maxFr;
        friendship = maxFr;
        maxCorruption = maxCr;
        corruption = 0;
    }

    public void CalculateFromStats(CharacterStats stats)
    {
        if (stats == null) return;

        // Calculate max values from character stats
        maxHealth = 30 + (stats.GetTotalStat(StatType.Strength) * 2) + (stats.GetTotalStat(StatType.Constitution) * 3);
        maxEnergy = 30 + (stats.GetTotalStat(StatType.Constitution) * 2) + (stats.GetTotalStat(StatType.Dexterity) * 1);
        maxMagic = 30 + (stats.GetTotalStat(StatType.Intelligence) * 3) + (stats.GetTotalStat(StatType.Wisdom) * 1);
        maxFriendship = 30 + (stats.GetTotalStat(StatType.Charisma) * 2) + (stats.GetTotalStat(StatType.Wisdom) * 2);

        // Ensure current values don't exceed max
        health = Mathf.Min(health, maxHealth);
        energy = Mathf.Min(energy, maxEnergy);
        magic = Mathf.Min(magic, maxMagic);
        friendship = Mathf.Min(friendship, maxFriendship);
        corruption = Mathf.Clamp(corruption, 0, maxCorruption);
    }

    public bool IsAlive => health > 0;
    public bool IsDefeated => health <= 0;

    public float HealthPercentage => maxHealth > 0 ? (float)health / maxHealth : 0f;
    public float EnergyPercentage => maxEnergy > 0 ? (float)energy / maxEnergy : 0f;
    public float MagicPercentage => maxMagic > 0 ? (float)magic / maxMagic : 0f;
    public float FriendshipPercentage => maxFriendship > 0 ? (float)friendship / maxFriendship : 0f;
    public float CorruptionPercentage => maxCorruption > 0 ? (float)corruption / maxCorruption : 0f;

    public void RestoreAll()
    {
        health = maxHealth;
        energy = maxEnergy;
        magic = maxMagic;
        friendship = maxFriendship;
        corruption = 0;
    }

    public void ModifyHealth(int amount)
    {
        health = Mathf.Clamp(health + amount, 0, maxHealth);
    }

    public void ModifyEnergy(int amount)
    {
        energy = Mathf.Clamp(energy + amount, 0, maxEnergy);
    }

    public void ModifyMagic(int amount)
    {
        magic = Mathf.Clamp(magic + amount, 0, maxMagic);
    }

    public void ModifyFriendship(int amount)
    {
        friendship = Mathf.Clamp(friendship + amount, 0, maxFriendship);
    }

    public void ModifyCorruption(int amount)
    {
        corruption = Mathf.Clamp(corruption + amount, 0, maxCorruption);
    }

    public GameStats Clone()
    {
        return new GameStats
        {
            health = this.health,
            maxHealth = this.maxHealth,
            energy = this.energy,
            maxEnergy = this.maxEnergy,
            magic = this.magic,
            maxMagic = this.maxMagic,
            friendship = this.friendship,
            maxFriendship = this.maxFriendship,
            corruption = this.corruption,
            maxCorruption = this.maxCorruption
        };
    }

    public string GetStatusSummary()
    {
        return $"HP: {health}/{maxHealth} | EN: {energy}/{maxEnergy} | MP: {magic}/{maxMagic} | FR: {friendship}/{maxFriendship} | CR: {corruption}/{maxCorruption}";
    }
}