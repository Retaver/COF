using UnityEngine;

// Call this from your story runner when applying effects from JSON choices.
public static class StoryEffectDispatcher
{
    // Minimal signature matching your JSON fields
    public static void Dispatch(string type, string target, int value = 0, string stringValue = null)
    {
        if (string.IsNullOrEmpty(type)) return;

        switch (type.ToLowerInvariant())
        {
            // This is the important mapping for your main_story.json
            case "combat":
                if (GameEventSystem.Instance == null)
                {
                    Debug.LogWarning("[StoryEffectDispatcher] GameEventSystem.Instance is null; cannot raise combat.");
                    return;
                }
                if (string.IsNullOrWhiteSpace(target))
                {
                    Debug.LogWarning("[StoryEffectDispatcher] Combat effect missing target id; defaulting to 'Hostile'.");
                    target = "Hostile";
                }
                GameEventSystem.Instance.RaiseCombatRequested(target);
                break;

            // Examples for other types you already use in the JSON
            case "personality":
                // TODO: Apply your personality changes here.
                // Example: PersonalitySystem.Apply(target, value);
                break;

            case "flag":
                // TODO: Set a story flag using your own flag system.
                // Example: StoryFlags.Set(target, stringValue ?? "true");
                break;

            case "item":
                // TODO: Add item to inventory.
                // Example: Inventory.Add(target, value);
                break;

            default:
                Debug.Log($"[StoryEffectDispatcher] Unhandled effect type: {type} ({target}, {value}, {stringValue})");
                break;
        }
    }
}