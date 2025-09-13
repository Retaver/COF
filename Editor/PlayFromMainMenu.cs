// PlayFromMainMenu.cs — robust start scene picker (no hardcoded path)
// - Finds any scene named "MainMenu" anywhere in Assets
// - Sets EditorSceneManager.playModeStartScene accordingly
// - Optional menu to put MainMenu first in Build Settings

#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayFromMainMenu
{
    static PlayFromMainMenu()
    {
        EnsurePlayFromMainMenu(auto: true);
    }

    [MenuItem("Tools/Scenes/Set Play From MainMenu")]
    public static void SetPlayFromMainMenuMenu() => EnsurePlayFromMainMenu(auto: false);

    [MenuItem("Tools/Scenes/Put MainMenu first in Build Settings")]
    public static void PutMainMenuFirstInBuild()
    {
        var path = GetMainMenuScenePath();
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("[PlayFromMainMenu] No scene named 'MainMenu' found to add/move in Build Settings.");
            return;
        }

        var scenes = EditorBuildSettings.scenes.ToList();
        int idx = scenes.FindIndex(s => s.path.Equals(path, StringComparison.OrdinalIgnoreCase));
        if (idx >= 0)
        {
            var item = scenes[idx];
            scenes.RemoveAt(idx);
            scenes.Insert(0, item);
        }
        else
        {
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = scenes.ToArray();
        Debug.Log($"[PlayFromMainMenu] '{path}' set to Build Settings index 0.");
    }

    private static void EnsurePlayFromMainMenu(bool auto)
    {
        var path = GetMainMenuScenePath();
        if (!string.IsNullOrEmpty(path))
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            if (sceneAsset != null)
            {
                EditorSceneManager.playModeStartScene = sceneAsset;
                if (!auto) Debug.Log($"[PlayFromMainMenu] PlayMode start scene set to '{path}'.");
                return;
            }
        }

        // Fallback: first enabled scene in Build Settings
        var buildScenes = EditorBuildSettings.scenes;
        if (buildScenes != null && buildScenes.Length > 0)
        {
            var first = buildScenes.FirstOrDefault(s => s.enabled);
            if (first != null)
            {
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(first.path);
                EditorSceneManager.playModeStartScene = sceneAsset;
                Debug.LogWarning($"[PlayFromMainMenu] Couldn’t find a scene named 'MainMenu'. Using first build scene: {first.path}");
                return;
            }
        }

        // Last fallback: default behavior (current open scene)
        EditorSceneManager.playModeStartScene = null;
        Debug.LogWarning("[PlayFromMainMenu] No scenes in Build Settings. Add scenes via File ▸ Build Settings.");
    }

    private static string GetMainMenuScenePath()
    {
        // Prefer exact “MainMenu.unity”, but accept any scene with name MainMenu
        var guids = AssetDatabase.FindAssets("t:Scene MainMenu");
        foreach (var guid in guids)
        {
            var p = AssetDatabase.GUIDToAssetPath(guid);
            if (p.EndsWith("/MainMenu.unity", StringComparison.OrdinalIgnoreCase) ||
                System.IO.Path.GetFileNameWithoutExtension(p).Equals("MainMenu", StringComparison.OrdinalIgnoreCase))
            {
                return p;
            }
        }
        // fallback to first match if any
        return guids.Length > 0 ? AssetDatabase.GUIDToAssetPath(guids[0]) : null;
    }
}
#endif
