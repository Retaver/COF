using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Central manager for narrative content. Loads the main story JSON from Resources
/// or StreamingAssets, deserializes it into a StoryData object, and maintains
/// the current story node along with a simple flag system for conditional content.
/// </summary>
public partial class StoryManager : MonoBehaviour
{
    /// <summary>Singleton instance of the StoryManager.</summary>
    public static StoryManager Instance { get; private set; }

    /// <summary>The deserialized story data. Contains all nodes and metadata.</summary>
    public StoryData Data;
    /// <summary>The currently active story node.</summary>
    public StoryNode CurrentNode;

    // Internal flag store for story conditions.
    private readonly Dictionary<string, string> storyFlags = new();

    private void Awake()
    {
        // Enforce singleton pattern.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load story on first instantiation
        if (Data == null) LoadStoryData();
        // Initialize current node
        if (CurrentNode == null && Data != null) SetCurrentNode(Data.GetStartNode());
    }

    /// <summary>
    /// Attempts to load the main story JSON from several locations. The order of
    /// preference is: Resources/Data/main_story, Resources/main_story, then
    /// StreamingAssets/Data/main_story.json. If no file is found, logs an error.
    /// </summary>
    public void LoadStoryData()
    {
        // First try: Resources/Data/main_story (without extension)
        if (TryLoadFromResources("Data/main_story", out var json))
        {
            Apply(json);
            return;
        }

        // Fallback 1: Resources/main_story
        if (TryLoadFromResources("main_story", out json))
        {
            Apply(json);
            return;
        }

        // Fallback 2: StreamingAssets/Data/main_story.json
        if (TryLoadFromStreamingAssets("Data/main_story.json", out json))
        {
            Apply(json);
            return;
        }

        Debug.LogError(
            "[StoryManager] Could not find main_story.json in Resources/Data, Resources, or StreamingAssets/Data.");
    }

    /// <summary>
    /// Loads a text asset from the Resources folder. Returns true if found and
    /// sets the JSON string.
    /// </summary>
    /// <param name="resourcePath">Path within Resources (without extension).</param>
    /// <param name="json">Output JSON text if the asset is found.</param>
    private bool TryLoadFromResources(string resourcePath, out string json)
    {
        json = null;
        var tx = Resources.Load<TextAsset>(resourcePath);
        if (tx == null) return false;
        json = tx.text;
        return true;
    }

    /// <summary>
    /// Loads a file from StreamingAssets. Returns true if found and sets the JSON string.
    /// </summary>
    /// <param name="relative">Relative path under StreamingAssets.</param>
    /// <param name="json">Output JSON text if the file exists.</param>
    private bool TryLoadFromStreamingAssets(string relative, out string json)
    {
        json = null;
        var full = Path.Combine(Application.streamingAssetsPath, relative);
        if (!File.Exists(full)) return false;
        json = File.ReadAllText(full);
        return true;
    }

    /// <summary>
    /// Parses JSON into StoryData and updates the Data property. Logs warnings
    /// if there are no nodes or if parsing fails.
    /// </summary>
    /// <param name="json">The raw JSON text.</param>
    private void Apply(string json)
    {
        try
        {
            var data = JsonUtility.FromJson<StoryData>(json);
            if (data == null || data.nodes == null || data.nodes.Count == 0)
            {
                Debug.LogError("[StoryManager] main_story.json parsed but contains no nodes.");
                return;
            }
            Data = data;
            Debug.Log(
                $"[StoryManager] Loaded {Data.nodes.Count} nodes. Start: '{Data.start_node_id}'.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[StoryManager] JSON parse error: {ex.Message}");
        }
    }

    /// <summary>
    /// Sets the current story node and raises the corresponding event.
    /// </summary>
    /// <param name="node">The node to set.</param>
    public void SetCurrentNode(StoryNode node)
    {
        CurrentNode = node;
        if (node == null) return;
        GameEventSystem.Instance?.RaiseStoryNodeChanged(node);
    }

    /// <summary>
    /// Resolves the next node from a choice. Prioritizes targetNodeId then
    /// nextNodeId then nextId. Logs a warning if none are defined.
    /// </summary>
    public void Choose(StoryChoice choice)
    {
        if (choice == null) return;
        var next = !string.IsNullOrEmpty(choice.targetNodeId)
            ? choice.targetNodeId
            : !string.IsNullOrEmpty(choice.nextNodeId)
                ? choice.nextNodeId
                : choice.nextId;
        if (!string.IsNullOrEmpty(next))
        {
            GoToNode(next);
        }
        else
        {
            Debug.LogWarning("[StoryManager] Choice has no next node id.");
        }
    }

    /// <summary>
    /// Navigates to the node by ID. Logs an error if the node is not found.
    /// </summary>
    public void GoToNode(string nodeId)
    {
        var node = Data?.GetNodeById(nodeId);
        if (node == null)
        {
            Debug.LogError($"[StoryManager] Node '{nodeId}' not found.");
            return;
        }
        SetCurrentNode(node);
    }

    /// <summary>
    /// Sets a flag to a value (default true) and raises the game flag event.
    /// </summary>
    public void SetFlag(string name, string value)
    {
        if (!string.IsNullOrEmpty(name)) storyFlags[name] = value ?? "true";
        GameEventSystem.Instance?.RaiseGameFlagSet(name, value ?? "true");
    }

    /// <summary>
    /// Retrieves a flag value or null if the flag does not exist.
    /// </summary>
    public string GetFlag(string name) =>
        string.IsNullOrEmpty(name)
            ? null
            : storyFlags.TryGetValue(name, out var v)
                ? v
                : null;

    /// <summary>
    /// Clears flags and resets the story to the first node.
    /// </summary>
    public void ResetStory()
    {
        storyFlags.Clear();
        CurrentNode = null;
        if (Data == null) LoadStoryData();
        if (Data != null) SetCurrentNode(Data.GetStartNode());
    }

    /// <summary>
    /// Stub method for future integration. Accepts a player object but does
    /// nothing by default.
    /// </summary>
    public void SetPlayer(object player) { }
}