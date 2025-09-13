using UnityEngine;

public class CombatEventBridge : MonoBehaviour
{
    [SerializeField] private CombatManager combatManager;

    private bool subscribed;

    private void Awake()
    {
        if (combatManager == null) combatManager = FindFirstObjectByType<CombatManager>();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Update()
    {
        // Lazy subscribe in case GameEventSystem gets created later
        if (!subscribed) TrySubscribe();
    }

    private void OnDisable()
    {
        if (GameEventSystem.Instance != null && subscribed)
        {
            // Unsubscribe using the +=/-= operator because OnCombatRequested is a plain Action<string>
            GameEventSystem.Instance.OnCombatRequested -= OnCombatRequested;
        }
        subscribed = false;
    }

    private void TrySubscribe()
    {
        if (subscribed) return;
        if (GameEventSystem.Instance == null) return;

        // Subscribe using the += operator because OnCombatRequested is a plain Action<string>
        GameEventSystem.Instance.OnCombatRequested += OnCombatRequested;
        subscribed = true;
        Debug.Log("[CombatEventBridge] Subscribed to OnCombatRequested");
    }

    private void OnCombatRequested(string encounterId)
    {
        if (combatManager == null)
        {
            Debug.LogWarning("[CombatEventBridge] No CombatManager found.");
            return;
        }
        var id = string.IsNullOrWhiteSpace(encounterId) ? "Hostile" : encounterId;
        Debug.Log($"[CombatEventBridge] Combat requested: '{id}'");
        // Begin combat on the manager.  In a complete implementation this would
        // initialize the encounter and switch to the combat UI.  For now it
        // simply logs the message.
        combatManager.BeginCombat(id);
        // Notify the game event system that combat has started so UIs can
        // update player/enemy portraits.  The data includes the enemy id.
        if (GameEventSystem.Instance != null)
        {
            var data = new CombatStartData { enemyId = id };
            GameEventSystem.Instance.RaiseCombatStarted(data);

            // Temporary stub: immediately award experience as if the enemy were defeated.
            // In a full implementation, this should only be called after the battle ends.
            const int xpAward = 20;
            GameEventSystem.Instance.RaiseEnemyDefeated(id, xpAward);
        }
    }

    // Optional helpers if you want to call this directly from your story runner
    public void TriggerCombatFromStory(string encounterId) => OnCombatRequested(encounterId);
    public void TriggerCombatFromStory() => OnCombatRequested("Hostile");
}