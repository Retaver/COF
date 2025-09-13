using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MLPDatabase : MonoBehaviour
{
    public static MLPDatabase Instance { get; private set; }

    private Dictionary<RaceType, Race> raceDatabase;
    private Dictionary<PerkType, Perk> perkDatabase;
    private bool isInitialized = false;

    public bool IsInitialized => isInitialized;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDatabase()
    {
        if (isInitialized) return;

        try
        {
            InitializeRaces();
            InitializePerks();
            isInitialized = true;
            Debug.Log("MLPDatabase initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize MLPDatabase: {e.Message}");
        }
    }

    private void InitializeRaces()
    {
        raceDatabase = new Dictionary<RaceType, Race>();

        // Earth Pony
        var earthPony = new Race(
            RaceType.EarthPony,
            "Earth Pony",
            "Earth ponies are the backbone of Equestria, known for their connection to nature and incredible physical strength.",
            "Your hooves are firmly planted on the ground, and you feel the earth's strength flowing through you."
        );
        earthPony.statBonuses[StatType.Strength] = 2;
        earthPony.statBonuses[StatType.Constitution] = 1;
        earthPony.racialAbilities.Add("Earth Pony Strength: +25% physical damage");
        earthPony.racialAbilities.Add("Nature's Gift: Can sense plant growth and soil quality");
        earthPony.startingPerks.Add(PerkType.EarthPonyStrength);
        raceDatabase[RaceType.EarthPony] = earthPony;

        // Unicorn
        var unicorn = new Race(
            RaceType.Unicorn,
            "Unicorn",
            "Unicorns are the magic users of Equestria, wielding powerful spells with their horns.",
            "Your horn tingles with magical energy, ready to shape reality with your will."
        );
        unicorn.statBonuses[StatType.Intelligence] = 3;
        unicorn.statBonuses[StatType.Wisdom] = 1;
        unicorn.racialAbilities.Add("Magic Horn: Can cast spells and use telekinesis");
        unicorn.racialAbilities.Add("Magical Sight: Can see magical auras and enchantments");
        unicorn.startingPerks.Add(PerkType.UnicornMagic);
        raceDatabase[RaceType.Unicorn] = unicorn;

        // Pegasus
        var pegasus = new Race(
            RaceType.Pegasus,
            "Pegasus",
            "Pegasi are the weather controllers and sky guardians of Equestria.",
            "Your wings spread wide, and you feel the call of the endless sky above."
        );
        pegasus.statBonuses[StatType.Dexterity] = 2;
        pegasus.statBonuses[StatType.Constitution] = 1;
        pegasus.racialAbilities.Add("Flight: Can fly and hover in the air");
        pegasus.racialAbilities.Add("Weather Control: Can manipulate clouds and weather");
        pegasus.startingPerks.Add(PerkType.PegasusWings);
        raceDatabase[RaceType.Pegasus] = pegasus;

        // Bat Pony
        var batPony = new Race(
            RaceType.BatPony,
            "Bat Pony",
            "Bat ponies are the night guard of Equestria, adapted for darkness and stealth.",
            "Your leathery wings and enhanced senses make you perfectly suited for the night."
        );
        batPony.statBonuses[StatType.Dexterity] = 1;
        batPony.statBonuses[StatType.Wisdom] = 2;
        batPony.racialAbilities.Add("Night Vision: Can see perfectly in darkness");
        batPony.racialAbilities.Add("Echolocation: Can navigate and detect objects using sound");
        batPony.startingPerks.Add(PerkType.BatPonyEcholocation);
        raceDatabase[RaceType.BatPony] = batPony;

        // Griffon
        var griffon = new Race(
            RaceType.Griffon,
            "Griffon",
            "Griffons are proud warriors with the body of a lion and wings of an eagle.",
            "Your talons grip the ground as your keen eyes survey your domain with predatory grace."
        );
        griffon.statBonuses[StatType.Strength] = 1;
        griffon.statBonuses[StatType.Dexterity] = 1;
        griffon.statBonuses[StatType.Charisma] = 1;
        griffon.racialAbilities.Add("Razor Talons: Natural weapons that deal extra damage");
        griffon.racialAbilities.Add("Keen Sight: Enhanced vision and perception");
        griffon.startingPerks.Add(PerkType.GriffonTalons);
        raceDatabase[RaceType.Griffon] = griffon;

        // Dragon
        var dragon = new Race(
            RaceType.Dragon,
            "Dragon",
            "Dragons are ancient, powerful beings with incredible magical and physical prowess.",
            "Scales shimmer with inner fire as your draconic heritage awakens ancient power within you."
        );
        dragon.statBonuses[StatType.Strength] = 2;
        dragon.statBonuses[StatType.Intelligence] = 2;
        dragon.statBonuses[StatType.Constitution] = 1;
        dragon.racialAbilities.Add("Dragon Breath: Can breathe magical fire");
        dragon.racialAbilities.Add("Scales: Natural armor providing damage resistance");
        dragon.startingPerks.Add(PerkType.DragonBreath);
        raceDatabase[RaceType.Dragon] = dragon;

        // Human
        var human = new Race(
            RaceType.Human,
            "Human",
            "Humans are adaptable beings from another world, bringing unique perspectives to Equestria.",
            "Though you lack the natural abilities of Equestria's natives, your adaptability is unmatched."
        );
        human.statBonuses[StatType.Charisma] = 1;
        human.statBonuses[StatType.Intelligence] = 1;
        human.statBonuses[StatType.Wisdom] = 1;
        human.racialAbilities.Add("Adaptability: Gains extra perk points as they level");
        human.racialAbilities.Add("Foreign Knowledge: Unique insights from another world");
        human.startingPerks.Add(PerkType.HumanVersatility);
        raceDatabase[RaceType.Human] = human;

        Debug.Log($"Initialized {raceDatabase.Count} races");
    }

    private void InitializePerks()
    {
        perkDatabase = new Dictionary<PerkType, Perk>();

        // ===== RACIAL STARTING PERKS (core) =====
        AddPerk(new Perk
        {
            perkType = PerkType.EarthPonyStrength,
            name = "Earth Pony Strength",
            description = "Your connection to the earth grants you exceptional physical power.",
            category = PerkCategory.Racial.ToString(),
            levelRequirement = 1,
            statBonuses = new Dictionary<StatType, int> { { StatType.Strength, 2 } },
            allowedRaces = new List<RaceType> { RaceType.EarthPony }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.UnicornMagic,
            name = "Unicorn Magic",
            description = "Your horn allows you to cast spells and manipulate magical energy.",
            category = PerkCategory.Racial.ToString(),
            levelRequirement = 1,
            statBonuses = new Dictionary<StatType, int> { { StatType.Intelligence, 2 } },
            allowedRaces = new List<RaceType> { RaceType.Unicorn }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.PegasusWings,
            name = "Pegasus Wings",
            description = "Your wings grant you the power of flight and weather manipulation.",
            category = PerkCategory.Racial.ToString(),
            levelRequirement = 1,
            statBonuses = new Dictionary<StatType, int> { { StatType.Dexterity, 2 } },
            allowedRaces = new List<RaceType> { RaceType.Pegasus }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.BatPonyEcholocation,
            name = "Echolocation",
            description = "Your enhanced hearing allows you to navigate in complete darkness.",
            category = PerkCategory.Racial.ToString(),
            levelRequirement = 1,
            statBonuses = new Dictionary<StatType, int> { { StatType.Wisdom, 1 } },
            allowedRaces = new List<RaceType> { RaceType.BatPony }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.GriffonTalons,
            name = "Griffon Talons",
            description = "Your sharp talons are deadly weapons in combat.",
            category = PerkCategory.Racial.ToString(),
            levelRequirement = 1,
            statBonuses = new Dictionary<StatType, int> { { StatType.Strength, 1 } },
            allowedRaces = new List<RaceType> { RaceType.Griffon }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.DragonBreath,
            name = "Dragon Breath",
            description = "You can breathe magical fire to devastate your enemies.",
            category = PerkCategory.Racial.ToString(),
            levelRequirement = 1,
            statBonuses = new Dictionary<StatType, int> { { StatType.Intelligence, 1 } },
            allowedRaces = new List<RaceType> { RaceType.Dragon }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.HumanVersatility,
            name = "Human Versatility",
            description = "Your adaptability allows you to learn new skills more quickly.",
            category = PerkCategory.Racial.ToString(),
            levelRequirement = 1,
            statBonuses = new Dictionary<StatType, int> { { StatType.Charisma, 1 } },
            allowedRaces = new List<RaceType> { RaceType.Human }
        });

        // ===== UNIVERSAL PERKS =====
        AddPerk(new Perk
        {
            perkType = PerkType.Tough,
            name = "Tough",
            description = "You're naturally resilient and hardy. Increases Constitution.",
            category = PerkCategory.Physical.ToString(),
            levelRequirement = 1,
            statBonuses = new Dictionary<StatType, int> { { StatType.Constitution, 1 } },
            allowedRaces = new List<RaceType>() // empty = all races
        });

        AddPerk(new Perk
        {
            perkType = PerkType.StrongBack,
            name = "Strong Back",
            description = "You can carry more weight without getting tired. Increases carrying capacity.",
            category = PerkCategory.Physical.ToString(),
            levelRequirement = 2,
            statRequirements = new Dictionary<StatType, int> { { StatType.Strength, 6 } },
            allowedRaces = new List<RaceType>()
        });

        AddPerk(new Perk
        {
            perkType = PerkType.Charming,
            name = "Charming",
            description = "Your natural charisma makes others more likely to trust you. Increases Charisma.",
            category = PerkCategory.Social.ToString(),
            levelRequirement = 1,
            statBonuses = new Dictionary<StatType, int> { { StatType.Charisma, 2 } },
            allowedRaces = new List<RaceType>()
        });

        AddPerk(new Perk
        {
            perkType = PerkType.Hardy,
            name = "Hardy",
            description = "Your body recovers from damage more quickly.",
            category = PerkCategory.Physical.ToString(),
            levelRequirement = 3,
            statRequirements = new Dictionary<StatType, int> { { StatType.Constitution, 7 } },
            allowedRaces = new List<RaceType>()
        });

        AddPerk(new Perk
        {
            perkType = PerkType.FastLearner,
            name = "Fast Learner",
            description = "You learn new skills and abilities more quickly. Increases experience gain.",
            category = PerkCategory.Mental.ToString(),
            levelRequirement = 2,
            statRequirements = new Dictionary<StatType, int> { { StatType.Intelligence, 6 } },
            allowedRaces = new List<RaceType>()
        });

        // ===== UNICORN-ONLY (expanded) =====
        AddPerk(new Perk
        {
            perkType = PerkType.MagicalAptitude,
            name = "Magical Aptitude",
            description = "Your horn glows brighter with increased magical power.",
            category = PerkCategory.Magic.ToString(),
            levelRequirement = 2,
            statBonuses = new Dictionary<StatType, int> { { StatType.Intelligence, 2 } },
            allowedRaces = new List<RaceType> { RaceType.Unicorn }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.TeleportMaster,
            name = "Teleport Master",
            description = "Mastered teleportation. Allows short-range blink and travel options.",
            category = PerkCategory.Magic.ToString(),
            levelRequirement = 5,
            statRequirements = new Dictionary<StatType, int> { { StatType.Intelligence, 8 } },
            prerequisites = new List<PerkType> { PerkType.MagicalAptitude },
            allowedRaces = new List<RaceType> { RaceType.Unicorn }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.MagicShield,
            name = "Magic Shield",
            description = "Create protective magical barriers around yourself and allies.",
            category = PerkCategory.Magic.ToString(),
            levelRequirement = 3,
            statRequirements = new Dictionary<StatType, int> { { StatType.Wisdom, 6 } },
            allowedRaces = new List<RaceType> { RaceType.Unicorn }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.Levitation,
            name = "Levitation",
            description = "Minor telekinetic levitation that improves mobility and item handling.",
            category = PerkCategory.Magic.ToString(),
            levelRequirement = 2,
            statBonuses = new Dictionary<StatType, int> { { StatType.Wisdom, 1 } },
            allowedRaces = new List<RaceType> { RaceType.Unicorn }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.SpellCaster,
            name = "Spell Caster",
            description = "Broadens your spell list and increases spell potency.",
            category = PerkCategory.Magic.ToString(),
            levelRequirement = 4,
            statRequirements = new Dictionary<StatType, int> { { StatType.Intelligence, 9 } },
            allowedRaces = new List<RaceType> { RaceType.Unicorn }
        });

        // ===== PEGASUS-ONLY (expanded) =====
        AddPerk(new Perk
        {
            perkType = PerkType.WeatherControl,
            name = "Weather Control",
            description = "Influence weather and clouds with ease.",
            category = PerkCategory.Flight.ToString(),
            levelRequirement = 3,
            statRequirements = new Dictionary<StatType, int> { { StatType.Dexterity, 7 } },
            allowedRaces = new List<RaceType> { RaceType.Pegasus }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.SonicRainboom,
            name = "Sonic Rainboom",
            description = "Break the sound barrier; unique combat/scene effects.",
            category = PerkCategory.Flight.ToString(),
            levelRequirement = 8,
            statRequirements = new Dictionary<StatType, int> { { StatType.Dexterity, 12 } },
            prerequisites = new List<PerkType> { PerkType.WeatherControl },
            allowedRaces = new List<RaceType> { RaceType.Pegasus }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.CloudWalking,
            name = "Cloud Walking",
            description = "Move across clouds and soft surfaces without sinking.",
            category = PerkCategory.Flight.ToString(),
            levelRequirement = 2,
            statBonuses = new Dictionary<StatType, int> { { StatType.Dexterity, 1 } },
            allowedRaces = new List<RaceType> { RaceType.Pegasus }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.AerialAce,
            name = "Aerial Ace",
            description = "Expert maneuvering and aerial combat techniques.",
            category = PerkCategory.Flight.ToString(),
            levelRequirement = 5,
            statRequirements = new Dictionary<StatType, int> { { StatType.Dexterity, 10 } },
            allowedRaces = new List<RaceType> { RaceType.Pegasus }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.StormMaster,
            name = "Storm Master",
            description = "Command storm-level weather phenomena in limited situations.",
            category = PerkCategory.Flight.ToString(),
            levelRequirement = 10,
            statRequirements = new Dictionary<StatType, int> { { StatType.Dexterity, 14 }, { StatType.Wisdom, 8 } },
            allowedRaces = new List<RaceType> { RaceType.Pegasus }
        });

        // ===== EARTH PONY-ONLY (expanded) =====
        AddPerk(new Perk
        {
            perkType = PerkType.EarthConnection,
            name = "Earth Connection",
            description = "Deep bond with the earth, grants bonuses when working land and resistance to certain effects.",
            category = PerkCategory.Nature.ToString(),
            levelRequirement = 2,
            statRequirements = new Dictionary<StatType, int> { { StatType.Constitution, 6 } },
            allowedRaces = new List<RaceType> { RaceType.EarthPony }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.SuperStrength,
            name = "Super Strength",
            description = "Exceptional physical power beyond typical ponies.",
            category = PerkCategory.Physical.ToString(),
            levelRequirement = 4,
            statRequirements = new Dictionary<StatType, int> { { StatType.Strength, 8 } },
            prerequisites = new List<PerkType> { PerkType.EarthConnection },
            statBonuses = new Dictionary<StatType, int> { { StatType.Strength, 3 } },
            allowedRaces = new List<RaceType> { RaceType.EarthPony }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.PlantWhisperer,
            name = "Plant Whisperer",
            description = "Communicate with plants and boost growth outcomes.",
            category = PerkCategory.Nature.ToString(),
            levelRequirement = 3,
            statBonuses = new Dictionary<StatType, int> { { StatType.Wisdom, 1 } },
            allowedRaces = new List<RaceType> { RaceType.EarthPony }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.IronHooves,
            name = "Iron Hooves",
            description = "Hooves hardened for heavy work and combat; increases melee damage and armor.",
            category = PerkCategory.Physical.ToString(),
            levelRequirement = 3,
            statBonuses = new Dictionary<StatType, int> { { StatType.Constitution, 1 }, { StatType.Strength, 1 } },
            allowedRaces = new List<RaceType> { RaceType.EarthPony }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.NaturalHealer,
            name = "Natural Healer",
            description = "Enhanced regenerative ability when resting on natural ground.",
            category = PerkCategory.Nature.ToString(),
            levelRequirement = 4,
            statBonuses = new Dictionary<StatType, int> { { StatType.Wisdom, 2 } },
            allowedRaces = new List<RaceType> { RaceType.EarthPony }
        });

        // ===== BAT PONY-ONLY (expanded) =====
        AddPerk(new Perk
        {
            perkType = PerkType.NightVision,
            name = "Night Vision",
            description = "See clearly in darkness.",
            category = PerkCategory.Racial.ToString(),
            levelRequirement = 1,
            statBonuses = new Dictionary<StatType, int> { { StatType.Wisdom, 1 } },
            allowedRaces = new List<RaceType> { RaceType.BatPony }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.SilentFlight,
            name = "Silent Flight",
            description = "Move through the air with near-silence, reducing detection.",
            category = PerkCategory.Flight.ToString(),
            levelRequirement = 2,
            allowedRaces = new List<RaceType> { RaceType.BatPony }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.EcholocationMaster,
            name = "Echolocation Master",
            description = "Mastered echolocation � pinpoint enemies and hidden objects.",
            category = PerkCategory.Racial.ToString(),
            levelRequirement = 4,
            prerequisites = new List<PerkType> { PerkType.BatPonyEcholocation },
            allowedRaces = new List<RaceType> { RaceType.BatPony }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.ShadowStep,
            name = "Shadow Step",
            description = "Move between shadows quickly for short-range teleportation in darkness.",
            category = PerkCategory.Magic.ToString(),
            levelRequirement = 5,
            allowedRaces = new List<RaceType> { RaceType.BatPony }
        });

        // ===== GRIFFON-ONLY =====
        AddPerk(new Perk
        {
            perkType = PerkType.TalonStrike,
            name = "Talon Strike",
            description = "Powerful talon attacks that deal extra damage.",
            category = PerkCategory.Combat.ToString(),
            levelRequirement = 2,
            statBonuses = new Dictionary<StatType, int> { { StatType.Strength, 1 } },
            allowedRaces = new List<RaceType> { RaceType.Griffon }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.KeenEye,
            name = "Keen Eye",
            description = "Exceptional perception, increases critical and ranged accuracy.",
            category = PerkCategory.Combat.ToString(),
            levelRequirement = 2,
            statBonuses = new Dictionary<StatType, int> { { StatType.Dexterity, 1 } },
            allowedRaces = new List<RaceType> { RaceType.Griffon }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.PredatorInstinct,
            name = "Predator Instinct",
            description = "Natural hunter; bonuses to tracking and combat initiation.",
            category = PerkCategory.Combat.ToString(),
            levelRequirement = 3,
            allowedRaces = new List<RaceType> { RaceType.Griffon }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.AerialCombat,
            name = "Aerial Combat",
            description = "Advanced aerial maneuvers and strikes.",
            category = PerkCategory.Flight.ToString(),
            levelRequirement = 4,
            allowedRaces = new List<RaceType> { RaceType.Griffon }
        });

        // ===== DRAGON-ONLY =====
        AddPerk(new Perk
        {
            perkType = PerkType.DragonScales,
            name = "Dragon Scales",
            description = "Natural scale armor increases damage resistance.",
            category = PerkCategory.Physical.ToString(),
            levelRequirement = 1,
            statBonuses = new Dictionary<StatType, int> { { StatType.Constitution, 2 } },
            allowedRaces = new List<RaceType> { RaceType.Dragon }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.FireBreath,
            name = "Fire Breath",
            description = "Breath weapon dealing area fire damage.",
            category = PerkCategory.Combat.ToString(),
            levelRequirement = 3,
            statBonuses = new Dictionary<StatType, int> { { StatType.Intelligence, 1 } },
            allowedRaces = new List<RaceType> { RaceType.Dragon }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.AncientWisdom,
            name = "Ancient Wisdom",
            description = "Centuries of knowledge increase magical potency and lore checks.",
            category = PerkCategory.Mental.ToString(),
            levelRequirement = 5,
            statBonuses = new Dictionary<StatType, int> { { StatType.Intelligence, 2 }, { StatType.Wisdom, 1 } },
            allowedRaces = new List<RaceType> { RaceType.Dragon }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.DragonFear,
            name = "Dragon Fear",
            description = "Your presence intimidates weaker foes, modifying morale checks.",
            category = PerkCategory.Social.ToString(),
            levelRequirement = 2,
            allowedRaces = new List<RaceType> { RaceType.Dragon }
        });

        // ===== HUMAN-ONLY =====
        AddPerk(new Perk
        {
            perkType = PerkType.Adaptability,
            name = "Adaptability",
            description = "Humans learn and adapt; gain small bonus to perk/skill point gains.",
            category = PerkCategory.Utility.ToString(),
            levelRequirement = 1,
            statBonuses = new Dictionary<StatType, int> { { StatType.Intelligence, 1 } },
            allowedRaces = new List<RaceType> { RaceType.Human }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.Innovation,
            name = "Innovation",
            description = "Novel solutions from another world; unlocks unique crafting options.",
            category = PerkCategory.Crafting.ToString(),
            levelRequirement = 3,
            statBonuses = new Dictionary<StatType, int> { { StatType.Intelligence, 1 }, { StatType.Charisma, 1 } },
            allowedRaces = new List<RaceType> { RaceType.Human }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.Diplomacy,
            name = "Diplomacy",
            description = "Increased success in negotiation and social resolution checks.",
            category = PerkCategory.Social.ToString(),
            levelRequirement = 2,
            statBonuses = new Dictionary<StatType, int> { { StatType.Charisma, 1 } },
            allowedRaces = new List<RaceType> { RaceType.Human }
        });

        AddPerk(new Perk
        {
            perkType = PerkType.QuickLearner,
            name = "Quick Learner",
            description = "Gain experience from study more efficiently.",
            category = PerkCategory.Mental.ToString(),
            levelRequirement = 2,
            statBonuses = new Dictionary<StatType, int> { { StatType.Intelligence, 1 } },
            allowedRaces = new List<RaceType> { RaceType.Human }
        });

        Debug.Log($"Initialized {perkDatabase.Count} perks with race restrictions");
    }

    private void AddPerk(Perk perk)
    {
        if (perk.statRequirements == null)
            perk.statRequirements = new Dictionary<StatType, int>();
        if (perk.prerequisites == null)
            perk.prerequisites = new List<PerkType>();
        if (perk.allowedRaces == null)
            perk.allowedRaces = new List<RaceType>();
        if (perk.forbiddenRaces == null)
            perk.forbiddenRaces = new List<RaceType>();
        if (perk.statBonuses == null)
            perk.statBonuses = new Dictionary<StatType, int>();
        if (perk.specialEffects == null)
            perk.specialEffects = new List<string>();

        perkDatabase[perk.perkType] = perk;
    }

    // Public access methods
    public Race GetRace(RaceType raceType)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("MLPDatabase not initialized yet");
            return null;
        }

        return raceDatabase.TryGetValue(raceType, out Race race) ? race : null;
    }

    public Perk GetPerk(PerkType perkType)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("MLPDatabase not initialized yet");
            return null;
        }

        return perkDatabase.TryGetValue(perkType, out Perk perk) ? perk : null;
    }

    public List<Race> GetAllRaces()
    {
        if (!isInitialized) return new List<Race>();
        return new List<Race>(raceDatabase.Values);
    }

    public List<Perk> GetAllPerks()
    {
        if (!isInitialized) return new List<Perk>();
        return new List<Perk>(perkDatabase.Values);
    }

    public List<Perk> GetPerksByCategory(PerkCategory category)
    {
        if (!isInitialized) return new List<Perk>();
        return perkDatabase.Values.Where(p => p.category == category.ToString()).ToList();
    }

    public List<Perk> GetPerksForRace(RaceType raceType)
    {
        if (!isInitialized) return new List<Perk>();
        return perkDatabase.Values.Where(p => p.IsAvailableForRace(raceType)).ToList();
    }

    // --- NEW HELPERS FOR STARTING PERKS ---
    // Returns the PerkType list that a Race declares as starting perks (may be empty)
    public List<PerkType> GetStartingPerkTypes(RaceType raceType)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("MLPDatabase not initialized yet (GetStartingPerkTypes)");
            return new List<PerkType>();
        }

        var race = GetRace(raceType);
        if (race == null)
        {
            Debug.LogWarning($"GetStartingPerkTypes: Race {raceType} not found");
            return new List<PerkType>();
        }

        // race.startingPerks assumed to be List<PerkType>
        return new List<PerkType>(race.startingPerks ?? new List<PerkType>());
    }

    // Returns resolved Perk objects for the race starting perks (skips missing perks)
    public List<Perk> GetStartingPerksForRace(RaceType raceType)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("MLPDatabase not initialized yet (GetStartingPerksForRace)");
            return new List<Perk>();
        }

        var types = GetStartingPerkTypes(raceType);
        var result = new List<Perk>();
        foreach (var t in types)
        {
            var p = GetPerk(t);
            if (p != null) result.Add(p);
            else Debug.LogWarning($"GetStartingPerksForRace: perk {t} not found in perkDatabase");
        }
        return result;
    }
    // --- end new helpers ---

    // Debug method
    [UnityEngine.ContextMenu("Debug Perk Database")]
    public void DebugPerkDatabase()
    {
        Debug.Log("=== PERK DATABASE DEBUG ===");
        foreach (var kvp in perkDatabase)
        {
            var perk = kvp.Value;
            Debug.Log($"Perk: {perk.name} | Type: {perk.perkType} | Race Restriction: {perk.GetRaceRestrictionText()} | LevelReq: {perk.levelRequirement}");
        }
    }
}