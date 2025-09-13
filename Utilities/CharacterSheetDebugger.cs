using UnityEngine;

public class CharacterSheetDebugger : MonoBehaviour
{
    [Header("Debug Controls")]
    [SerializeField] private KeyCode testKey = KeyCode.F1;

    private void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            DebugCharacterSheet();
        }
    }

    [ContextMenu("Debug Character Sheet")]
    public void DebugCharacterSheet()
    {
        Debug.Log("=== CHARACTER SHEET DEBUG ===");

        // 1. Test GameSceneManager
        var gsm = FindFirstObjectByType<GameSceneManager>();
        if (gsm != null)
        {
            Debug.Log($"✅ Found GameSceneManager: {gsm.name}");
        }
        else
        {
            Debug.LogError("❌ No GameSceneManager found!");
            return;
        }

        // 2. Test CharacterSheetController
        var charSheet = FindFirstObjectByType<CharacterSheetController>();
        if (charSheet != null)
        {
            Debug.Log($"✅ Found CharacterSheetController: {charSheet.name}");
        }
        else
        {
            Debug.LogError("❌ No CharacterSheetController found!");
            return;
        }

        // 3. Test CharacterSystem
        var characterSystem = FindFirstObjectByType<CharacterSystem>();
        if (characterSystem != null)
        {
            Debug.Log($"✅ Found CharacterSystem: {characterSystem.name}");

            // 4. Test Player Character
            var player = characterSystem.GetPlayerCharacter();
            if (player != null)
            {
                Debug.Log($"✅ Found player character: {player.name}");
                Debug.Log($"   Level: {player.level}");
                Debug.Log($"   Bits: {player.bits}");
                Debug.Log($"   Perk Points: {player.perkPoints}");

                // 5. Test Stats
                if (player.stats != null)
                {
                    Debug.Log($"✅ Player has stats system");
                    Debug.Log($"   Strength: {player.stats.GetTotalStat(StatType.Strength)}");
                    Debug.Log($"   Dexterity: {player.stats.GetTotalStat(StatType.Dexterity)}");
                    Debug.Log($"   Constitution: {player.stats.GetTotalStat(StatType.Constitution)}");
                }
                else
                {
                    Debug.LogError("❌ Player stats is null!");
                }

                // 6. Test Game Stats
                if (player.gameStats != null)
                {
                    Debug.Log($"✅ Player has game stats");
                    Debug.Log($"   Health: {player.gameStats.health}/{player.gameStats.maxHealth}");
                    Debug.Log($"   Energy: {player.gameStats.energy}/{player.gameStats.maxEnergy}");
                }
                else
                {
                    Debug.LogError("❌ Player gameStats is null!");
                }

                // 7. Test showing character sheet directly.  ShowCharacterSheet() does
                // not take a player argument; use the parameterless overload.
                Debug.Log("🔧 Attempting to show character sheet...");
                charSheet.ShowCharacterSheet();

            }
            else
            {
                Debug.LogError("❌ No player character found!");
            }
        }
        else
        {
            Debug.LogError("❌ No CharacterSystem found!");
        }

        // 8. Test MLPDatabase
        if (MLPDatabase.Instance != null)
        {
            Debug.Log($"✅ Found MLPDatabase, Initialized: {MLPDatabase.Instance.IsInitialized}");
            if (MLPDatabase.Instance.IsInitialized)
            {
                var perks = MLPDatabase.Instance.GetAllPerks();
                Debug.Log($"   Available perks: {perks.Count}");
            }
        }
        else
        {
            Debug.LogError("❌ No MLPDatabase found!");
        }

        Debug.Log("=== DEBUG COMPLETE ===");
    }
}