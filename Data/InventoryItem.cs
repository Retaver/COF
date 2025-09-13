using System;
using UnityEngine;
using MyGameNamespace;

[Serializable]
public class InventoryItem
{
    public string id;
    public string name;
    public string description;
    public int quantity;
    public string iconPath;
    public bool isUsable;
    public string effectId; // Reference to ItemEffect instead of Action delegate
    public ItemCategory category = ItemCategory.Miscellaneous; // <-- Added

    public InventoryItem(string itemId, string itemName, string desc, string icon = "", bool usable = false, string effect = "", ItemCategory cat = ItemCategory.Miscellaneous)
    {
        id = itemId;
        name = itemName;
        description = desc;
        quantity = 1;
        iconPath = icon;
        isUsable = usable;
        effectId = effect;
        category = cat;
    }

    // Use the item on a player
    public bool Use(PlayerCharacter player)
    {
        if (!isUsable || player == null) return false;

        ItemEffect effect = GetEffect();
        if (effect != null)
        {
            if (effect.CanUse(player))
            {
                effect.Apply(player);
                return true;
            }
            else
            {
                Debug.Log($"Cannot use {name} - conditions not met");
                return false;
            }
        }

        // Fallback for items without ItemEffect (legacy support)
        return UseLegacyEffect(player);
    }

    private ItemEffect GetEffect()
    {
        if (string.IsNullOrEmpty(effectId)) return null;
        return Resources.Load<ItemEffect>($"ItemEffects/{effectId}");
    }

    private bool UseLegacyEffect(PlayerCharacter player)
    {
        switch (id)
        {
            case "health_potion":
                if (player.gameStats.health < player.gameStats.maxHealth)
                {
                    player.gameStats.health = Mathf.Min(player.gameStats.health + 25, player.gameStats.maxHealth);
                    GameEventSystem.Instance?.RaisePlayerStatsChanged(player);
                    return true;
                }
                break;
            case "energy_potion":
                if (player.gameStats.energy < player.gameStats.maxEnergy)
                {
                    player.gameStats.energy = Mathf.Min(player.gameStats.energy + 25, player.gameStats.maxEnergy);
                    GameEventSystem.Instance?.RaisePlayerStatsChanged(player);
                    return true;
                }
                break;
            case "bread":
                if (player.gameStats.health < player.gameStats.maxHealth ||
                    player.gameStats.energy < player.gameStats.maxEnergy)
                {
                    player.gameStats.health = Mathf.Min(player.gameStats.health + 10, player.gameStats.maxHealth);
                    player.gameStats.energy = Mathf.Min(player.gameStats.energy + 10, player.gameStats.maxEnergy);
                    GameEventSystem.Instance?.RaisePlayerStatsChanged(player);
                    return true;
                }
                break;
            case "apple":
                if (player.gameStats.health < player.gameStats.maxHealth)
                {
                    player.gameStats.health = Mathf.Min(player.gameStats.health + 5, player.gameStats.maxHealth);
                    GameEventSystem.Instance?.RaisePlayerStatsChanged(player);
                    return true;
                }
                break;
        }
        Debug.Log($"No effect defined for item: {id}");
        return false;
    }

    public bool CanUse(PlayerCharacter player)
    {
        if (!isUsable || player == null) return false;

        ItemEffect effect = GetEffect();
        if (effect != null)
        {
            return effect.CanUse(player);
        }

        switch (id)
        {
            case "health_potion":
                return player.gameStats.health < player.gameStats.maxHealth;
            case "energy_potion":
                return player.gameStats.energy < player.gameStats.maxEnergy;
            case "bread":
                return player.gameStats.health < player.gameStats.maxHealth ||
                       player.gameStats.energy < player.gameStats.maxEnergy;
            case "apple":
                return player.gameStats.health < player.gameStats.maxHealth;
            default:
                return true;
        }
    }

    public InventoryItem Clone()
    {
        return new InventoryItem(id, name, description, iconPath, isUsable, effectId, category)
        {
            quantity = this.quantity
        };
    }
}