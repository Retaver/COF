// Equipment.cs - Complete equipment system
using System.Collections.Generic;
using UnityEngine;
using MyGameNamespace;

[System.Serializable]
public class Equipment
{
    [Header("Basic Info")]
    public string id;
    public string name;
    public string description;
    public EquipmentSlot slot;

    [Header("Requirements")]
    public int levelRequirement = 1;
    public List<RaceType> allowedRaces = new List<RaceType>();
    public List<RaceType> forbiddenRaces = new List<RaceType>();
    public Dictionary<StatType, int> statRequirements = new Dictionary<StatType, int>();

    [Header("Stat Bonuses")]
    public Dictionary<StatType, int> statBonuses = new Dictionary<StatType, int>();

    [Header("Derived Stat Bonuses")]
    public int healthBonus = 0;
    public int energyBonus = 0;
    public int magicBonus = 0;

    [Header("Combat Bonuses")]
    public int defenseBonus = 0;
    public float accuracyBonus = 0f;
    public float damageBonus = 0f;
    public float magicDamageBonus = 0f;

    [Header("Special Properties")]
    public List<string> specialEffects = new List<string>();
    public ItemQuality quality = ItemQuality.Common;
    public int durability = 100;
    public int maxDurability = 100;
    public int value = 0;

    public Equipment()
    {
        statBonuses = new Dictionary<StatType, int>();
        statRequirements = new Dictionary<StatType, int>();
        allowedRaces = new List<RaceType>();
        forbiddenRaces = new List<RaceType>();
        specialEffects = new List<string>();
    }

    public Equipment(string itemId, string itemName, EquipmentSlot equipSlot)
    {
        id = itemId;
        name = itemName;
        slot = equipSlot;
        statBonuses = new Dictionary<StatType, int>();
        statRequirements = new Dictionary<StatType, int>();
        allowedRaces = new List<RaceType>();
        forbiddenRaces = new List<RaceType>();
        specialEffects = new List<string>();
    }

    public bool CanEquip(PlayerCharacter character)
    {
        if (character == null) return false;

        // Level requirement
        if (character.level < levelRequirement)
        {
            Debug.Log($"Cannot equip {name}: Requires level {levelRequirement} (current: {character.level})");
            return false;
        }

        // Race restrictions
        if (forbiddenRaces.Contains(character.race))
        {
            Debug.Log($"Cannot equip {name}: Forbidden for race {character.race}");
            return false;
        }

        if (allowedRaces.Count > 0 && !allowedRaces.Contains(character.race))
        {
            Debug.Log($"Cannot equip {name}: Only allowed for races {string.Join(", ", allowedRaces)}");
            return false;
        }

        // Stat requirements
        foreach (var statReq in statRequirements)
        {
            if (character.stats.GetTotalStat(statReq.Key) < statReq.Value)
            {
                Debug.Log($"Cannot equip {name}: Requires {statReq.Value} {statReq.Key} (current: {character.stats.GetTotalStat(statReq.Key)})");
                return false;
            }
        }

        return true;
    }

    public void AddStatBonus(StatType stat, int bonus)
    {
        if (statBonuses.ContainsKey(stat))
            statBonuses[stat] += bonus;
        else
            statBonuses[stat] = bonus;
    }

    public void SetStatBonus(StatType stat, int bonus)
    {
        statBonuses[stat] = bonus;
    }

    public void AddStatRequirement(StatType stat, int requirement)
    {
        statRequirements[stat] = requirement;
    }

    public void AddSpecialEffect(string effect)
    {
        if (!specialEffects.Contains(effect))
            specialEffects.Add(effect);
    }

    public bool HasSpecialEffect(string effect)
    {
        return specialEffects.Contains(effect);
    }

    public void TakeDamage(int damage)
    {
        durability = Mathf.Max(0, durability - damage);
        if (durability <= 0)
        {
            Debug.Log($"{name} has broken!");
        }
    }

    public void Repair(int amount)
    {
        durability = Mathf.Min(maxDurability, durability + amount);
    }

    public float DurabilityPercentage => maxDurability > 0 ? (float)durability / maxDurability : 0f;
    public bool IsBroken => durability <= 0;
    public bool NeedsRepair => DurabilityPercentage < 0.25f;

