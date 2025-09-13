using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyGameNamespace;

public class PersonalityDatabase : MonoBehaviour
{
    public static PersonalityDatabase Instance { get; private set; }

    private static Dictionary<string, PersonalityTrait> traitDatabase = new Dictionary<string, PersonalityTrait>();
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

        CreateDefaultTraits();
        isInitialized = true;
        Debug.Log($"PersonalityDatabase initialized with {traitDatabase.Count} traits");
    }

    private void CreateDefaultTraits()
    {
        traitDatabase.Clear();

        // Positive Traits
        AddTrait(new PersonalityTrait
        {
            id = "compassionate",
            name = "Compassionate",
            description = "Your heart aches for the suffering of others. You gain friendship more easily but lose it when making cruel choices.",
            category = PersonalityCategory.Social,
            intensity = 4,
            statModifiers = new Dictionary<StatType, int> { { StatType.Charisma, 2 }, { StatType.Wisdom, 1 } },
            unlockedDialogueOptions = new List<string> { "comfort", "heal", "protect" },
            blockedDialogueOptions = new List<string> { "cruel", "abandon", "ignore_suffering" },
            storyFlags = new List<string> { "trait_compassionate", "personality_kind" }
        });

        AddTrait(new PersonalityTrait
        {
            id = "charismatic",
            name = "Charismatic",
            description = "Your presence lights up any room. Others are naturally drawn to your confidence and charm.",
            category = PersonalityCategory.Social,
            intensity = 4,
            statModifiers = new Dictionary<StatType, int> { { StatType.Charisma, 3 } },
            unlockedDialogueOptions = new List<string> { "charm", "persuade", "inspire", "lead" },
            storyFlags = new List<string> { "trait_charismatic", "personality_confident" }
        });

        AddTrait(new PersonalityTrait
        {
            id = "scholar",
            name = "Scholar",
            description = "Knowledge is your greatest treasure. You seek understanding in all things and remember what you learn.",
            category = PersonalityCategory.Intellectual,
            intensity = 4,
            statModifiers = new Dictionary<StatType, int> { { StatType.Intelligence, 2 }, { StatType.Wisdom, 2 } },
            unlockedDialogueOptions = new List<string> { "analyze", "research", "teach", "question" },
            storyFlags = new List<string> { "trait_scholar", "personality_curious" }
        });

        AddTrait(new PersonalityTrait
        {
            id = "inspiring",
            name = "Inspiring",
            description = "Your optimism is infectious. Even in the darkest times, you help others see the light.",
            category = PersonalityCategory.Emotional,
            intensity = 4,
            statModifiers = new Dictionary<StatType, int> { { StatType.Charisma, 2 } },
            unlockedDialogueOptions = new List<string> { "encourage", "motivate", "hope", "uplift" },
            storyFlags = new List<string> { "trait_inspiring", "personality_optimistic" }
        });

        AddTrait(new PersonalityTrait
        {
            id = "commanding",
            name = "Commanding Presence",
            description = "When you speak, others listen. Your natural authority makes you a born leader.",
            category = PersonalityCategory.Social,
            intensity = 4,
            statModifiers = new Dictionary<StatType, int> { { StatType.Charisma, 1 }, { StatType.Strength, 1 } },
            unlockedDialogueOptions = new List<string> { "command", "order", "direct", "take_charge" },
            storyFlags = new List<string> { "trait_commanding", "personality_assertive" }
        });

        // Negative Traits
        AddTrait(new PersonalityTrait
        {
            id = "ruthless",
            name = "Ruthless",
            description = "You've learned that mercy is a luxury you can't afford. Efficiency matters more than feelings.",
            category = PersonalityCategory.Moral,
            intensity = 4,
            statModifiers = new Dictionary<StatType, int> { { StatType.Strength, 2 }, { StatType.Charisma, -1 } },
            unlockedDialogueOptions = new List<string> { "threaten", "eliminate", "cruel", "efficient" },
            blockedDialogueOptions = new List<string> { "mercy", "forgive", "spare" },
            storyFlags = new List<string> { "trait_ruthless", "personality_cruel" }
        });

        AddTrait(new PersonalityTrait
        {
            id = "manipulative",
            name = "Manipulative",
            description = "Truth is just one tool among many. You've mastered the art of getting what you want through words.",
            category = PersonalityCategory.Social,
            intensity = 4,
            statModifiers = new Dictionary<StatType, int> { { StatType.Charisma, 2 }, { StatType.Intelligence, 1 } },
            unlockedDialogueOptions = new List<string> { "manipulate", "deceive", "trick", "exploit" },
            blockedDialogueOptions = new List<string> { "honest", "straightforward", "truthful" },
            storyFlags = new List<string> { "trait_manipulative", "personality_deceptive" }
        });

        // Neutral/Balanced Traits
        AddTrait(new PersonalityTrait
        {
            id = "pragmatic",
            name = "Pragmatic",
            description = "You focus on what works rather than what feels good. Results matter more than intentions.",
            category = PersonalityCategory.Moral,
            intensity = 3,
            statModifiers = new Dictionary<StatType, int> { { StatType.Intelligence, 1 }, { StatType.Wisdom, 1 } },
            unlockedDialogueOptions = new List<string> { "practical", "efficient", "realistic" },
            storyFlags = new List<string> { "trait_pragmatic", "personality_practical" }
        });

        AddTrait(new PersonalityTrait
        {
            id = "curious",
            name = "Endlessly Curious",
            description = "Every mystery calls to you. You can't resist investigating the unknown, even when it's dangerous.",
            category = PersonalityCategory.Intellectual,
            intensity = 3,
            statModifiers = new Dictionary<StatType, int> { { StatType.Intelligence, 1 }, { StatType.Dexterity, 1 } },
            unlockedDialogueOptions = new List<string> { "investigate", "explore", "experiment", "discover" },
            storyFlags = new List<string> { "trait_curious", "personality_investigative" }
        });

        AddTrait(new PersonalityTrait
        {
            id = "protective",
            name = "Protective",
            description = "You have a strong instinct to shield others from harm, sometimes at your own expense.",
            category = PersonalityCategory.Social,
            intensity = 3,
            statModifiers = new Dictionary<StatType, int> { { StatType.Constitution, 2 } },
            unlockedDialogueOptions = new List<string> { "protect", "shield", "defend", "guard" },
            storyFlags = new List<string> { "trait_protective", "personality_guardian" }
        });
    }

    private void AddTrait(PersonalityTrait trait)
    {
        if (trait.statModifiers == null) trait.statModifiers = new Dictionary<StatType, int>();
        if (trait.unlockedDialogueOptions == null) trait.unlockedDialogueOptions = new List<string>();
        if (trait.blockedDialogueOptions == null) trait.blockedDialogueOptions = new List<string>();
        if (trait.storyFlags == null) trait.storyFlags = new List<string>();

        traitDatabase[trait.id] = trait;
    }

    public static PersonalityTrait GetTrait(string traitId)
    {
        if (!isInitialized && Instance != null)
            Instance.InitializeDatabase();

        return traitDatabase.TryGetValue(traitId, out PersonalityTrait trait) ? trait : null;
    }

    public static List<PersonalityTrait> GetAllTraits()
    {
        if (!isInitialized && Instance != null)
            Instance.InitializeDatabase();

        return new List<PersonalityTrait>(traitDatabase.Values);
    }

    public static List<PersonalityTrait> GetTraitsByCategory(PersonalityCategory category)
    {
        if (!isInitialized && Instance != null)
            Instance.InitializeDatabase();

        return traitDatabase.Values.Where(t => t.category == category).ToList();
    }

    [ContextMenu("Debug Personality Database")]
    public void DebugDatabase()
    {
        Debug.Log("=== PERSONALITY DATABASE DEBUG ===");
        foreach (var trait in traitDatabase.Values)
        {
            Debug.Log($"Trait: {trait.name} ({trait.id}) - Category: {trait.category}");
            Debug.Log($"  Description: {trait.description}");
            Debug.Log($"  Unlocks: {string.Join(", ", trait.unlockedDialogueOptions)}");
            if (trait.blockedDialogueOptions.Count > 0)
                Debug.Log($"  Blocks: {string.Join(", ", trait.blockedDialogueOptions)}");
        }
    }
}
