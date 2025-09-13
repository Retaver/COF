using MyGameNamespace;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryScreenController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument inventoryDocument;

    [Header("Settings")]
    [SerializeField] private bool verboseLogging = true;
    [SerializeField] private int maxCapacity = 100;

    [Header("Capacity Thresholds")]
    [Range(0.0f, 1.0f)] public float warnPct = 0.75f;
    [Range(0.0f, 1.0f)] public float dangerPct = 0.90f;

    private VisualElement root;
    private VisualElement container;

    // Header elements
    private Label inventoryTitle;
    private Label capacityLabel;
    private ProgressBar capacityBar;

    // Category tabs
    private readonly Dictionary<string, Button> categoryTabs = new();
    private Button activeTab;
    private string currentCategory = "All";

    // Main content
    private VisualElement itemsGrid;
    private ScrollView itemsScroll;

    // Item details panel
    private Image detailIcon;
    private Label detailName;
    private Label detailQuantity;
    private Label detailDescription;
    private ScrollView detailDescriptionScroll;

    // Action buttons
    private Button useButton;
    private Button equipButton;
    private Button dropButton;

    // Footer buttons
    private Button sortButton;
    private Button closeButton;

    // Data
    private readonly List<InventoryItem> allItems = new();
    private readonly List<InventoryItem> filteredItems = new();
    private InventoryItem selectedItem;

    // Systems
    private InventorySystem inventorySystem;
    private CharacterSystem characterSystem;

    private void Awake()
    {
        if (inventoryDocument == null)
            inventoryDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (inventoryDocument == null)
        {
            // try to auto-find a UIDocument that looks like the inventory
            var docs = CompatUtils.FindObjectsOfTypeCompat<UIDocument>();
            foreach (var d in docs)
            {
                try
                {
                    var r = d.rootVisualElement;
                    if (r != null && (r.Q<VisualElement>("inventory-container") != null || r.Q<Label>("inventory-title") != null))
                    {
                        inventoryDocument = d;
                        break;
                    }
                }
                catch { }
            }
        }

        if (inventoryDocument == null)
        {
            Debug.LogWarning("InventoryScreenController: No UIDocument found for inventory. Inventory UI will not be available, but controller will not crash.");
            return;
        }

        root = inventoryDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogWarning("InventoryScreenController: rootVisualElement is null!");
            return;
        }

        // Get system references
        inventorySystem = CompatUtils.FindFirstObjectByTypeCompat<InventorySystem>();
        characterSystem = CompatUtils.FindFirstObjectByTypeCompat<CharacterSystem>();

        CacheElements();
        SetupCategoryTabs();
        SetupButtons();

        // Hide initially
        Hide();

        if (verboseLogging) Debug.Log("InventoryScreenController initialized");
    }

    private void CacheElements()
    {
        if (root == null)
        {
            if (verboseLogging) Debug.LogWarning("InventoryScreenController: CacheElements called but root is null.");
            return;
        }

        // Find the main container
        container = root.Q<VisualElement>("inventory-container");

        // Header
        inventoryTitle = root.Q<Label>("inventory-title");
        capacityLabel = root.Q<Label>("capacity-label");
        capacityBar = root.Q<ProgressBar>("capacity-bar");

        // Category tabs
        var tabNames = new[] { "all-tab", "weapons-tab", "armor-tab", "consumables-tab", "quest-tab", "misc-tab" };
        foreach (var tabName in tabNames)
        {
            var tab = root.Q<Button>(tabName);
            if (tab != null)
            {
                categoryTabs[tabName] = tab;
            }
        }

        // Content
        itemsScroll = root.Q<ScrollView>("items-scroll");
        itemsGrid = root.Q<VisualElement>("items-grid");

        // Details panel
        detailIcon = root.Q<Image>("item-icon");
        detailName = root.Q<Label>("item-name");
        detailQuantity = root.Q<Label>("item-quantity");
        detailDescription = root.Q<Label>("item-description");
        detailDescriptionScroll = root.Q<ScrollView>("item-description-scroll");

        // Action buttons
        useButton = root.Q<Button>("use-button");
        equipButton = root.Q<Button>("equip-button");
        dropButton = root.Q<Button>("drop-button");

        // Footer buttons
        sortButton = root.Q<Button>("sort-button");
        closeButton = root.Q<Button>("close-button");
    }

    private void SetupCategoryTabs()
    {
        foreach (var (tabName, tabButton) in categoryTabs)
        {
            if (tabButton != null)
            {
                tabButton.clicked += () => SelectCategory(tabName);
            }
        }

        // Set "All" as default active tab
        if (categoryTabs.TryGetValue("all-tab", out var allTab))
        {
            activeTab = allTab;
            allTab.AddToClassList("active");
        }
    }

    private void SetupButtons()
    {
        // Action buttons
        if (useButton != null)
            useButton.clicked += OnUseClicked;

        if (equipButton != null)
            equipButton.clicked += OnEquipClicked;

        if (dropButton != null)
            dropButton.clicked += OnDropClicked;

        // Footer buttons
        if (sortButton != null)
            sortButton.clicked += OnSortClicked;

        if (closeButton != null)
        {
            closeButton.clicked += () =>
            {
                var gsm = CompatUtils.FindFirstObjectByTypeCompat<GameSceneManager>();
                if (gsm != null)
                    gsm.ToggleInventory(false);
                else
                    Hide();
            };
        }
    }

    private void SelectCategory(string tabName)
    {
        // Remove active class from all tabs
        foreach (var tab in categoryTabs.Values)
        {
            tab?.RemoveFromClassList("active");
        }

        // Add active class to selected tab
        if (categoryTabs.TryGetValue(tabName, out var selectedTab))
        {
            selectedTab.AddToClassList("active");
            activeTab = selectedTab;
        }

        // Update category filter
        currentCategory = tabName switch
        {
            "all-tab" => "All",
            "weapons-tab" => "Weapons",
            "armor-tab" => "Armor",
            "consumables-tab" => "Consumables",
            "quest-tab" => "Quest Items",
            "misc-tab" => "Misc",
            _ => "All"
        };

        RefreshItems();

        if (verboseLogging) Debug.Log($"Selected category: {currentCategory}");
    }

    public void Show()
    {
        if (root != null)
        {
            root.style.display = DisplayStyle.Flex;

            // Make sure this UIDocument root is front-most and accepts pointer events
            try { root.pickingMode = PickingMode.Position; } catch { }
            try { root.BringToFront(); } catch { }
        }

        RefreshInventory();
    }

    public void Hide()
    {
        if (root != null) root.style.display = DisplayStyle.None;
        selectedItem = null;
    }

    public bool IsVisible()
    {
        return root != null && root.style.display == DisplayStyle.Flex;
    }

    public void RefreshInventory()
    {
        LoadItemsFromSystem();
        UpdateCapacityDisplay();
        RefreshItems();
        UpdateItemDetails(null);
    }

    private void LoadItemsFromSystem()
    {
        allItems.Clear();

        if (inventorySystem != null)
        {
            var systemItems = inventorySystem.GetAllItems();
            if (systemItems != null)
            {
                foreach (var item in systemItems)
                {
                    if (item != null && item.quantity > 0)
                    {
                        allItems.Add(item);
                    }
                }
            }
        }

        if (verboseLogging) Debug.Log($"Loaded {allItems.Count} items from inventory system");
    }

    private void RefreshItems()
    {
        FilterItems();
        PopulateItemsGrid();
    }

    private void FilterItems()
    {
        filteredItems.Clear();

        foreach (var item in allItems)
        {
            if (ShouldShowItem(item))
            {
                filteredItems.Add(item);
            }
        }

        // Sort items
        filteredItems.Sort((a, b) =>
        {
            var categoryA = GetItemCategory(a);
            var categoryB = GetItemCategory(b);
            int categoryComparison = categoryA.CompareTo(categoryB);

            if (categoryComparison != 0)
                return categoryComparison;

            return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
        });
    }

    private bool ShouldShowItem(InventoryItem item)
    {
        if (currentCategory == "All") return true;

        var itemCategory = GetItemCategory(item);
        return currentCategory switch
        {
            "Weapons" => itemCategory == ItemCategory.Weapon,
            "Armor" => itemCategory == ItemCategory.Armor,
            "Consumables" => itemCategory == ItemCategory.Consumable,
            "Quest Items" => itemCategory == ItemCategory.Quest,
            "Misc" => itemCategory == ItemCategory.Miscellaneous || itemCategory == ItemCategory.Material || itemCategory == ItemCategory.Valuable,
            _ => true
        };
    }

    private ItemCategory GetItemCategory(InventoryItem item)
    {
        try
        {
            return ItemDatabase.GetItemCategory(item.id);
        }
        catch
        {
            return ItemCategory.Miscellaneous;
        }
    }

    private void PopulateItemsGrid()
    {
        if (itemsGrid == null) return;

        itemsGrid.Clear();

        if (filteredItems.Count == 0)
        {
            var emptyLabel = new Label($"No {currentCategory.ToLower()} items found.");
            emptyLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
            emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            emptyLabel.style.fontSize = 14;
            emptyLabel.style.marginTop = 20;
            itemsGrid.Add(emptyLabel);
            return;
        }

        foreach (var item in filteredItems)
        {
            var slot = CreateItemSlot(item);
            itemsGrid.Add(slot);
        }
    }

    private VisualElement CreateItemSlot(InventoryItem item)
    {
        var slot = new VisualElement();
        slot.AddToClassList("item-slot");

        // Item icon
        var icon = new Image();
        icon.AddToClassList("item-icon");
        icon.image = LoadItemIcon(item.iconPath);
        slot.Add(icon);

        // Quantity badge
        if (item.quantity > 1)
        {
            var quantityLabel = new Label($"x{item.quantity}");
            quantityLabel.AddToClassList("item-quantity");
            slot.Add(quantityLabel);
        }

        // Click handler
        slot.RegisterCallback<ClickEvent>(_ =>
        {
            SelectItem(item, slot);
        });

        // Tooltip on hover
        slot.tooltip = $"{item.name}\n{item.description}";

        return slot;
    }

    private void SelectItem(InventoryItem item, VisualElement slot)
    {
        // Remove selection from all slots
        if (itemsGrid != null)
        {
            foreach (var child in itemsGrid.Children())
            {
                child.RemoveFromClassList("selected");
            }
        }

        // Add selection to clicked slot
        slot.AddToClassList("selected");

        selectedItem = item;
        UpdateItemDetails(item);

        if (verboseLogging) Debug.Log($"Selected item: {item.name}");
    }

    private void UpdateItemDetails(InventoryItem item)
    {
        if (item == null)
        {
            // Show default/empty state
            if (detailIcon != null) detailIcon.image = null;
            if (detailName != null) detailName.text = "Item Name";
            if (detailQuantity != null) detailQuantity.text = "x1";
            if (detailDescription != null) detailDescription.text = "Item description will appear here.";

            UpdateActionButtons(null);
            return;
        }

        // Update details
        if (detailIcon != null) detailIcon.image = LoadItemIcon(item.iconPath);
        if (detailName != null) detailName.text = item.name;
        if (detailQuantity != null) detailQuantity.text = $"x{item.quantity}";
        if (detailDescription != null) detailDescription.text = item.description;

        UpdateActionButtons(item);
    }

    private void UpdateActionButtons(InventoryItem item)
    {
        var player = characterSystem?.GetPlayerCharacter();
        bool hasItem = item != null;

        // Use button
        if (useButton != null)
        {
            bool canUse = hasItem && item.isUsable && item.CanUse(player);
            useButton.SetEnabled(canUse);
        }

        // Equip button (placeholder - not implemented)
        if (equipButton != null)
        {
            equipButton.SetEnabled(false);
            equipButton.tooltip = "Equipment system not yet implemented";
        }

        // Drop button
        if (dropButton != null)
        {
            dropButton.SetEnabled(hasItem);
        }
    }

    private void UpdateCapacityDisplay()
    {
        int totalItems = allItems.Sum(item => item.quantity);

        if (capacityLabel != null)
        {
            capacityLabel.text = $"Capacity: {totalItems}/{maxCapacity}";
        }

        if (capacityBar != null)
        {
            capacityBar.value = totalItems;
            capacityBar.highValue = maxCapacity;

            // Update capacity bar styling
            capacityBar.RemoveFromClassList("ok");
            capacityBar.RemoveFromClassList("warn");
            capacityBar.RemoveFromClassList("danger");

            float percentage = (float)totalItems / maxCapacity;
            if (percentage >= dangerPct)
                capacityBar.AddToClassList("danger");
            else if (percentage >= warnPct)
                capacityBar.AddToClassList("warn");
            else
                capacityBar.AddToClassList("ok");
        }
    }

    // Button event handlers
    private void OnUseClicked()
    {
        if (selectedItem == null || inventorySystem == null) return;

        inventorySystem.UseItem(selectedItem.id); // Pass only the item ID
        RefreshInventory();

        if (verboseLogging) Debug.Log($"Used item: {selectedItem.name}");
    }

    private void OnEquipClicked()
    {
        if (verboseLogging) Debug.Log("Equip functionality not yet implemented");
    }

    private void OnDropClicked()
    {
        if (selectedItem == null || inventorySystem == null) return;

        inventorySystem.RemoveItem(selectedItem.id, 1);
        RefreshInventory();

        if (verboseLogging) Debug.Log($"Dropped item: {selectedItem.name}");
    }

    private void OnSortClicked()
    {
        // Items are already sorted when filtered
        RefreshItems();

        if (verboseLogging) Debug.Log("Inventory sorted");
    }

    private static Texture2D LoadItemIcon(string iconPath)
    {
        if (string.IsNullOrWhiteSpace(iconPath)) return null;
        return Resources.Load<Texture2D>(iconPath);
    }

    // Event subscriptions
    private void OnEnable()
    {
        if (GameEventSystem.Instance != null)
        {
            // Subscribe to inventory changes using += since OnInventoryChanged is a plain Action
            GameEventSystem.Instance.OnInventoryChanged += OnInventoryChanged;
        }
    }

    private void OnDisable()
    {
        if (GameEventSystem.Instance != null)
        {
            // Unsubscribe from inventory changes using -=
            GameEventSystem.Instance.OnInventoryChanged -= OnInventoryChanged;
        }
    }

    // Called when the inventory changes.  The OnInventoryChanged event carries no payload,
    // so this method takes no parameters.
    private void OnInventoryChanged()
    {
        if (IsVisible())
        {
            RefreshInventory();
        }
    }
}