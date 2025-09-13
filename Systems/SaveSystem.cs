using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    [SerializeField] private string saveDirectory = "Saves";
    [SerializeField] private bool verboseLogging = true;

    private string SavePath => Path.Combine(Application.persistentDataPath, saveDirectory);

    private void Awake()
    {
        EnsureDirectory();
    }

    public void Initialize()
    {
        EnsureDirectory();
    }

    private void EnsureDirectory()
    {
        try
        {
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
                if (verboseLogging)
                    Debug.Log($"[SaveSystem] Created save directory: {SavePath}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Failed to ensure save directory: {ex.Message}");
        }
    }

    // Save using save.saveName
    public bool SaveGame(GameSave save)
    {
        if (save == null || string.IsNullOrWhiteSpace(save.saveName))
        {
            Debug.LogError("[SaveSystem] SaveGame requires a GameSave with a saveName");
            return false;
        }
        return Save(save.saveName, save);
    }

    // Save by slot name
    public bool Save(string saveSlot, GameSave save)
    {
        if (string.IsNullOrWhiteSpace(saveSlot) || save == null)
        {
            Debug.LogError("[SaveSystem] Invalid save parameters");
            return false;
        }

        try
        {
            EnsureDirectory();
            string filePath = Path.Combine(SavePath, $"{saveSlot}.json");

            // Optional: stamp time
            try
            {
                // If GameSave has these fields, set them (ignore if not present)
                var type = save.GetType();
                var savedAtUtcField = type.GetField("savedAtUtc");
                if (savedAtUtcField != null)
                    savedAtUtcField.SetValue(save, DateTime.UtcNow.ToString("o"));
            }
            catch { /* ignore */ }

            string jsonData = JsonUtility.ToJson(save, true);
            File.WriteAllText(filePath, jsonData);

            if (verboseLogging)
                Debug.Log($"[SaveSystem] Saved slot '{saveSlot}' to {filePath}");

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Failed to save '{saveSlot}': {ex.Message}");
            return false;
        }
    }

    public bool TryLoad(string saveSlot, out GameSave save)
    {
        save = null;

        if (string.IsNullOrWhiteSpace(saveSlot))
        {
            Debug.LogError("[SaveSystem] Invalid save slot");
            return false;
        }

        try
        {
            string filePath = Path.Combine(SavePath, $"{saveSlot}.json");
            if (!File.Exists(filePath))
            {
                if (verboseLogging)
                    Debug.Log($"[SaveSystem] Save '{saveSlot}' not found");
                return false;
            }

            string jsonData = File.ReadAllText(filePath);
            save = JsonUtility.FromJson<GameSave>(jsonData);

            if (verboseLogging)
                Debug.Log($"[SaveSystem] Loaded save '{saveSlot}'");

            return save != null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Failed to load '{saveSlot}': {ex.Message}");
            return false;
        }
    }

    public GameSave LoadGame(string saveSlot)
    {
        return TryLoad(saveSlot, out var save) ? save : null;
    }

    public bool HasSave(string saveSlot)
    {
        if (string.IsNullOrWhiteSpace(saveSlot)) return false;
        string filePath = Path.Combine(SavePath, $"{saveSlot}.json");
        return File.Exists(filePath);
    }

    // Returns true if a file was deleted
    public bool DeleteSave(string saveName)
    {
        if (string.IsNullOrWhiteSpace(saveName)) return false;

        try
        {
            string filePath = Path.Combine(SavePath, $"{saveName}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                if (verboseLogging)
                    Debug.Log($"[SaveSystem] Deleted save '{saveName}'");
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Failed to delete '{saveName}': {ex.Message}");
        }

        return false;
    }

    // Matches PauseMenuController usage (with confirm bool)
    public void ClearAllSaves(bool confirm)
    {
        if (!confirm)
        {
            if (verboseLogging)
                Debug.LogWarning("[SaveSystem] ClearAllSaves called without confirmation");
            return;
        }
        ClearAllSaves();
    }

    // No-arg helper
    public void ClearAllSaves()
    {
        try
        {
            if (!Directory.Exists(SavePath)) return;

            string[] saveFiles = Directory.GetFiles(SavePath, "*.json");
            foreach (string file in saveFiles)
            {
                File.Delete(file);
            }

            if (verboseLogging)
                Debug.Log($"[SaveSystem] Cleared {saveFiles.Length} save files");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Failed to clear saves: {ex.Message}");
        }
    }

    // List<GameSave> version to match PauseMenuController
    public List<GameSave> GetAllSaves()
    {
        var saves = new List<GameSave>();

        try
        {
            if (!Directory.Exists(SavePath)) return saves;

            string[] files = Directory.GetFiles(SavePath, "*.json");
            foreach (string file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    var save = JsonUtility.FromJson<GameSave>(json);
                    if (save != null)
                    {
                        // If filename has the slot, ensure saveName is set
                        if (string.IsNullOrWhiteSpace(save.saveName))
                            save.saveName = Path.GetFileNameWithoutExtension(file);

                        saves.Add(save);
                    }
                }
                catch (Exception inner)
                {
                    Debug.LogWarning($"[SaveSystem] Skipped bad save '{file}': {inner.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveSystem] Failed to enumerate saves: {ex.Message}");
        }

        return saves;
    }
}