// CompositionRoot.cs — scene-aware wiring (prevents in-game UI from initializing in MainMenu/Creation)
// This keeps StoryManager and menus working, but blocks UIController / InventoryScreenController
// from wiring up outside the "Game" scene.

using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace MyGameNamespace
{
    [DefaultExecutionOrder(-1000)]
    public class CompositionRoot : MonoBehaviour
    {
        [Header("Logging")]
        [SerializeField] private bool verbose = true;

        private void Awake()
        {
            if (verbose) Debug.Log("[CompositionRoot] Awake()");
            SceneManager.sceneLoaded += OnSceneLoaded;
            WireSceneConsumers();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (verbose) Debug.Log($"[CompositionRoot] OnSceneLoaded: {scene.name}");
            WireSceneConsumers();
        }

        public void WireSceneConsumers()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            bool isGameScene = string.Equals(sceneName, "Game", StringComparison.OrdinalIgnoreCase);

            if (verbose) Debug.Log($"[CompositionRoot] Wiring for scene: {sceneName} (isGameScene={isGameScene})");

            // Find every MonoBehaviour (including inactive) so we can gate what initializes
#if UNITY_2023_1_OR_NEWER
            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#else
            var behaviours = FindObjectsOfType<MonoBehaviour>(true);
#endif
            foreach (var mb in behaviours)
            {
                if (mb == null) continue;
                var type = mb.GetType();
                string tn = type.Name;

                // Gate in-game HUD systems when we're NOT in the Game scene
                if (!isGameScene)
                {
                    if (IsInGameHUD(type))
                    {
                        // Hide any UIDocument so it doesn’t show or eat clicks in non-Game scenes
                        HideIfUIDocument(mb);
                        if (verbose) Debug.Log($"[CompositionRoot] Skipping Initialize() for {tn} in scene '{sceneName}'.");
                        continue;
                    }
                }

                // Allow everything else to run its Initialize() if it has one
                CallIfExists(mb, "Initialize");
            }
        }

        private static bool IsInGameHUD(Type t)
        {
            // Centralize types that should only be active in the Game scene
            // Add/Remove types here as needed for your project.
            return t.Name == "UIController" ||
                   t.Name == "InventoryScreenController" ||
                   t.Name == "CombatUIController" ||
                   t.Name == "MLPGameUI"; // MLPGameUI already self-hides, but gating is fine
        }

        private static void HideIfUIDocument(Component comp)
        {
            try
            {
                var doc = comp.GetComponent<UIDocument>();
                if (doc != null && doc.rootVisualElement != null)
                {
                    doc.rootVisualElement.style.display = DisplayStyle.None;
                    try { doc.rootVisualElement.pickingMode = PickingMode.Ignore; } catch { }
                }
            }
            catch { /* safe no-op */ }
        }

        public static void CallIfExists(object target, string methodName)
        {
            if (target == null) return;
            var t = target.GetType();
            var mi = t.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi == null) return;

            try
            {
                // Prefer StartCoroutine if it returns IEnumerator (like your UIController.InitializeCoroutine)
                if (typeof(System.Collections.IEnumerator).IsAssignableFrom(mi.ReturnType))
                {
                    var mb = target as MonoBehaviour;
                    if (mb != null)
                    {
                        var enumerator = mi.Invoke(target, null) as System.Collections.IEnumerator;
                        if (enumerator != null)
                        {
                            mb.StartCoroutine(enumerator);
                            return;
                        }
                    }
                }

                mi.Invoke(target, null);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CompositionRoot] Error invoking {t.Name}.{methodName}: {ex.Message}");
            }
        }
    }
}
