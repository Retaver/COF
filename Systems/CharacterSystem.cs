using UnityEngine;
using MyGameNamespace;

public class CharacterSystem : MonoBehaviour
{
    private PlayerCharacter playerCharacter;
    private Inventory inventory;

    // Ensure initialization as soon as the GameObject is created
    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (playerCharacter == null)
        {
            Debug.Log("[CharacterSystem] Creating new PlayerCharacter");
            playerCharacter = new PlayerCharacter();
        }

        if (inventory == null)
        {
            inventory = new Inventory();
        }
    }

    public PlayerCharacter GetPlayerCharacter()
    {
        // Return existing playerCharacter (Awake should have initialized it)
        if (playerCharacter == null)
        {
            Debug.Log("[CharacterSystem] PlayerCharacter was null when requested — initializing now");
            Initialize();
        }
        return playerCharacter;
    }

    public void SetPlayerCharacter(PlayerCharacter character)
    {
        playerCharacter = character;
        Debug.Log($"[CharacterSystem] Player character set: {character?.name ?? "null"}");

        // Notify UI systems of the change
        if (character != null)
        {
            GameEventSystem.Instance?.RaisePlayerCharacterChanged(character);
            GameEventSystem.Instance?.RaisePlayerStatsChanged(character);
        }
    }

    // Clear character data (useful for starting fresh)
    public void ClearCharacterData()
    {
        playerCharacter = null;
        inventory = null;
        Debug.Log("[CharacterSystem] Character data cleared");
    }

    public void ApplyRacialBonuses(Race race)
    {
        if (race == null || playerCharacter == null) return;

        playerCharacter.ApplyRacialBonuses(race);
        Debug.Log($"[CharacterSystem] Applied racial bonuses for {race.name}");
    }

    public void AddStartingItems()
    {
        if (inventory == null) inventory = new Inventory();

        var itemDb = ItemDatabase.Instance;
        if (itemDb != null)
        {
            var healthPotion = itemDb.GetItemById("health_potion");
            var energyPotion = itemDb.GetItemById("energy_potion");
            var bread = itemDb.GetItemById("bread");

            if (healthPotion != null) inventory.AddItem(healthPotion, 2);
            if (energyPotion != null) inventory.AddItem(energyPotion, 1);
            if (bread != null) inventory.AddItem(bread, 3);

            Debug.Log("[CharacterSystem] Added starting items to inventory");
        }
        else
        {
            Debug.LogError("[CharacterSystem] ItemDatabase instance not found");
        }
    }

    public Inventory GetInventory()
    {
        if (inventory == null) inventory = new Inventory();
        return inventory;
    }

    public string GetPlayerStatsDisplay()
    {
        if (playerCharacter == null || playerCharacter.stats == null)
            return "No character stats available";

        return playerCharacter.stats.GetStatsDisplay();
    }

    public void AddExperience(int amount)
    {
        if (playerCharacter != null)
        {
            playerCharacter.AddExperience(amount);
            Debug.Log($"[CharacterSystem] Added {amount} XP to player. Current level: {playerCharacter.level}");
        }
        else
        {
            Debug.LogError("[CharacterSystem] Cannot add experience - PlayerCharacter is null");
        }
    }

    public bool AddPerk(PerkType perkType)
    {
        if (playerCharacter != null)
        {
            return playerCharacter.AddPerk(perkType);
        }
        Debug.LogError("[CharacterSystem] Cannot add perk - PlayerCharacter is null");
        return false;
    }

    public bool AllocateStatPoint(StatType stat, int points = 1)
    {
        if (playerCharacter != null)
        {
            return playerCharacter.AllocateStatPoint(stat, points);
        }
        Debug.LogError("[CharacterSystem] Cannot allocate stat point - PlayerCharacter is null");
        return false;
    }
}