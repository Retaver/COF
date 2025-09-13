using UnityEngine;

/// <summary>
/// Ensures that the core combat systems (CombatManager and CombatEventBridge)
/// exist in the scene at runtime.  Some scenes may not include these
/// components by default, causing the "combat" StoryEffect to do nothing.
/// This bootstrap creates them on load if they are missing so that
/// GameEventSystem.RaiseCombatRequested will properly begin combat and
/// notify the combat UI.  Components are created with DontDestroyOnLoad
/// semantics so they persist across scene transitions.
/// </summary>
public static class CombatBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureCombatSystems()
    {
        // Ensure a CombatManager exists.  If none is found, create one on a
        // dedicated GameObject and mark it to persist between scenes.
        var existingManager = Object.FindAnyObjectByType<CombatManager>();
        if (existingManager == null)
        {
            var cmObj = new GameObject("CombatManager");
            existingManager = cmObj.AddComponent<CombatManager>();
            Object.DontDestroyOnLoad(cmObj);
        }

        // Ensure a CombatEventBridge exists.  This component listens for
        // GameEventSystem.OnCombatRequested and starts combat via the
        // CombatManager.  If missing, create it on a new GameObject and
        // mark it to persist across scenes.
        var existingBridge = Object.FindAnyObjectByType<CombatEventBridge>();
        if (existingBridge == null)
        {
            var bridgeObj = new GameObject("CombatEventBridge");
            existingBridge = bridgeObj.AddComponent<CombatEventBridge>();
            Object.DontDestroyOnLoad(bridgeObj);
        }
    }
}