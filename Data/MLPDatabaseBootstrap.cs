// MLPDatabaseBootstrap.cs
// Ensures a single, persistent MLPDatabase exists during *Play Mode* only.
// Never creates/keeps objects during editor-time script reloads.

using UnityEngine;

public static class MLPDatabaseBootstrap
{
    // Run early in runtime (Play Mode) only.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    private static void EnsureBeforeSplash() => EnsureInternal();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureBeforeScene() => EnsureInternal();

    /// <summary>Manual entry point if another script wants to ensure at runtime.</summary>
    public static void EnsureNow() => EnsureInternal();

    private static void EnsureInternal()
    {
        // IMPORTANT: never create in editor outside Play Mode (prevents DontDestroyOnLoad exception)
        if (!Application.isPlaying)
            return;

        // Already alive?
        if (MLPDatabase.Instance != null)
            return;

        // See if one exists (even inactive)
#if UNITY_2023_1_OR_NEWER
        var existing = Object.FindFirstObjectByType<MLPDatabase>(FindObjectsInactive.Include);
#else
        var existing = Object.FindObjectOfType<MLPDatabase>(true);
#endif
        if (existing != null)
            return; // its Awake will handle singleton init

        // Create one and keep it across scenes (now safe: we're in Play Mode)
        var go = new GameObject("MLPDatabase (Auto)");
        go.AddComponent<MLPDatabase>();
        Object.DontDestroyOnLoad(go);

#if UNITY_EDITOR
        Debug.Log("[MLPDatabaseBootstrap] Auto-created persistent MLPDatabase (runtime).");
#endif
    }
}
