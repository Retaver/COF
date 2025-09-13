using System.Collections.Generic;
using UnityEngine;

public class BackgroundDatabase : MonoBehaviour
{
    public static BackgroundDatabase Instance { get; private set; }
    private static Dictionary<string, Background> backgroundDatabase = new Dictionary<string, Background>();
    private static bool isInitialized = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            var rootGo = transform.root != null ? transform.root.gameObject : gameObject;
            DontDestroyOnLoad(rootGo);
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

        CreateDefaultBackgrounds();
        isInitialized = true;
        Debug.Log($"BackgroundDatabase initialized with {backgroundDatabase.Count} backgrounds");
    }

    private void CreateDefaultBackgrounds()
    {
        backgroundDatabase.Clear();

        // Scholar Background
        AddBackground(new Background
        {
            id = "scholar",
            name = "Scholar",
            description = "You spent your youth buried in books and ancient texts in Canterlot's Great Library.",
            flavorText = "Knowledge has always been your greatest treasure. You remember countless hours reading by candlelight...",
            statBonuses = new Dictionary<StatType, int> { { StatType.Intelligence, 2 }, { StatType.Wisdom, 1 } },
            startingItems = new List<string> { "ancient_book", "quill", "ink" },
            startingBits = 75,
            suggestedTraits = new List<PersonalityTrait>(),
            unlockedDialogueCategories = new List<string> { "academic", "research", "knowledge" },
            backgroundFlags = new List<string> { "background_scholar", "canterlot_connection" }
        });

        // Guard Background
        AddBackground(new Background
        {
            id = "guard",
            name = "Town Guard",
            description = "You served as a guard in your hometown, protecting ponies and keeping the peace.",
            flavorText = "Years of training and duty have shaped you into a protector. You still remember your oath...",
            statBonuses = new Dictionary<StatType, int> { { StatType.Strength, 1 }, { StatType.Constitution, 2 } },
            startingItems = new List<string> { "guard_badge", "basic_armor", "guard_manual" },
            startingBits = 100,
            suggestedTraits = new List<PersonalityTrait>(),
            unlockedDialogueCategories = new List<string> { "law", "protection", "authority" },
            backgroundFlags = new List<string> { "background_guard", "law_enforcement" }
        });

        // Merchant Background
        AddBackground(new Background
        {
            id = "merchant",
            name = "Traveling Merchant",
            description = "You've spent years traveling between towns, buying and selling goods.",
            flavorText = "The road has been your home, and you know the value of everything. But now...",
            statBonuses = new Dictionary<StatType, int> { { StatType.Charisma, 2 }, { StatType.Intelligence, 1 } },
            startingItems = new List<string> { "merchant_ledger", "trade_goods", "travel_pack" },
            startingBits = 150,
            suggestedTraits = new List<PersonalityTrait>(),
            unlockedDialogueCategories = new List<string> { "trade", "negotiation", "travel" },
            backgroundFlags = new List<string> { "background_merchant", "trade_connections" }
        });

        // Farm Pony Background
        AddBackground(new Background
        {
            id = "farmer",
            name = "Farm Pony",
            description = "You grew up working the fields, connected to the earth and simple honest work.",
            flavorText = "The soil under your hooves, the sun on your back... that was your world until everything changed.",
            statBonuses = new Dictionary<StatType, int> { { StatType.Strength, 1 }, { StatType.Constitution, 1 }, { StatType.Wisdom, 1 } },
            startingItems = new List<string> { "farming_tools", "seeds", "harvest_basket" },
            startingBits = 50,
            suggestedTraits = new List<PersonalityTrait>(),
            unlockedDialogueCategories = new List<string> { "farming", "nature", "honest_work" },
            backgroundFlags = new List<string> { "background_farmer", "earth_connection" }
        });

        // Entertainer Background
        AddBackground(new Background
        {
            id = "entertainer",
            name = "Entertainer",
            description = "You performed in taverns and theaters, bringing joy and music to ponies everywhere.",
            flavorText = "The stage was your world, the crowd your family. Every night brought new faces and new stories...",
            statBonuses = new Dictionary<StatType, int> { { StatType.Charisma, 3 } },
            startingItems = new List<string> { "musical_instrument", "costume", "performance_notes" },
            startingBits = 80,
            suggestedTraits = new List<PersonalityTrait>(),
            unlockedDialogueCategories = new List<string> { "performance", "charm", "stories" },
            backgroundFlags = new List<string> { "background_entertainer", "social_connections" }
        });

        // Outcast Background
        AddBackground(new Background
        {
            id = "outcast",
            name = "Outcast",
            description = "You've lived on the margins of society, never quite fitting in anywhere.",
            flavorText = "Others always looked at you differently. You learned to rely on yourself...",
            statBonuses = new Dictionary<StatType, int> { { StatType.Dexterity, 2 }, { StatType.Wisdom, 1 } },
            startingItems = new List<string> { "survival_kit", "hidden_blade" },
            startingBits = 25,
            suggestedTraits = new List<PersonalityTrait>(),
            unlockedDialogueCategories = new List<string> { "survival", "outsider", "pragmatic" },
            backgroundFlags = new List<string> { "background_outcast", "self_reliant" }
        });
    }

    private void AddBackground(Background background)
    {
        if (background.statBonuses == null) background.statBonuses = new Dictionary<StatType, int>();
        if (background.startingItems == null) background.startingItems = new List<string>();
        if (background.suggestedTraits == null) background.suggestedTraits = new List<PersonalityTrait>();
        if (background.unlockedDialogueCategories == null) background.unlockedDialogueCategories = new List<string>();
        if (background.backgroundFlags == null) background.backgroundFlags = new List<string>();

        backgroundDatabase[background.id] = background;
    }

    public static Background GetBackground(string backgroundId)
    {
        if (!isInitialized && Instance != null)
            Instance.InitializeDatabase();

        return backgroundDatabase.TryGetValue(backgroundId, out Background background) ? background : null;
    }

    public static List<Background> GetAllBackgrounds()
    {
        if (!isInitialized && Instance != null)
            Instance.InitializeDatabase();

        return new List<Background>(backgroundDatabase.Values);
    }

    public void PopulateSuggestedTraits()
    {
        if (PersonalityDatabase.Instance == null) return;

        void AddIf(string bgId, string traitId)
        {
            if (!backgroundDatabase.TryGetValue(bgId, out var bg)) return;
            var trait = PersonalityDatabase.GetTrait(traitId);
            if (trait != null) bg.suggestedTraits.Add(trait);
        }

        if (backgroundDatabase.TryGetValue("scholar", out var scholar)) scholar.suggestedTraits.Clear();
        AddIf("scholar", "curious");
        AddIf("scholar", "scholar");

        if (backgroundDatabase.TryGetValue("guard", out var guard)) guard.suggestedTraits.Clear();
        AddIf("guard", "protective");
        AddIf("guard", "commanding");

        if (backgroundDatabase.TryGetValue("merchant", out var merchant)) merchant.suggestedTraits.Clear();
        AddIf("merchant", "charismatic");
        AddIf("merchant", "pragmatic");

        if (backgroundDatabase.TryGetValue("farmer", out var farmer)) farmer.suggestedTraits.Clear();
        AddIf("farmer", "protective");
        AddIf("farmer", "compassionate");

        if (backgroundDatabase.TryGetValue("entertainer", out var entertainer)) entertainer.suggestedTraits.Clear();
        AddIf("entertainer", "charismatic");
        AddIf("entertainer", "inspiring");

        if (backgroundDatabase.TryGetValue("outcast", out var outcast)) outcast.suggestedTraits.Clear();
        AddIf("outcast", "pragmatic");
        AddIf("outcast", "curious");
    }
}
