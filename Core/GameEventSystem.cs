// Canonical GameEventSystem
using System;
using UnityEngine;

public class GameEventSystem : MonoBehaviour
{
    public static GameEventSystem Instance { get; private set; }

    public Action<StoryNode> OnStoryNodeChanged;
    public Action<StoryChoice> OnStoryChoiceMade;

    public Action OnInventoryChanged;
    public Action<object> OnInventoryChangedObj;

    public Action OnPlayerStatsChanged;
    public Action<object> OnPlayerStatsChangedObj;

    public Action<object> OnPlayerCharacterChanged;

    public Action<string, string> OnGameFlagSet;

    public Action<string> OnCombatRequested;

    // === Additional events ===
    /// <summary>
    /// Invoked when combat begins. Provides a data object containing the enemy ID.
    /// Useful for updating combat UIs and portraits.
    /// </summary>
    public Action<CombatStartData> OnCombatStarted;

    /// <summary>
    /// Invoked when an enemy is defeated. Provides the enemy ID and the experience awarded.
    /// </summary>
    public Action<string, int> OnEnemyDefeated;

    /// <summary>
    /// Invoked when the player levels up. Provides the new level.
    /// </summary>
    public Action<int> OnPlayerLeveledUp;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static GameEventSystem GetOrCreate()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("GameEventSystem");
        return go.AddComponent<GameEventSystem>();
    }

    public void RaiseStoryNodeChanged(StoryNode node) => OnStoryNodeChanged?.Invoke(node);
    public void RaiseStoryChoiceMade(StoryChoice choice) => OnStoryChoiceMade?.Invoke(choice);

    public void RaiseInventoryChanged() => OnInventoryChanged?.Invoke();
    public void RaiseInventoryChanged(object payload) => OnInventoryChangedObj?.Invoke(payload);

    public void RaisePlayerStatsChanged() => OnPlayerStatsChanged?.Invoke();
    public void RaisePlayerStatsChanged(object payload) => OnPlayerStatsChangedObj?.Invoke(payload);

    public void RaisePlayerCharacterChanged(object payload) => OnPlayerCharacterChanged?.Invoke(payload);

    public void RaiseGameFlagSet(string name, string value) => OnGameFlagSet?.Invoke(name, value);

    public void RaiseCombatRequested(string enemyId) => OnCombatRequested?.Invoke(enemyId);

    // ----------- New raise methods -----------
    public void RaiseCombatStarted(CombatStartData data) => OnCombatStarted?.Invoke(data);
    public void RaiseEnemyDefeated(string enemyId, int xp) => OnEnemyDefeated?.Invoke(enemyId, xp);
    public void RaisePlayerLeveledUp(int newLevel) => OnPlayerLeveledUp?.Invoke(newLevel);
}
