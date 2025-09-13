// SkillTree.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyGameNamespace;

public class SkillTree : MonoBehaviour
{
    [Header("Skill Database")]
    [SerializeField] private List<SkillNode> allSkills = new();

    private Dictionary<string, SkillNode> skillLookup = new();
    private bool isInitialized = false;

    public bool IsInitialized => isInitialized;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (isInitialized) return;

        CreateDefaultSkills();
        BuildSkillLookup();
        isInitialized = true;

        Debug.Log($"SkillTree initialized with {allSkills.Count} skills");
    }

    private void CreateDefaultSkills()
    {
        allSkills.Clear();

        // ===== COMBAT SKILLS =====
        AddSkill(new SkillNode
        {
            id = "combat_mastery",
            name = "Combat Mastery",
            description = "Improves your overall combat effectiveness",
            category = SkillCategory.Combat,
            maxRank = 5,
            skillPointCost = 1,
            statBonusPerRank = new() { { StatType.Strength, 1 } }
        });

        AddSkill(new SkillNode
        {
            id = "iron_hide",
            name = "Iron Hide",
            description = "Toughens your body against physical damage",
            category = SkillCategory.Combat,
            maxRank = 3,
            skillPointCost = 2,
            levelRequirement = 3,
            prerequisiteSkills = new() { "combat_mastery" },
            healthBonusPerRank = 10,
            statBonusPerRank = new() { { StatType.Constitution, 1 } }
        });

        // ===== EARTH PONY SKILLS =====
        AddSkill(new SkillNode
        {
            id = "earth_connection",
            name = "Earth Connection",
            description = "Deepens your bond with the earth itself",
            category = SkillCategory.Nature,
            maxRank = 5,
            allowedRaces = new() { RaceType.EarthPony },
            statBonusPerRank = new() { { StatType.Strength, 1 }, { StatType.Constitution, 1 } }
        });

        AddSkill(new SkillNode
        {
            id = "apple_bucking",
            name = "Professional Apple Bucker",
            description = "Master the art of apple harvesting",
            category = SkillCategory.Nature,
            maxRank = 3,
            allowedRaces = new() { RaceType.EarthPony },
            prerequisiteSkills = new() { "earth_connection" },
            specialEffects = new() { "Increased harvest yields", "Faster farming" }
        });

        // ===== UNICORN SKILLS =====
        AddSkill(new SkillNode
        {
            id = "advanced_magic",
            name = "Advanced Magic",
            description = "Unlocks more powerful magical abilities",
            category = SkillCategory.Magic,
            maxRank = 5,
            allowedRaces = new() { RaceType.Unicorn },
            statRequirements = new() { { StatType.Intelligence, 6 } },
            statBonusPerRank = new() { { StatType.Intelligence, 2 } },
            magicBonusPerRank = 5
        });

        AddSkill(new SkillNode
        {
            id = "teleportation",
            name = "Teleportation Magic",
            description = "Master the art of magical teleportation",
            category = SkillCategory.Magic,
            maxRank = 3,
            skillPointCost = 3,
            levelRequirement = 5,
            allowedRaces = new() { RaceType.Unicorn },
            prerequisiteSkills = new() { "advanced_magic" },
            statRequirements = new() { { StatType.Intelligence, 10 } }
        });

        // ===== PEGASUS SKILLS =====
        AddSkill(new SkillNode
        {
            id = "weather_control",
            name = "Weather Manipulation",
            description = "Control clouds and weather patterns",
            category = SkillCategory.Flight,
            maxRank = 5,
            allowedRaces = new() { RaceType.Pegasus },
            statBonusPerRank = new() { { StatType.Dexterity, 1 } }
        });

        AddSkill(new SkillNode
        {
            id = "sonic_flight",
            name = "Sonic Flight",
            description = "Achieve incredible flight speeds",
            category = SkillCategory.Flight,
            maxRank = 3,
            skillPointCost = 4,
            levelRequirement = 7,
            allowedRaces = new() { RaceType.Pegasus },
            prerequisiteSkills = new() { "weather_control" },
            statRequirements = new() { { StatType.Dexterity, 12 } }
        });

        // ===== SOCIAL SKILLS =====
        AddSkill(new SkillNode
        {
            id = "leadership",
            name = "Natural Leader",
            description = "Your presence inspires others to follow",
            category = SkillCategory.Leadership,
            maxRank = 5,
            statBonusPerRank = new() { { StatType.Charisma, 1 } }
        });

        AddSkill(new SkillNode
        {
            id = "diplomacy",
            name = "Master Diplomat",
            description = "Resolve conflicts through words rather than force",
            category = SkillCategory.Social,
            maxRank = 3,
            skillPointCost = 2,
            prerequisiteSkills = new() { "leadership" },
            statRequirements = new() { { StatType.Charisma, 8 } }
        });

        // ===== APPEND LORE SKILLS =====
        // Requires SkillDefinitions.cs to be present in the project
        allSkills.AddRange(SkillDefinitions.GetLoreSkills());
    }

    private void AddSkill(SkillNode skill)
    {
        allSkills.Add(skill);
    }

    private void BuildSkillLookup()
    {
        skillLookup.Clear();
        foreach (var skill in allSkills)
        {
            skillLookup[skill.id] = skill;
        }
    }

    public SkillNode GetSkill(string skillId)
    {
        return skillLookup.TryGetValue(skillId, out var skill) ? skill : null;
    }

    public List<SkillNode> GetSkillsByCategory(SkillCategory category)
    {
        return allSkills.Where(s => s.category == category).ToList();
    }

    public List<SkillNode> GetAvailableSkills(PlayerCharacter player)
    {
        return allSkills.Where(s => s.CanUpgrade(player, this)).ToList();
    }

    public List<SkillNode> GetSkillsForRace(RaceType race)
    {
        return allSkills.Where(s => s.allowedRaces.Count == 0 || s.allowedRaces.Contains(race)).ToList();
    }
}