    public string GetQualityColor()
    {
        return quality switch
        {
            ItemQuality.Poor => "#9D9D9D",        // Gray
            ItemQuality.Common => "#FFFFFF",      // White
            ItemQuality.Uncommon => "#1EFF00",    // Green
            ItemQuality.Rare => "#0070DD",        // Blue
            ItemQuality.Epic => "#A335EE",        // Purple
            ItemQuality.Legendary => "#FF8000",   // Orange
            ItemQuality.Artifact => "#E6CC80",    // Gold
            _ => "#FFFFFF"
        };
    }

    public string GetTooltipText()
    {
        var tooltip = new System.Text.StringBuilder();
        tooltip.AppendLine($"<color={GetQualityColor()}>{name}</color>");
        tooltip.AppendLine($"<i>{slot} - {quality}</i>");

        if (!string.IsNullOrEmpty(description))
        {
            tooltip.AppendLine();
            tooltip.AppendLine(description);
        }

        // Requirements
        if (levelRequirement > 1)
        {
            tooltip.AppendLine($"\n<color=#FFD700>Requires Level {levelRequirement}</color>");
        }

        if (statRequirements.Count > 0)
        {
            tooltip.AppendLine("<color=#FFD700>Stat Requirements:</color>");
            foreach (var req in statRequirements)
            {
                tooltip.AppendLine($"  {req.Key}: {req.Value}");
            }
        }

        // Bonuses
        if (statBonuses.Count > 0)
        {
            tooltip.AppendLine("\n<color=#00FF00>Stat Bonuses:</color>");
            foreach (var bonus in statBonuses)
            {
                tooltip.AppendLine($"  +{bonus.Value} {bonus.Key}");
            }
        }

        if (healthBonus != 0)
            tooltip.AppendLine($"<color=#00FF00>  {(healthBonus > 0 ? "+" : "")}{healthBonus} Health</color>");
        if (energyBonus != 0)
            tooltip.AppendLine($"<color=#00FF00>  {(energyBonus > 0 ? "+" : "")}{energyBonus} Energy</color>");
        if (magicBonus != 0)
            tooltip.AppendLine($"<color=#00FF00>  {(magicBonus > 0 ? "+" : "")}{magicBonus} Magic</color>");

        // Special effects
        if (specialEffects.Count > 0)
        {
            tooltip.AppendLine("\n<color=#FFD700>Special Effects:</color>");
            foreach (var effect in specialEffects)
            {
                tooltip.AppendLine($"  • {effect}");
            }
        }

        // Durability
        if (maxDurability > 0)
        {
            string durabilityColor = DurabilityPercentage > 0.5f ? "#00FF00" : (DurabilityPercentage > 0.25f ? "#FFFF00" : "#FF0000");
            tooltip.AppendLine($"\n<color={durabilityColor}>Durability: {durability}/{maxDurability}</color>");
        }

        // Value
        if (value > 0)
        {
            tooltip.AppendLine($"\n<color=#FFD700>Value: {value} bits</color>");
        }

        return tooltip.ToString();
    }

    public Equipment Clone()
    {
        var clone = new Equipment
        {
            id = this.id,
            name = this.name,
            description = this.description,
            slot = this.slot,
            levelRequirement = this.levelRequirement,
            healthBonus = this.healthBonus,
            energyBonus = this.energyBonus,
            magicBonus = this.magicBonus,
            defenseBonus = this.defenseBonus,
            accuracyBonus = this.accuracyBonus,
            damageBonus = this.damageBonus,
            magicDamageBonus = this.magicDamageBonus,
            quality = this.quality,
            durability = this.durability,
            maxDurability = this.maxDurability,
            value = this.value
        };

        clone.statBonuses = new Dictionary<StatType, int>(this.statBonuses);
        clone.statRequirements = new Dictionary<StatType, int>(this.statRequirements);
        clone.allowedRaces = new List<RaceType>(this.allowedRaces);
        clone.forbiddenRaces = new List<RaceType>(this.forbiddenRaces);
        clone.specialEffects = new List<string>(this.specialEffects);

        return clone;
    }
}