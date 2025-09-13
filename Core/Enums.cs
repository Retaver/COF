// Consolidated Enums for the project.
// Replace your existing Enums.cs with this file (or place it in the same folder),
// and delete any other files that declare the same enums (e.g., MissingEnums.cs).

public enum RaceType
{
    EarthPony,
    Unicorn,
    Pegasus,
    BatPony,
    Griffon,
    Dragon,
    Human
}

public enum PerkType
{
    // Racial Starting Perks
    EarthPonyStrength,
    UnicornMagic,
    PegasusWings,
    BatPonyEcholocation,
    GriffonTalons,
    DragonBreath,
    HumanVersatility,

    // Universal Perks (Available to all races)
    Tough,
    StrongBack,
    Charming,
    Hardy,
    FastLearner,

    // Unicorn-Only Perks
    MagicalAptitude,
    TeleportMaster,
    MagicShield,
    Levitation,
    SpellCaster,

    // Pegasus-Only Perks
    WeatherControl,
    SonicRainboom,
    CloudWalking,
    AerialAce,
    StormMaster,

    // Earth Pony-Only Perks
    EarthConnection,
    SuperStrength,
    PlantWhisperer,
    IronHooves,
    NaturalHealer,

    // Bat Pony-Only Perks
    NightVision,
    SilentFlight,
    EcholocationMaster,
    ShadowStep,

    // Griffon-Only Perks
    TalonStrike,
    KeenEye,
    PredatorInstinct,
    AerialCombat,

    // Dragon-Only Perks
    DragonScales,
    FireBreath,
    AncientWisdom,
    DragonFear,

    // Human-Only Perks
    Adaptability,
    Innovation,
    Diplomacy,
    QuickLearner
}

public enum SkillCategory
{
    Combat,
    Magic,
    Social,
    Nature,
    Flight,
    Leadership,
    Crafting,
    Utility
}

public enum StatType
{
    Strength,
    Intelligence,
    Wisdom,
    Charisma,
    Constitution,
    Dexterity,
    Health,
    Energy,
    Magic,
    Friendship,
    Corruption
}

public enum PerkCategory
{
    Racial,
    Physical,
    Magical,
    Social,
    Mental,
    Nature,
    Flight,
    Combat,
    Magic,
    Utility,
    Crafting    // added because scripts reference PerkCategory.Crafting
}

// Equipment slot enum (used across item/equipment code)
public enum EquipmentSlot
{
    None,
    Head,
    Neck,
    Chest,
    Back,
    Hands,
    Legs,
    Feet,
    Weapon,
    OffHand,
    Accessory,
    Ring,
    Trinket
}

// Item quality / rarity enum (added Poor because some code expects it)
public enum ItemQuality
{
    Junk = 0,
    Poor = 1,     // added to match existing code references
    Common = 2,
    Uncommon = 3,
    Rare = 4,
    Epic = 5,
    Legendary = 6,
    Artifact = 7
}

// Enemy disposition enum (added Enamored because some code references it)
public enum EnemyDisposition
{
    Hostile,
    Aggressive,
    Neutral,
    Passive,
    Friendly,
    Enamored
}

// NEW: Personality System Enums
public enum PersonalityArchetype
{
    Balanced,    // No dominant traits
    Idealist,    // High kindness + optimism
    Leader,      // High confidence + assertiveness
    Scholar,     // High curiosity + honesty
    Pragmatist,  // Low kindness + high assertiveness
    Pessimist,   // Low confidence + optimism
    Manipulator, // Low honesty + high confidence
    Caregiver    // High kindness + low assertiveness
}

public enum PersonalityCategory
{
    Social,      // Shy, Outgoing, Charming
    Moral,       // Kind, Pragmatic, Ruthless  
    Intellectual,// Curious, Practical, Studious
    Emotional    // Optimistic, Pessimistic, Calm
}