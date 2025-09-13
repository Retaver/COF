using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MyGameNamespace;

[Serializable]
public class InventoryData
{
    public string id;
    public string name;
    public string description;
    public int quantity;
    public string iconPath;
    public bool isUsable;
    public string effectId; // Use effectId for compatibility

    // Constructor for easier item creation
    public InventoryData(string itemId, string itemName, string desc, string icon = "", bool usable = false, string effect = "")
    {
        id = itemId;
        name = itemName;
        description = desc;
        quantity = 1;
        iconPath = icon;
        isUsable = usable;
        effectId = effect;
    }
}

[Serializable]
public class Inventory
{
    public List<InventoryItem> items = new List<InventoryItem>();

    public void AddItem(InventoryItem item, int quantity = 1)
    {
        var existingItem = items.FirstOrDefault(i => i.id == item.id);
        if (existingItem != null)
        {
            existingItem.quantity += quantity;
        }
        else
        {
            // Create a copy to avoid modifying the original
            InventoryItem newItem = new InventoryItem(
                item.id,
                item.name,
                item.description,
                item.iconPath,
                item.isUsable,
                item.effectId // <-- Fixed: use effectId here, not useAction!
            );
            newItem.quantity = quantity;
            items.Add(newItem);
        }

        // Raise inventory changed event
        GameEventSystem.Instance.RaiseInventoryChanged(this);
    }

    public void RemoveItem(string itemId, int quantity = 1)
    {
        var item = items.FirstOrDefault(i => i.id == itemId);
        if (item != null)
        {
            item.quantity -= quantity;
            if (item.quantity <= 0)
            {
                items.Remove(item);
            }

            // Raise inventory changed event
            GameEventSystem.Instance.RaiseInventoryChanged(this);
        }
    }

    public bool HasItem(string itemId)
    {
        return items.Any(i => i.id == itemId && i.quantity > 0);
    }

    public int GetItemQuantity(string itemId)
    {
        var item = items.FirstOrDefault(i => i.id == itemId);
        return item != null ? item.quantity : 0;
    }

    public bool UseItem(string itemId, PlayerCharacter player)
    {
        var item = items.FirstOrDefault(i => i.id == itemId);
        if (item != null && item.isUsable && item.quantity > 0)
        {
            // Execute the item's use method
            if (item.Use(player))
            {
                // Reduce quantity
                item.quantity--;

                // Remove if quantity is zero
                if (item.quantity <= 0)
                {
                    items.Remove(item);
                }

                // Raise inventory changed event
                GameEventSystem.Instance.RaiseInventoryChanged(this);

                return true;
            }
        }
        return false;
    }

    // Get all items of a specific type
    public List<InventoryItem> GetItemsByType(string type)
    {
        return items.Where(i => i.id.StartsWith(type + "_")).ToList();
    }

    // Get all usable items
    public List<InventoryItem> GetUsableItems()
    {
        return items.Where(i => i.isUsable && i.quantity > 0).ToList();
    }

    // Clear the inventory
    public void Clear()
    {
        items.Clear();

        // Raise inventory changed event
        GameEventSystem.Instance.RaiseInventoryChanged(this);
    }
}