using System.Collections.Generic;

public static class SkillDefinitions
{
    // Call from SkillTree.CreateDefaultSkills():
    // allSkills.AddRange(SkillDefinitions.GetLoreSkills());
    public static List<SkillNode> GetLoreSkills()
    {
        var skills = new List<SkillNode>();

        // ========= EARTH PONY =========
        skills.Add(new SkillNode
        {
            id = "seasonal_harvest_techniques",
            name = "Seasonal Harvest Techniques",
            description = "Refined methods for managing demanding harvest cycles and yield reliability.",
            category = SkillCategory.Nature,
            maxRank = 3,
            skillPointCost = 1,
            allowedRaces = new() { RaceType.EarthPony },
            prerequisiteSkills = new() { "earth_connection" },
            statBonusPerRank = new() { { StatType.Constitution, 1 } },
            healthBonusPerRank = 5,
            specialEffects = new() { "Improved gathering outcomes in seasonal events" }
        });

        skills.Add(new SkillNode
        {
            id = "stonecraft_techniques",
            name = "Stonecraft Techniques",
            description = "Applied leverage, patience, and material sense for quarrying and extraction.",
            category = SkillCategory.Crafting,
            maxRank = 3,
            skillPointCost = 2,
            levelRequirement = 4,
            allowedRaces = new() { RaceType.EarthPony },
            prerequisiteSkills = new() { "earth_connection" },
            statRequirements = new() { { StatType.Constitution, 6 } },
            statBonusPerRank = new() { { StatType.Strength, 1 }, { StatType.Constitution, 1 } },
            specialEffects = new() { "Higher quality mineral finds; access to certain extraction checks" }
        });

        skills.Add(new SkillNode
        {
            id = "field_engineering",
            name = "Field Engineering",
            description = "Compact rigging, quick setup practices, and reliable field apparatus.",
            category = SkillCategory.Crafting,
            maxRank = 2,
            skillPointCost = 2,
            levelRequirement = 5,
            allowedRaces = new() { RaceType.EarthPony },
            statRequirements = new() { { StatType.Dexterity, 6 } },
            statBonusPerRank = new() { { StatType.Dexterity, 1 } },
            energyBonusPerRank = 5,
            specialEffects = new() { "Unlocks gadget crafting shortcuts; reduces setup time in events" }
        });

        skills.Add(new SkillNode
        {
            id = "enduring_work_ethic",
            name = "Enduring Work Ethic",
            description = "Sustained effort under load; steady recovery during extended tasks.",
            category = SkillCategory.Combat,
            maxRank = 3,
            skillPointCost = 1,
            allowedRaces = new() { RaceType.EarthPony },
            statBonusPerRank = new() { { StatType.Strength, 1 }, { StatType.Constitution, 1 } },
            healthBonusPerRank = 6
        });

        // ========= UNICORN =========
        skills.Add(new SkillNode
        {
            id = "textual_projection",
            name = "Textual Projection",
            description = "Arcane methods for entering and traversing written works during research.",
            category = SkillCategory.Magic,
            maxRank = 2,
            skillPointCost = 3,
            levelRequirement = 6,
            allowedRaces = new() { RaceType.Unicorn },
            prerequisiteSkills = new() { "advanced_magic" },
            statRequirements = new() { { StatType.Intelligence, 10 }, { StatType.Wisdom, 6 } },
            statBonusPerRank = new() { { StatType.Intelligence, 2 } },
            magicBonusPerRank = 6,
            specialEffects = new() { "Unique research routes; book-bound encounter options" }
        });

        skills.Add(new SkillNode
        {
            id = "defensive_wardcasting",
            name = "Defensive Wardcasting",
            description = "Practiced barrier formulation to protect allies and stabilize engagements.",
            category = SkillCategory.Magic,
            maxRank = 3,
            skillPointCost = 2,
            levelRequirement = 5,
            allowedRaces = new() { RaceType.Unicorn },
            prerequisiteSkills = new() { "advanced_magic" },
            statRequirements = new() { { StatType.Wisdom, 8 } },
            statBonusPerRank = new() { { StatType.Wisdom, 1 } },
            specialEffects = new() { "Unlocks ward options in encounters; improved magical defense checks" }
        });

        skills.Add(new SkillNode
        {
            id = "gem_prospecting",
            name = "Gem Prospecting",
            description = "Attunement to crystalline signatures for targeted resource location.",
            category = SkillCategory.Nature,
            maxRank = 2,
            skillPointCost = 1,
            allowedRaces = new() { RaceType.Unicorn },
            prerequisiteSkills = new() { "advanced_magic" },
            statRequirements = new() { { StatType.Intelligence, 8 } },
            statBonusPerRank = new() { { StatType.Wisdom, 1 } },
            specialEffects = new() { "Additional gem finds; navigation hints for crystal-laden areas" }
        });

        skills.Add(new SkillNode
        {
            id = "umbral_theory",
            name = "Umbral Theory",
            description = "Cautious study of shadow-aligned principles and their constraints.",
            category = SkillCategory.Magic,
            maxRank = 3,
            skillPointCost = 3,
            levelRequirement = 7,
            allowedRaces = new() { RaceType.Unicorn },
            prerequisiteSkills = new() { "advanced_magic" },
            statRequirements = new() { { StatType.Intelligence, 10 }, { StatType.Wisdom, 10 } },
            statBonusPerRank = new() { { StatType.Intelligence, 1 } },
            magicBonusPerRank = 5,
            specialEffects = new() { "Enables shadow-leaning spell choices; sets related story flags" }
        });

        // ========= PEGASUS =========
        skills.Add(new SkillNode
        {
            id = "cloudworks_certification",
            name = "Cloudworks Certification",
            description = "Structured training in cloud handling, moisture metrics, and safety protocols.",
            category = SkillCategory.Flight,
            maxRank = 3,
            skillPointCost = 1,
            allowedRaces = new() { RaceType.Pegasus },
            prerequisiteSkills = new() { "weather_control" },
            statBonusPerRank = new() { { StatType.Dexterity, 1 } },
            energyBonusPerRank = 4,
            specialEffects = new() { "Expanded weather options; faster manipulation during events" }
        });

        skills.Add(new SkillNode
        {
            id = "thunder_kick",
            name = "Thunder Kick",
            description = "Channelled discharge techniques for decisive aerial strikes.",
            category = SkillCategory.Combat,
            maxRank = 3,
            skillPointCost = 2,
            levelRequirement = 5,
            allowedRaces = new() { RaceType.Pegasus },
            prerequisiteSkills = new() { "weather_control" },
            statRequirements = new() { { StatType.Dexterity, 9 } },
            statBonusPerRank = new() { { StatType.Dexterity, 1 } },
            specialEffects = new() { "Unlocks charged strike options; situational bonuses in storms" }
        });

        skills.Add(new SkillNode
        {
            id = "aerobatics_discipline",
            name = "Aerobatics Discipline",
            description = "Formation routines, stamina management, and aerial control under pressure.",
            category = SkillCategory.Flight,
            maxRank = 3,
            skillPointCost = 2,
            levelRequirement = 6,
            allowedRaces = new() { RaceType.Pegasus },
            prerequisiteSkills = new() { "weather_control" },
            statRequirements = new() { { StatType.Dexterity, 10 } },
            statBonusPerRank = new() { { StatType.Dexterity, 1 }, { StatType.Charisma, 1 } },
            specialEffects = new() { "Group synergy bonuses; opens reputation lines with flight corps" }
        });

        skills.Add(new SkillNode
        {
            id = "transonic_protocols",
            name = "Transonic Protocols",
            description = "Advanced training focusing on high-speed thresholds and control envelopes.",
            category = SkillCategory.Flight,
            maxRank = 1,
            skillPointCost = 4,
            levelRequirement = 9,
            allowedRaces = new() { RaceType.Pegasus },
            prerequisiteSkills = new() { "sonic_flight" },
            statRequirements = new() { { StatType.Dexterity, 14 } },
            specialEffects = new() { "Limited-use burst maneuvers; opens specific story routes" }
        });

        // ========= UNIVERSAL / SOCIAL / LEADERSHIP =========
        skills.Add(new SkillNode
        {
            id = "diplomatic_mediation",
            name = "Diplomatic Mediation",
            description = "Measured dialogue, coalition building, and de-escalation practice.",
            category = SkillCategory.Social,
            maxRank = 3,
            skillPointCost = 1,
            statRequirements = new() { { StatType.Charisma, 6 } },
            statBonusPerRank = new() { { StatType.Charisma, 1 } },
            specialEffects = new() { "Unlocks nonviolent resolutions; improved negotiation checks" }
        });

        skills.Add(new SkillNode
        {
            id = "principles_of_harmony",
            name = "Principles of Harmony",
            description = "Applied ethics and coordination models for team effectiveness.",
            category = SkillCategory.Leadership,
            maxRank = 3,
            skillPointCost = 2,
            levelRequirement = 5,
            prerequisiteSkills = new() { "leadership" },
            statRequirements = new() { { StatType.Wisdom, 8 } },
            statBonusPerRank = new() { { StatType.Wisdom, 1 }, { StatType.Charisma, 1 } },
            specialEffects = new() { "Team synergy bonuses; additional cooperation prompts" }
        });

        skills.Add(new SkillNode
        {
            id = "talent_focus",
            name = "Talent Focus",
            description = "Intentional practice centered on a defined personal specialty.",
            category = SkillCategory.Crafting,
            maxRank = 2,
            skillPointCost = 2,
            levelRequirement = 4,
            statRequirements = new() { { StatType.Wisdom, 6 } },
            specialEffects = new() { "Reduced costs for niche skill paths; unlocks specialty checks" }
        });

        // ========= LORE HOOKS =========
        skills.Add(new SkillNode
        {
            id = "reflection_folklore",
            name = "Reflection Folklore",
            description = "Collected accounts of subterranean pools with unusual mimetic properties.",
            category = SkillCategory.Nature,
            maxRank = 1,
            skillPointCost = 2,
            levelRequirement = 6,
            statRequirements = new() { { StatType.Wisdom, 8 } },
            specialEffects = new() { "Sets exploration flags; enables rare investigation branches" }
        });

        skills.Add(new SkillNode
        {
            id = "crystalline_rituals",
            name = "Crystalline Rituals",
            description = "Ceremonial practices emphasizing resonance and social cohesion.",
            category = SkillCategory.Magic,
            maxRank = 2,
            skillPointCost = 2,
            levelRequirement = 6,
            statRequirements = new() { { StatType.Wisdom, 8 }, { StatType.Charisma, 8 } },
            magicBonusPerRank = 5,
            specialEffects = new() { "Resonance-based boons; social ritual options in specific locales" }
        });

        return skills;
    }
}