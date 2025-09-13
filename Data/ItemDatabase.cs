using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    [Header("Item Database")]
    [SerializeField] private List<InventoryItem> defaultItems = new();

    private Dictionary<string, InventoryItem> itemsById = new();
    private bool isInitialized = false;

    // Static convenience properties (redirect to instance)
    public static InventoryItem HealthPotion => Instance?.GetItemById("health_potion");
    public static InventoryItem EnergyPotion => Instance?.GetItemById("energy_potion");
    public static InventoryItem Bread => Instance?.GetItemById("bread");
    public static InventoryItem Apple => Instance?.GetItemById("apple");
    public static InventoryItem MagicPotion => Instance?.GetItemById("magic_potion");

    public bool IsInitialized => isInitialized;

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

        try
        {
            itemsById.Clear();
            CreateDefaultItems();
            // If inspector defaultItems are provided, register them too
            if (defaultItems != null)
            {
                foreach (var it in defaultItems)
                {
                    if (it != null && !string.IsNullOrEmpty(it.id))
                        itemsById[it.id] = it;
                }
            }

            isInitialized = true;
            Debug.Log($"ItemDatabase initialized with {itemsById.Count} items");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize ItemDatabase: {e.Message}");
        }
    }

    private void CreateDefaultItems()
    {
        // Health Potion
        RegisterItem(new InventoryItem(
            "health_potion",
            "Health Potion",
            "A magical potion that restores 25 health points.",
            "Icons/health_potion",
            true,
            "HealthEffect",
            ItemCategory.Consumable
        ));

        // Energy Potion
        RegisterItem(new InventoryItem(
            "energy_potion",
            "Energy Potion",
            "A refreshing potion that restores 25 energy points.",
            "Icons/energy_potion",
            true,
            "EnergyEffect",
            ItemCategory.Consumable
        ));

        // Bread
        RegisterItem(new InventoryItem(
            "bread",
            "Fresh Bread",
            "Simple but nourishing bread. Restores 10 health and 10 energy.",
            "Icons/bread",
            true,
            "FoodEffect",
            ItemCategory.Consumable
        ));

        // Apple (legacy support)
        RegisterItem(new InventoryItem(
            "apple",
            "Fresh Apple",
            "A crisp, juicy apple from Sweet Apple Acres. Restores 5 health.",
            "Icons/apple",
            true,
            "",
            ItemCategory.Consumable
        ));

        // Weapons
        RegisterItem(new InventoryItem(
            "basic_sword",
            "Iron Sword",
            "A simple but sturdy iron sword.",
            "Icons/iron_sword",
            false,
            "",
            ItemCategory.Weapon
        ));

        // Armor
        RegisterItem(new InventoryItem(
            "leather_armor",
            "Leather Armor",
            "Basic leather armor that provides modest protection.",
            "Icons/leather_armor",
            false,
            "",
            ItemCategory.Armor
        ));

        // Add more items as needed...
    }

    private void RegisterItem(InventoryItem item)
    {
        if (item != null && !string.IsNullOrEmpty(item.id))
        {
            itemsById[item.id] = item;
        }
        else
        {
            Debug.LogWarning("Attempted to register invalid item");
        }
    }

    // Instance lookup - primary public accessor
    public InventoryItem GetItemById(string itemId)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("ItemDatabase not initialized yet");
            return null;
        }

        if (itemsById.TryGetValue(itemId, out InventoryItem item))
        {
            return item.Clone();
        }

        Debug.LogWarning($"Item not found: {itemId}");
        return null;
    }

    public List<InventoryItem> GetAllItems()
    {
        if (!isInitialized) return new List<InventoryItem>();

        List<InventoryItem> items = new List<InventoryItem>();
        foreach (var item in itemsById.Values)
        {
            items.Add(item.Clone());
        }
        return items;
    }

    public bool HasItem(string itemId)
    {
        return isInitialized && itemsById.ContainsKey(itemId);
    }

    // Static methods for backward compatibility (do not conflict with instance method)
    public static InventoryItem GetItemByIdStatic(string itemId)
    {
        return Instance?.GetItemById(itemId);
    }

    public static List<InventoryItem> GetAllItemsStatic()
    {
        return Instance?.GetAllItems() ?? new List<InventoryItem>();
    }

    public static bool HasItemStatic(string itemId)
    {
        return Instance?.HasItem(itemId) ?? false;
    }

    // Category lookup helper (static)
    public static ItemCategory GetItemCategory(string itemId)
    {
        if (Instance == null) return ItemCategory.Miscellaneous;
        var item = Instance.GetItemById(itemId);
        return item != null ? item.category : ItemCategory.Miscellaneous;
    }
}