// 9/6/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private Inventory inventory;

    private void Awake()
    {
        EnsureInventory();
    }

    private void EnsureInventory()
    {
        if (inventory == null)
            inventory = new Inventory();

        if (inventory.items == null)
            inventory.items = new List<InventoryItem>();
    }

    /// <summary>
    /// Retrieves an item by its ID.
    /// </summary>
    public InventoryItem GetItemById(string itemId)
    {
        EnsureInventory();
        return inventory.items.FirstOrDefault(item => item.id == itemId);
    }

    public void AddItem(InventoryItem item, int quantity = 1)
    {
        if (item == null) return;
        EnsureInventory();

        inventory.AddItem(item, quantity);
        Debug.Log($"Added {quantity}x {item.name} to inventory");
    }

    public void RemoveItem(string itemId, int quantity = 1)
    {
        EnsureInventory();
        inventory.RemoveItem(itemId, quantity);
        Debug.Log($"Removed {quantity}x {itemId} from inventory");
    }

    public List<InventoryItem> GetAllItems()
    {
        EnsureInventory();
        return inventory.items ?? new List<InventoryItem>();
    }

    public void UseItem(string itemId)
    {
        Debug.Log($"Using item with ID: {itemId}");
        // Add logic to use the item here
    }

}